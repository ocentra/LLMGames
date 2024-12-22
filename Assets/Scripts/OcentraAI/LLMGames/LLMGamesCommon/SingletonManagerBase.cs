using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager
{
    public abstract class SingletonManagerBase<T> : SerializedMonoBehaviour,IEventHandler where T : Component
    {
        [Header("File Logging Settings")]
        [ShowInInspector] public bool ToEditor { get; set; } = true;
        [ShowInInspector] public bool ToFile { get; set; } = false;

        [ShowInInspector, Required] public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();

        private readonly UniTaskCompletionSource initializationSource = new UniTaskCompletionSource();
        protected UniTaskCompletionSource<bool> QuitCompletionSource = new UniTaskCompletionSource<bool>();
        private static bool isQuitting = false;
        public static T Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void SetupApplicationQuitHandler()
        {
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        private static bool OnApplicationWantsToQuit()
        {
            if (Instance == null)
            {
                CreateSingletonInstance();
            }

            SingletonManagerBase<T> singletonManager = Instance as SingletonManagerBase<T>;
            if (singletonManager != null && singletonManager.QuitCompletionSource == null && !isQuitting)
            {
                singletonManager.QuitCompletionSource = new UniTaskCompletionSource<bool>();
                isQuitting = true;
                singletonManager.HandleApplicationQuit(singletonManager.QuitCompletionSource).Forget();
                return false;
            }
            return true;
        }

        private async UniTaskVoid HandleApplicationQuit(UniTaskCompletionSource<bool> completionSource)
        {
            bool success = await ApplicationWantsToQuit();
            completionSource.TrySetResult(success);
            if (success)
            {
                Application.Quit();
            }
            else
            {
                isQuitting = false;
            }

            QuitCompletionSource = null;
        }

        public static T GetInstance()
        {
            CreateSingletonInstance();
            return Instance;
        }

        public async UniTask WaitForInitializationAsync()
        {
            await initializationSource.Task;
        }

        protected virtual void Awake()
        {
            CreateSingletonInstance();
            InitializeAsync().Forget();
        }

        protected virtual void OnValidate()
        {
            InitializeAsync().Forget();
        }

        public static void CreateSingletonInstance()
        {
            if (Instance == null)
            {
                Instance = FindFirstObjectByType<T>();
                if (Instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).Name);
                    Instance = gameObject.AddComponent<T>();
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        protected virtual async UniTask InitializeAsync()
        {
            if (initializationSource.Task.Status == UniTaskStatus.Pending)
            {
                await UniTask.SwitchToMainThread();

                initializationSource.TrySetResult();
            }

            await initializationSource.Task;
        }


        protected virtual void Start() { }

        protected virtual void OnEnable()
        {
            SubscribeToEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        public virtual void SubscribeToEvents()
        {
          

        }

        public virtual void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();

        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
            {
                OnApplicationWantsToQuit();
            }
        }

        protected virtual UniTask<bool> ApplicationWantsToQuit()
        {
            return UniTask.FromResult(true);
        }



        public virtual void Log(string message, Object context, bool toEditor = default, bool toFile = default)
        {
            if (toEditor == default)
            {
                toEditor = ToEditor;
            }

            if (toFile == default)
            {
                toFile = ToFile;
            }
            GlobalConfig<GameLoggerScriptable>.Instance?.Log(message, context, toEditor, toFile);
        }

        public virtual void LogWarning(string message, Object context, bool toEditor = default, bool toFile = default)
        {
            if (toEditor == default)
            {
                toEditor = ToEditor;
            }

            if (toFile == default)
            {
                toFile = ToFile;
            }
            GlobalConfig<GameLoggerScriptable>.Instance?.LogWarning(message, context, toEditor, toFile);
        }

        public virtual void LogError(string message, Object context, bool toEditor = default, bool toFile = default)
        {
            if (toEditor == default)
            {
                toEditor = ToEditor;
            }

            if (toFile == default)
            {
                toFile = ToFile;
            }
            GlobalConfig<GameLoggerScriptable>.Instance?.LogError(message, context, toEditor, toFile);
        }
    }
}