using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Screens3D;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class RulesScreen : UI3DScreen<RulesScreen>
    {
        [ShowInInspector] [Required] public TextMeshProUGUI RulesText { get; private set; }

        [ShowInInspector] [Required] public Button BackButton { get; private set; }
        [Required] public GameMode GameMode;

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
            RulesText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(RulesText));
            BackButton = transform.FindChildRecursively<Button>(nameof(RulesText));
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
            InitializeRulesScreen();
        }

        private void InitializeRulesScreen()
        {
            RulesText.text = GameMode.GameRules.Player;
            BackButton.onClick.AddListener(GoBack);
        }
    }
}