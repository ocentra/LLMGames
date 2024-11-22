using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.Screens
{
    public abstract class UIScreen : SerializedMonoBehaviour
    {
        protected static  Stack<UIScreen> ScreenHistory { get;  set; }= new Stack<UIScreen>();

        protected static bool FirstShow = true;
        protected static bool FirstHide = true;

        [SerializeField] [Required] protected CanvasGroup CanvasGroup;
        [SerializeField] protected float FadeDuration = 0.5f;
        public bool Interactable;
        public bool IsFocus;

        [Required] public Transform MainPanel;
        public bool StartEnabled;
       
        protected virtual void Awake()
        {
            Init();
           
        }
        protected virtual void OnValidate()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            SubscribeToEvents();
        }

        protected virtual void OnDisable()
        {

            UnsubscribeFromEvents();
        }

        protected virtual void SubscribeToEvents()
        {

        }

        protected virtual void UnsubscribeFromEvents()
        {

        }
        public virtual void Init()
        {
            transform.FindChildWithComponent(ref MainPanel, nameof(MainPanel));
            if (MainPanel != null)
            {
                if (CanvasGroup == null)
                {

                    CanvasGroup = MainPanel.GetComponent<CanvasGroup>();
                }

                if (CanvasGroup == null)
                {
                    CanvasGroup = MainPanel.gameObject.AddComponent<CanvasGroup>();
                }
            }

        }

        public static void ShowLastScreen()
        {
            if (ScreenHistory.Count > 1)
            {
                UIScreen currentScreen = ScreenHistory.Pop();
                currentScreen.HideScreen();

                UIScreen lastScreen = ScreenHistory.Peek();
                lastScreen.ShowScreen();
            }
            else if (ScreenHistory.Count == 1)
            {
                Debug.Log("Only one screen in history. Cannot show last screen.");
            }
            else
            {
                Debug.Log("No screens in history.");
            }
        }



        public virtual bool VerifyCanShow()
        {
            return true;
        }

        public virtual void OnShowScreen(bool first) { }

        public virtual void OnHideScreen(bool first) { }

        public virtual void OnScreenFocusChanged(bool focus) { }

        public virtual void OnScreenDestroy() { }

        public virtual void ResetScreenToStartState(bool cascade) { }

        public bool IsScreenInstanceVisible()
        {
            return MainPanel.gameObject.activeInHierarchy;
        }

        protected IEnumerator FadeCoroutine(bool fadeIn)
        {
            float elapsedTime = 0;
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;

            while (elapsedTime < FadeDuration)
            {
                elapsedTime += Time.deltaTime;
                CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / FadeDuration);
                yield return null;
            }

            CanvasGroup.alpha = endAlpha;
        }

        public virtual void PlaySelectionSound() { }

        public virtual void PlayNavigationSound() { }

        public virtual void PlayBackGroundSound() { }

        public virtual void QuitGame()
        {
            PlaySelectionSound();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public IEnumerator ShowScreenCoroutine()
        {
            MainPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCoroutine(true));
            SetFocus(true);
            OnShowScreen(FirstShow);
            FirstShow = false;
            ScreenHistory.Push(this);
        }

        public IEnumerator HideScreenCoroutine()
        {
            SetFocus(false);
            yield return StartCoroutine(FadeCoroutine(false));
            MainPanel.gameObject.SetActive(false);
            OnHideScreen(FirstHide);
            FirstHide = false;
            if (ScreenHistory.Count > 0)
            {
                ScreenHistory.Pop();
            }
        }

        public virtual void ShowScreen()
        {
            StartCoroutine(ShowScreenCoroutine());
        }

        public virtual void HideScreen()
        {
            StartCoroutine(HideScreenCoroutine());
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

        public void GoBack()
        {
            if (ScreenHistory.Count > 1)
            {
                HideScreen();
                UIScreen previousScreen = ScreenHistory.Peek();
                previousScreen.ShowScreen();
            }
        }

        public virtual void SetFocus(bool isFocus)
        {
            Interactable = isFocus;
            IsFocus = isFocus;
            OnScreenFocusChanged(isFocus);
        }
    }

    public abstract class UIScreen<T> : UIScreen where T : UIScreen
    {
        public static T Instance { get; private set; }

        protected override void Awake()
        {
            
            if (Instance == null)
            {
                Instance = (T)(object)this;
            }

            

            base.Awake();
            ResetScreenToStartState(false);
            AwakeOverride();

        }

        protected override void OnValidate()
        {
            if (Instance == null)
            {
                Instance = (T)(object)this;
            }

            base.OnValidate();
        }

        protected virtual void AwakeOverride() { }

        public override void ResetScreenToStartState(bool cascade)
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
                foreach (var screen in GetComponentsInChildren<UIScreen>(true))
                {
                    if (screen != this)
                    {
                        screen.ResetScreenToStartState(false);
                    }
                }
            }
        }
    }
}