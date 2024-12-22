using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public interface IBonusDetail
    {
        public string RuleName { get; set; }
        public int BaseBonus { get; set; }
        public int AdditionalBonus { get; set; }
        public List<string> BonusDescriptions { get; set; } 
        public string BonusCalculationDescriptions { get; set; }
        public int Priority { get; set; }
        public int TotalBonus => BaseBonus + AdditionalBonus;
    }
}