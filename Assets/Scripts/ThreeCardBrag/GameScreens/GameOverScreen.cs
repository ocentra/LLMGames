
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using ThreeCardBrag.Extensions;

namespace ThreeCardBrag.GameScreen
{
    public class GameOverScreen : BragScreen<GameOverScreen>
    {
        [ShowInInspector, Required]
        public TextMeshProUGUI GameOverText { get; private set; }

        [ShowInInspector, Required]
        public TextMeshProUGUI ScoreText { get; private set; }

        [ShowInInspector, Required]
        public Button PlayAgainButton { get; private set; }

        [ShowInInspector, Required]
        public Button MainMenuButton { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        void OnValidate()
        {
            InitReferences();
        }

        private void InitReferences()
        {
            GameOverText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(GameOverText));
            ScoreText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ScoreText));
            PlayAgainButton = transform.FindChildRecursively<Button>(nameof(PlayAgainButton));
            MainMenuButton = transform.FindChildRecursively<Button>(nameof(MainMenuButton));
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeGameOverScreen();
        }

        private void InitializeGameOverScreen()
        {
            PlayAgainButton.onClick.AddListener(PlayAgain);
            MainMenuButton.onClick.AddListener(ReturnToMainMenu);
            UpdateScore();
        }

        private void UpdateScore()
        {
            // Implement score update logic here
            ScoreText.text = $"Your Score: {GameManager.Instance.ScoreKeeper.HumanTotalWins}";
        }

        private void PlayAgain()
        {
            PlaySelectionSound();
            GameManager.Instance.StartNewGame();
            HideScreen();
        }

        private void ReturnToMainMenu()
        {
            PlaySelectionSound();
            BragScreen<WelcomeScreen>.ShowScreen();
            HideScreen();
        }
    }
}
