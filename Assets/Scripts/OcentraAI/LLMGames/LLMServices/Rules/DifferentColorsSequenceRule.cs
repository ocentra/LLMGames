using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public class DifferentColorsSequenceRule : BaseBonusRule
    {
        public DifferentColorsSequenceRule() : base("Sequence of 3 cards of different colors", 5) { }

        public override bool Evaluate(List<Card> hand)
        {
            return hand.Count == 3 && hand.Select(card => card.Suit).Distinct().Count() == 3;
        }
    }
}