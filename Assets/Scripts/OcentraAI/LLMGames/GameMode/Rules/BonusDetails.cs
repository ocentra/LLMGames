using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [System.Serializable]
    public class BonusDetails
    {
        [OdinSerialize, ShowInInspector] public string RuleName { get; set; }
        [OdinSerialize, ShowInInspector] public int BaseBonus { get; set; }
        [OdinSerialize, ShowInInspector] public int AdditionalBonus { get; set; }
        [OdinSerialize, ShowInInspector] public List<string> BonusDescriptions { get; set; } = new List<string>();
        [OdinSerialize, ShowInInspector] public int Priority { get; set; }
        [OdinSerialize, ShowInInspector] public int TotalBonus => BaseBonus + AdditionalBonus;
    }
}