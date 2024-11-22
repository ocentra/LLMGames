using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class RulesScreen : UIScreen<RulesScreen>
    {
        [ShowInInspector] [Required] public TextMeshProUGUI RulesText { get; private set; }

        [ShowInInspector] [Required] public Button BackButton { get; private set; }


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

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeRulesScreen();
        }

        private void InitializeRulesScreen()
        {
            RulesText.text = GameManager.Instance.GameMode.GameRules.Player;
            BackButton.onClick.AddListener(GoBack);
        }
    }
}