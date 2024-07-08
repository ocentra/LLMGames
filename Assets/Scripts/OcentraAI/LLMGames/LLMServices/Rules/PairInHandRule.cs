using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public class PairInHandRule : BaseBonusRule
    {
        public PairInHandRule() : base("Pair in hand", 5) { }

        public override bool Evaluate(List<Card> hand)
        {
            return hand.GroupBy(card => card.Rank).Any(group => group.Count() == 2);
        }
    }
}