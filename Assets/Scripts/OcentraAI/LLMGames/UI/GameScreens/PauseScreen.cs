using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class PauseScreen : UI3DScreen<PauseScreen>
    {
        [ShowInInspector][Required] public Button ResumeButton { get; private set; }

        [ShowInInspector][Required] public Button OptionsButton { get; private set; }

        [ShowInInspector][Required] public Button QuitToMainMenuButton { get; private set; }

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
            ResumeButton = transform.FindChildRecursively<Button>(nameof(ResumeButton));
            OptionsButton = transform.FindChildRecursively<Button>(nameof(OptionsButton));
            QuitToMainMenuButton = transform.FindChildRecursively<Button>(nameof(QuitToMainMenuButton));
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
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