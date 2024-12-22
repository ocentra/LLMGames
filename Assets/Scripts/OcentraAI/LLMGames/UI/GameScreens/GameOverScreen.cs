using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;


namespace OcentraAI.LLMGames.Screens
{
    public class GameOverScreen : UI3DScreen<GameOverScreen>
    {
        [ShowInInspector] [Required] public TextMeshProUGUI GameOverText { get; private set; }

        [ShowInInspector] [Required] public TextMeshProUGUI ScoreText { get; private set; }

        [ShowInInspector] [Required] public Button PlayAgainButton { get; private set; }

        [ShowInInspector] [Required] public Button MainMenuButton { get; private set; }

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
            GameOverText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(GameOverText));
            ScoreText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ScoreText));
            PlayAgainButton = transform.FindChildRecursively<Button>(nameof(PlayAgainButton));
            MainMenuButton = transform.FindChildRecursively<Button>(nameof(MainMenuButton));
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
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
            // ScoreText.text = $"Your Score: {GameManager.Instance.ScoreKeeper.HumanTotalWins}";
        }

        private void PlayAgain()
        {
            PlaySelectionSound();
            EventBus.Instance.Publish(new PlayerActionStartNewGameEvent());
            HideScreen();
        }

        private void ReturnToMainMenu()
        {
            PlaySelectionSound();
            UI3DScreen<WelcomeScreen>.Instance.ShowScreen();
            HideScreen();
        }
    }
}