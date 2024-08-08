using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.Screens
{
    public abstract class UIScreen : SerializedMonoBehaviour
    {
        [Required] protected PlayerManager PlayerManager => PlayerManager.Instance;
        [Required] protected ScoreManager ScoreManager => ScoreManager.Instance;
        [Required] protected DeckManager DeckManager => DeckManager.Instance;
        [Required] protected TurnManager TurnManager => TurnManager.Instance;
        [Required] protected GameManager GameManager => GameManager.Instance;
        [Required] public GameMode GameMode => GameManager.GameMode;


        [Required]
        public GameObject MainPanel;
        public bool StartEnabled;
        public bool IsFocus;
        public bool Interactable;

        [SerializeField,Required] protected CanvasGroup CanvasGroup;
        [SerializeField] protected float FadeDuration = 0.5f;

        protected static readonly Stack<UIScreen> ScreenHistory = new Stack<UIScreen>();


        protected virtual void Awake()
        {
            Init();
        }

        public virtual void Init()
        {
            MainPanel = transform.FindChildRecursively<Transform>(nameof(MainPanel)).gameObject;


            if (CanvasGroup == null)
            {
                CanvasGroup = MainPanel.GetComponent<CanvasGroup>();
            }

            if (CanvasGroup == null)
            {
                CanvasGroup = MainPanel.AddComponent<CanvasGroup>();
            }
        }

        public virtual bool VerifyCanShow() => true;

        public virtual void OnShowScreen(bool first) { }

        public virtual void OnHideScreen(bool first) { }

        public virtual void OnScreenFocusChanged(bool focus) { }

        public virtual void OnScreenDestroy() { }

        public virtual void ResetScreenToStartState(bool cascade) { }

        public bool IsScreenInstanceVisible() => MainPanel.activeInHierarchy;

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
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public IEnumerator ShowScreenCoroutine()
        {
            MainPanel.SetActive(true);
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
            MainPanel.SetActive(false);
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
                HideScreen();
            else
                ShowScreen();
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

        protected static bool FirstShow = true;
        protected static bool FirstHide = true;

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
            base.Awake();
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = (T)(object)this;
            ResetScreenToStartState(false);
            AwakeOverride();
        }

        protected virtual void AwakeOverride() { }

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
                foreach (var screen in GetComponentsInChildren<UIScreen>())
                {
                    if (screen != this)
                        screen.ResetScreenToStartState(false);
                }
            }
        }
    }
}
