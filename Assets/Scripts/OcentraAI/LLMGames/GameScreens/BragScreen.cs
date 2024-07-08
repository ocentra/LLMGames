using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OcentraAI.LLMGames.UI;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.GameScreen
{
    public abstract class BragScreen : MonoBehaviour
    {
        public GameObject Panel;
        public bool StartEnabled;
        public bool IsFocus;
        public bool Interactable;

        [SerializeField] protected CanvasGroup CanvasGroup;
        [SerializeField] protected float FadeDuration = 0.5f;

        protected virtual void Awake()
        {
            if (CanvasGroup == null)
                CanvasGroup = GetComponent<CanvasGroup>();
            if (CanvasGroup == null)
                CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public virtual bool VerifyCanShow() => true;

        public virtual void OnShowScreen(bool first) { }

        public virtual void OnHideScreen(bool first) { }

        public virtual void OnScreenFocusChanged(bool focus) { }

        public virtual void OnScreenDestroy() { }

        public virtual void ResetScreenToStartState(bool cascade) { }

        public bool IsScreenInstanceVisible() => Panel.activeInHierarchy;

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

        protected virtual void SetupBragSelectables()
        {
            if (Panel != null)
            {
                var bragSelectables = Panel.GetComponentsInChildren<BragSelectable>(true);
                foreach (var bragSelectable in bragSelectables)
                {
                    bragSelectable.ParentScreen = this;
                }
            }
        }

        public virtual void PlaySelectionSound() { }

        public virtual void PlayNavigationSound() { }

        public virtual void PlayBackGroundSound() { }

        public virtual void QuitGame()
        {
            PlaySelectionSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        public IEnumerator ShowScreenCoroutine()
        {
            Panel.SetActive(true);
            yield return StartCoroutine(FadeCoroutine(true));
            SetFocus(true);
            OnShowScreen(FirstShow);
            FirstShow = false;
        }

        public IEnumerator HideScreenCoroutine()
        {
            SetFocus(false);
            yield return StartCoroutine(FadeCoroutine(false));
            Panel.SetActive(false);
            OnHideScreen(FirstHide);
            FirstHide = false;
        }

        public virtual void ShowScreenInstance() { }

        public virtual void HideScreenInstance() { }

        public virtual void ToggleScreenInstance() { }

        protected static bool FirstShow = true;
        protected static bool FirstHide = true;

        public virtual void SetFocus(bool isFocus)
        {
            Interactable = isFocus;
            IsFocus = isFocus;
            OnScreenFocusChanged(isFocus);
        }
    }

    public abstract class BragScreen<T> : BragScreen where T : BragScreen
    {
        public static T Instance { get; private set; }

        private static Stack<BragScreen> screenHistory = new Stack<BragScreen>();

        public static void DestroyScreen()
        {
            if (Instance != null)
            {
                Instance.OnScreenDestroy();
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        public static bool IsScreenVisible() => Instance != null && Instance.Panel.activeInHierarchy;

        public override void ShowScreenInstance() => ShowScreen();

        public override void HideScreenInstance() => HideScreen();

        public override void ToggleScreenInstance() => ToggleScreen();

        public static void ShowScreen(bool condition)
        {
            if (condition != IsScreenVisible())
            {
                if (condition)
                    ShowScreen();
                else
                    HideScreen();
            }
        }

        public static void ShowScreen()
        {
            if (Instance != null && Instance.VerifyCanShow())
            {
                Instance.StartCoroutine(Instance.ShowScreenCoroutine());
                screenHistory.Push(Instance);
            }
        }

        public static void HideScreen()
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(Instance.HideScreenCoroutine());
                screenHistory.Pop();
            }
        }

        public static void ToggleScreen()
        {
            if (IsScreenVisible())
                HideScreen();
            else
                ShowScreen();
        }

        public static void GoBack()
        {
            if (screenHistory.Count > 1)
            {
                HideScreen();
                BragScreen previousScreen = screenHistory.Peek();
                previousScreen.ShowScreenInstance();
            }
        }

        public override void ResetScreenToStartState(bool cascade)
        {
            FirstShow = true;
            FirstHide = true;

            if (StartEnabled)
                ShowScreen();
            else
                HideScreen();

            if (cascade)
            {
                foreach (var screen in GetComponentsInChildren<BragScreen>())
                {
                    if (screen != this)
                        screen.ResetScreenToStartState(false);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null)
            {
                Instance.gameObject.SetActive(false);
                Destroy(Instance.gameObject);
            }

            Instance = (T)(object)this;
            SetupBragSelectables();
            Instance.ResetScreenToStartState(false);
            AwakeOverride();
        }

        protected virtual void AwakeOverride() { }
    }
}