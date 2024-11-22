using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.Screens
{
    public class MatchmakerScreen : UI3DScreen<MatchmakerScreen>
    {
        [SerializeField] private TMP_Dropdown currencyDropdown;
        [SerializeField] private TMP_Dropdown gameModeDropdown;
        [SerializeField] private TMP_Dropdown skillLevelDropdown;
        [SerializeField] private TMP_Dropdown tableTypeDropdown;
        public TMP_Dropdown GameModeDropdown => gameModeDropdown;
        public TMP_Dropdown SkillLevelDropdown => skillLevelDropdown;
        public TMP_Dropdown TableTypeDropdown => tableTypeDropdown;
        public TMP_Dropdown CurrencyDropdown => currencyDropdown;



        #region Event Subscriptions

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
        }


        #endregion

        protected override void Init(bool startEnabled)
        {
            transform.FindChildWithComponent(ref gameModeDropdown, nameof(gameModeDropdown));
            transform.FindChildWithComponent(ref skillLevelDropdown, nameof(skillLevelDropdown));
            transform.FindChildWithComponent(ref tableTypeDropdown, nameof(tableTypeDropdown));
            transform.FindChildWithComponent(ref currencyDropdown, nameof(currencyDropdown));
            base.Init(StartEnabled);
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
            InitializeScreen();
        }

        private void InitializeScreen()
        {
        }
    }
}