using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public abstract class BaseBonusRule
    {
        public string Description;
        public int BonusValue;

        protected BaseBonusRule(string description, int bonusValue)
        {
            Description = description;
            BonusValue = bonusValue;
        }

        public abstract bool Evaluate(List<Card> hand);
    }
}