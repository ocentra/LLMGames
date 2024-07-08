using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public class SameColorSequenceRule : BaseBonusRule
    {
        public SameColorSequenceRule() : base("Sequence of 3 cards of the same color", 10) { }

        public override bool Evaluate(List<Card> hand)
        {
            return hand.Count == 3 && hand.All(card => card.Suit == hand[0].Suit);
        }
    }
}