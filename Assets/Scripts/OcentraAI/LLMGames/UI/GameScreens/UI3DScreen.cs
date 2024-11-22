using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Extensions;
using UnityEngine;
using Sirenix.Utilities;
using Object = UnityEngine.Object;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace OcentraAI.LLMGames.Screens3D
{
    public interface IUIScreen
    {
        void ShowScreen();
        void HideScreen();
        void ToggleScreen();
        bool IsScreenInstanceVisible();
        bool IsInitialized { get; }
        string ScreenId { get; }
    }

    [Serializable]
    public abstract class UI3DScreen : SerializedMonoBehaviour, IUIScreen
    {
        public GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        public virtual string ScreenName { get; protected set; }
        [ShowInInspector] public static Dictionary<Type, UI3DScreen> RegisteredScreens { get; protected set; } = new Dictionary<Type, UI3DScreen>();
        [ShowInInspector] public static Stack<ScreenHistoryEntry> ScreenHistory { get; protected set; } = new Stack<ScreenHistoryEntry>();

        protected bool FirstShow { get; set; } = true;
        protected bool FirstHide { get; set; } = true;

        [SerializeField][Required] protected Transform MainPanel3D;

        [SerializeField] private bool isInitialized;
        public bool IsInitialized => isInitialized;

        [ShowInInspector] public bool Interactable { get; protected set; }
        [ShowInInspector] public bool IsFocus { get; protected set; }
        public bool StartEnabled;

        [ShowInInspector, ReadOnly] private bool isTransitioning;
        [ShowInInspector, ReadOnly] public string ScreenId { get; private set; }


        private TaskCompletionSource<bool> InitTaskSource { get; set; } = new TaskCompletionSource<bool>();

        [ShowInInspector, ReadOnly] private HashSet<Delegate> EventSubscriptions { get; set; } = new HashSet<Delegate>();

        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                isInitialized = false;
                Init(StartEnabled);
            }
        }



        protected virtual void OnEnable()
        {
            SubscribeToEvents();
            Init(StartEnabled);
        }

        protected virtual void OnDisable()
        {
            isInitialized = false;
        }

        protected virtual void OnDestroy()
        {
            try
            {
                UnregisterScreen();
                UnsubscribeFromAllEvents();
            }
            catch (Exception e)
            {
                GameLoggerScriptable.LogError($"Error during {GetType().Name} cleanup: {e}", null);
            }
        }

        protected virtual void SubscribeToEvents()
        {
            SafeSubscribe<ShowScreenEvent>(OnShowScreenEvent);
            SafeSubscribe<HideScreenEvent>(OnHideScreenEvent);
        }

        protected virtual void UnsubscribeFromEvents()
        {
            SafeUnsubscribe<ShowScreenEvent>(OnShowScreenEvent);
            SafeUnsubscribe<HideScreenEvent>(OnHideScreenEvent);
        }

        protected void SafeSubscribe<T>(Action<T> handler) where T : IEventArgs
        {
            if (handler == null)
                return;

            if (!EventSubscriptions.Contains(handler))
            {
                EventBus.Instance.Subscribe(handler);
                EventSubscriptions.Add(handler);
            }
        }

        protected void SafeUnsubscribe<T>(Action<T> handler) where T : IEventArgs
        {
            if (handler == null)
                return;

            if (EventSubscriptions.Contains(handler))
            {
                EventBus.Instance.Unsubscribe(handler);
                EventSubscriptions.Remove(handler);
            }
        }

        private void UnsubscribeFromAllEvents()
        {
            foreach (Delegate subscription in EventSubscriptions)
            {
                if (subscription is Action<IEventArgs> handler)
                {
                    EventBus.Instance.Unsubscribe(handler);
                }
            }
            EventSubscriptions.Clear();
        }

        private void OnShowScreenEvent(ShowScreenEvent evt)
        {
            if (evt.ScreenToShow == ScreenName)
            {
                ShowScreen();
            }
        }

        private void OnHideScreenEvent(HideScreenEvent evt)
        {
            if (evt.ScreenToHide == ScreenName)
            {
                HideScreen();
            }
        }

        protected virtual void Init(bool startEnabled)
        {
            if (InitTaskSource == null || InitTaskSource.Task.IsCompleted)
            {
                InitTaskSource = new TaskCompletionSource<bool>();
            }

            if (isInitialized)
            {
                InitTaskSource.SetResult(true);
                return;
            }



            try
            {
                if (!EnsureMainPanel(startEnabled))
                {
                    isInitialized = false;
                    InitTaskSource.SetResult(isInitialized);
                    GameLoggerScriptable.LogError($"MainPanel3D For {GetType().Name} Not Found", null);
                    return;
                }

                RegisterScreen();
                isInitialized = true;
                InitTaskSource.SetResult(true);
            }
            catch (Exception e)
            {
                GameLoggerScriptable.LogError($"Error during {GetType().Name} Init: {e}", null);
                isInitialized = false;
                InitTaskSource.SetException(e);

            }


        }

        public async UniTask WaitForInitializationAsync()
        {
            Init(StartEnabled);
            await InitTaskSource.Task.AsUniTask();
        }

        private bool EnsureMainPanel(bool startEnabled)
        {
            StartEnabled = startEnabled;
            if (!transform.gameObject.activeInHierarchy)
            {
                transform.gameObject.SetActive(true);
            }

            MainPanel3D = transform.FindChildRecursively(nameof(MainPanel3D));

            if (MainPanel3D != null)
            {
                MainPanel3D.gameObject.SetActive(startEnabled);
            }

            if (startEnabled)
            {
                ShowScreen();
            }

            return MainPanel3D != null;
        }

        protected virtual void RegisterScreen()
        {
            Type screenType = GetType();
            RegisteredScreens.TryAdd(screenType, this);
        }

        protected virtual void UnregisterScreen()
        {
            Type screenType = GetType();
            RegisteredScreens.Remove(screenType, out _);
        }

        public static T GetScreen<T>() where T : UI3DScreen
        {
            return RegisteredScreens.TryGetValue(typeof(T), out UI3DScreen screen) ? screen as T : null;
        }

        [Button]
        public void PrintScreens()
        {
            if (RegisteredScreens.Count == 0)
            {
                Debug.Log("No registered screens found.");
                return;
            }

            string screensList = "Registered Screens:\n";
            foreach (UI3DScreen screen in RegisteredScreens.Values)
            {
                screensList += $"{screen.GetType().Name} (ID: {screen.ScreenId})\n";
            }

            Debug.Log(screensList);
        }


        public virtual void ShowScreen()
        {
            if (isTransitioning || !VerifyCanShow()) return;

            isTransitioning = true;
            try
            {
                if (MainPanel3D != null)
                {
                    MainPanel3D.gameObject.SetActive(true);
                    if (ScreenHistory.Count == 0 || ScreenHistory.Peek().Screen != this)
                    {
                        ScreenHistory.Push(new ScreenHistoryEntry(this));
                    }
                    SetFocus(true);
                    FirstShow = true;
                    PlayNavigationSound();
                }
                else
                {
                    GameLoggerScriptable.LogError("MainPanel3D is null. Cannot show screen.", null);
                }
            }
            finally
            {
                isTransitioning = false;
            }
        }

        public virtual void HideScreen()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            try
            {
                if (MainPanel3D != null)
                {
                    SetFocus(false);
                    MainPanel3D.gameObject.SetActive(false);

                    if (ScreenHistory.Count > 0 && ScreenHistory.Peek().Screen == this)
                    {
                        ScreenHistory.Pop();
                    }
                    FirstHide = false;
                }
                else
                {
                    GameLoggerScriptable.LogError("MainPanel3D is null. Cannot hide screen.", null);
                }
            }
            finally
            {
                isTransitioning = false;
            }
        }

        public virtual void ToggleScreen()
        {
            if (IsScreenInstanceVisible())
            {
                HideScreen();
            }
            else
            {
                ShowScreen();
            }
        }

        public virtual void GoBack()
        {
            if (ScreenHistory.Count > 1)
            {
                HideScreen();
                ScreenHistoryEntry previousEntry = ScreenHistory.Peek();
                previousEntry.Screen.ShowScreen();
            }
        }
        public virtual void ShowLastScreen()
        {
            if (ScreenHistory.Count > 1)
            {
                HideScreen();
                ScreenHistory.Pop();

                ScreenHistoryEntry lastScreenEntry = ScreenHistory.Peek();
                lastScreenEntry.Screen.ShowScreen();
            }
            else
            {
                GameLoggerScriptable.LogWarning("No previous screen to show.", null);
            }
        }
        protected virtual bool VerifyCanShow()
        {
            return isInitialized && !isTransitioning;
        }

        public bool IsScreenInstanceVisible()
        {
            return MainPanel3D != null && MainPanel3D.gameObject.activeInHierarchy;
        }

        public virtual void SetFocus(bool isFocus)
        {
            Interactable = isFocus;
            IsFocus = isFocus;
        }

        public virtual void PlaySelectionSound() { }
        public virtual void PlayNavigationSound() { }
        public virtual void PlayBackGroundSound() { }

        public virtual void ResetScreenToStartState(bool cascade)
        {
            FirstShow = true;
            FirstHide = true;

            if (StartEnabled)
            {
                ShowScreen();
            }
            else
            {
                HideScreen();
            }

            if (cascade)
            {
                foreach (UI3DScreen screen in GetComponentsInChildren<UI3DScreen>(true))
                {
                    if (screen != this)
                    {
                        screen.ResetScreenToStartState(false);
                    }
                }
            }
        }





        protected virtual void Reset()
        {
            isInitialized = false;

#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying)
                {
                    Init(StartEnabled);
                    EditorUtility.SetDirty(this);
                }
            };
#endif
        }



       
    }

    [Serializable]
    public class ScreenHistoryEntry
    {
        public UI3DScreen Screen { get; }
        public string ScreenId { get; }
        public DateTime TimeStamp { get; }

        public ScreenHistoryEntry(UI3DScreen screen)
        {
            Screen = screen;
            ScreenId = screen.ScreenId;
            TimeStamp = DateTime.UtcNow;
        }
    }

    public abstract class UI3DScreen<T> : UI3DScreen where T : UI3DScreen<T>
    {
        private static volatile T instance;
        private static readonly object instanceLock = new object();
        private static bool applicationQuitting;

        public static T Instance
        {
            get
            {
                if (applicationQuitting)
                {
                    Debug.LogWarning($"Instance of {typeof(T).Name} requested after application quit. Returning null.");
                    return null;
                }

                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = GetScreen<T>();

                            if (instance == null)
                            {
                                instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);

                                if (instance == null)
                                {
                                    T[] allInstances = Resources.FindObjectsOfTypeAll<T>();
                                    if (allInstances.Length > 0)
                                    {
                                        instance = allInstances[0];
                                    }
                                }

                                if (instance == null)
                                {
                                    Debug.LogWarning($"No instance of {typeof(T).Name} found in the scene (including inactive objects).");
                                    return null;
                                }


                            }
                        }
                    }
                }
                else
                {
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(instance.gameObject);
                    }

                   
                }

                return instance;
            }
        }

        public static async UniTask<T> GetInstanceAsync()
        {
            if (applicationQuitting)
            {
                Debug.LogWarning($"Instance of {typeof(T).Name} requested after application quit. Returning null.");
                return null;
            }

            T foundInstance = Instance;

            if (foundInstance != null)
            {
                await foundInstance.WaitForInitializationAsync();
                return foundInstance;
            }

            return null;
        }

        public static bool HasInstance => instance != null;

        protected override void Awake()
        {
            if (!HasInstance)
            {
                instance = (T)this;
            }
            applicationQuitting = false;
            base.Awake();
        }

        public override string ScreenName => typeof(T).Name;


        protected override void RegisterScreen()
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = (T)this;
                   
                }
                else if (instance != this)
                {
                    GameLoggerScriptable.LogWarning($"Multiple instances of {typeof(T).Name} found. Destroying duplicate.", this);

                }
            }
            base.RegisterScreen();
        }



        protected virtual void OnApplicationQuit()
        {
            applicationQuitting = true;

        }
    }
}
