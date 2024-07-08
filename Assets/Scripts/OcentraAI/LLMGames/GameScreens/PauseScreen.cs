using OcentraAI.LLMGames.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace OcentraAI.LLMGames.GameScreen
{
    public class PauseScreen : BragScreen<PauseScreen>
    {
        [ShowInInspector, Required]
        public Button ResumeButton { get; private set; }

        [ShowInInspector, Required]
        public Button OptionsButton { get; private set; }

        [ShowInInspector, Required]
        public Button QuitToMainMenuButton { get; private set; }

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
            ResumeButton = transform.FindChildRecursively<Button>(nameof(ResumeButton));
            OptionsButton = transform.FindChildRecursively<Button>(nameof(OptionsButton));
            QuitToMainMenuButton = transform.FindChildRecursively<Button>(nameof(QuitToMainMenuButton));
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializePauseScreen();
        }

        private void InitializePauseScreen()
        {
            ResumeButton.onClick.AddListener(Resume);
            OptionsButton.onClick.AddListener(ShowOptions);
            QuitToMainMenuButton.onClick.AddListener(QuitToMainMenu);
        }

        private void Resume()
        {
            PlaySelectionSound();
           // GameManager.Instance.ResumeGame();
            HideScreen();
        }

        private void ShowOptions()
        {
            PlaySelectionSound();
            // Implement showing options screen logic here
        }

        private void QuitToMainMenu()
        {
            PlaySelectionSound();
           // GameManager.Instance.QuitToMainMenu();
            HideScreen();
        }
    }
}
