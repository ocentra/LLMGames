using ThreeCardBrag.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace ThreeCardBrag.GameScreen
{
    public class RulesScreen : BragScreen<RulesScreen>
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
