using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class RulesScreen : UIScreen<RulesScreen>
    {
        [ShowInInspector, Required]
        public TextMeshProUGUI RulesText { get; private set; }

        [ShowInInspector, Required]
        public Button BackButton { get; private set; }

        private GameInfo GameInfo { get; set; }

        protected override void Awake()
        {
            base.Awake();
            GameInfo = GameInfo.Instance;
            InitReferences();
        }

        void OnValidate()
        {
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
            RulesText.text = GameInfo.GameRules;
            BackButton.onClick.AddListener(GoBack);
        }


    }
}
