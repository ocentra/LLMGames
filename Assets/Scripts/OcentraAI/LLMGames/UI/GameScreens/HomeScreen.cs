using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class HomeScreen : UIScreen<HomeScreen>
    {
        [ShowInInspector] [Required] public TextMeshProUGUI TitleText { get; private set; }

        [ShowInInspector] [Required] public TextMeshProUGUI TaglineText { get; private set; }

        [ShowInInspector] [Required] public Button ThreeCardBragButton { get; private set; }

        [ShowInInspector] [Required] public List<Button> OtherGameButtons { get; private set; } = new List<Button>();

        [ShowInInspector] [Required] public Button SettingsButton { get; private set; }

        [ShowInInspector] [Required] public Button QuitButton { get; private set; }

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
            TitleText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(TitleText));
            TaglineText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(TaglineText));
            ThreeCardBragButton = transform.FindChildRecursively<Button>(nameof(ThreeCardBragButton));
            SettingsButton = transform.FindChildRecursively<Button>(nameof(SettingsButton));
            QuitButton = transform.FindChildRecursively<Button>(nameof(QuitButton));

            // Find other game buttons (for future games)
            Button OtherGameButton = transform.FindChildRecursively<Button>("OtherGameButton");
            OtherGameButtons = new List<Button> {OtherGameButton};
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeHomeScreen();
        }

        private void InitializeHomeScreen()
        {
            TitleText.text = "LLM Games";
            TaglineText.text = "Challenge the AI, Master the Game!";

            ThreeCardBragButton.onClick.AddListener(StartThreeCardBrag);
            SettingsButton.onClick.AddListener(OpenSettings);
            QuitButton.onClick.AddListener(QuitGame);

            // Set up other game buttons (for future games)
            foreach (var button in OtherGameButtons)
            {
                button.onClick.AddListener(() => Debug.Log("Other game selected (not implemented yet)"));
            }
        }

        private void StartThreeCardBrag()
        {
            PlaySelectionSound();
            // GameManager.Instance.StartThreeCardBrag();
        }

        private void OpenSettings()
        {
            PlaySelectionSound();
            UIScreen<SettingsScreen>.Instance.ShowScreen();
        }

        public override void QuitGame()
        {
            base.QuitGame();
        }
    }
}