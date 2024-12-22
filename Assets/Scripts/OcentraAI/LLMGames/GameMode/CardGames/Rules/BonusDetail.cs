using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class BonusDetail : IBonusDetail
    {
        [OdinSerialize] [ShowInInspector] public string RuleName { get; set; }
        [OdinSerialize] [ShowInInspector] public int BaseBonus { get; set; }
        [OdinSerialize] [ShowInInspector] public int AdditionalBonus { get; set; }
        [OdinSerialize] [ShowInInspector] public List<string> BonusDescriptions { get; set; } = new List<string>();
        [OdinSerialize] [ShowInInspector] public string BonusCalculationDescriptions { get; set; }

        [OdinSerialize] [ShowInInspector] public int Priority { get; set; }
        [OdinSerialize] [ShowInInspector] public int TotalBonus => BaseBonus + AdditionalBonus;
    }
}