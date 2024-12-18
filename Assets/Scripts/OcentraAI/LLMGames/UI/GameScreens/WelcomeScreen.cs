using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class WelcomeScreen : UIScreen<WelcomeScreen>
    {

        [Required] public GameMode GameMode => GameManager.Instance.GameMode;
        [ShowInInspector] [Required] public GameObject RulesPanel { get; private set; }

        [ShowInInspector] [Required] public Button ShowRulesButton { get; private set; }

        [ShowInInspector] [Required] public Button StartGameButton { get; private set; }

        [ShowInInspector] [Required] public Button QuitGameButton { get; private set; }

        [ShowInInspector] [Required] public TextMeshProUGUI WelcomeText { get; private set; }

        [ShowInInspector] [Required] public TextMeshProUGUI RulesText { get; private set; }

        [ShowInInspector] [Required] public TextMeshProUGUI DescriptionText { get; private set; }

        [ShowInInspector] [Required] public TextMeshProUGUI TipsText { get; private set; }


        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            InitReferences();
        }


        private void InitReferences()
        {
            RulesPanel = transform.FindChildRecursively<Transform>(nameof(RulesPanel)).gameObject;
            ShowRulesButton = transform.FindChildRecursively<Button>(nameof(ShowRulesButton));
            StartGameButton = transform.FindChildRecursively<Button>(nameof(StartGameButton));
            QuitGameButton = transform.FindChildRecursively<Button>(nameof(QuitGameButton));
            WelcomeText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(WelcomeText));
            RulesText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(RulesText));
            DescriptionText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(DescriptionText));
            TipsText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(TipsText));
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeWelcomeScreen();
        }

        private void InitializeWelcomeScreen()
        {
            RulesPanel.SetActive(false);
            StartGameButton.interactable = true;

            WelcomeText.text = "Welcome to Three Card Brag!";
            RulesText.text = GameMode.GameRules.Player;
            DescriptionText.text = GameMode.GameDescription.Player;
            TipsText.text = GameMode.StrategyTips.Player;

            ShowRulesButton.onClick.AddListener(ShowRules);
            StartGameButton.onClick.AddListener(StartGame);
            QuitGameButton.onClick.AddListener(QuitGame);
        }

        private void ShowRules()
        {
            RulesPanel.SetActive(true);
            PlaySelectionSound();
        }

        private void StartGame()
        {
            PlaySelectionSound();
            StartCoroutine(StartGameCoroutine());
        }

        private IEnumerator StartGameCoroutine()
        {
            yield return StartCoroutine(HideScreenCoroutine());
            //await GameManager.Instance.StartNewGameAsync();
        }

        public override void QuitGame()
        {
            base.QuitGame();
        }

        public override void OnHideScreen(bool first)
        {
            base.OnHideScreen(first);
            RulesPanel.SetActive(false);
        }

        public override void PlaySelectionSound()
        {
            // todo: Implement  sound playing logic here
            // For example:
            // AudioManager.Instance.PlaySound("ButtonClick");
        }


        public override void PlayBackGroundSound()
        {
            // todo :Implement back sound logic here
        }
    }
}