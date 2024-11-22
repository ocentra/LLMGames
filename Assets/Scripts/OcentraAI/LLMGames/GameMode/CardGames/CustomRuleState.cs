using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace OcentraAI.LLMGames.GameModes
{
    public class CustomRuleState
    {
        public CustomRuleState(BaseBonusRule rule)
        {
            Priority = rule.Priority;
            BonusValue = rule.BonusValue;
        }

        [OdinSerialize] [ShowInInspector] public float Priority { get; set; }
        [OdinSerialize] [ShowInInspector] public float BonusValue { get; set; }
        [OdinSerialize] [ShowInInspector] public bool IsSelected { get; set; }
    }
}