using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace OcentraAI.LLMGames.GameModes
{
    public class CustomRuleState
    {
        [OdinSerialize, ShowInInspector] public float Priority { get; set; }
        [OdinSerialize, ShowInInspector] public float BonusValue { get; set; }
        [OdinSerialize, ShowInInspector] public bool IsSelected { get; set; }

        public CustomRuleState(BaseBonusRule rule)
        {
            Priority = rule.Priority;
            BonusValue = rule.BonusValue;
        }
    }
}