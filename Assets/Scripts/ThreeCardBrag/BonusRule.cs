using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeCardBrag
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

    [Serializable]
    public class SameColorSequenceRule : BaseBonusRule
    {
        public SameColorSequenceRule() : base("Sequence of 3 cards of the same color", 10) { }

        public override bool Evaluate(List<Card> hand)
        {
            return hand.Count == 3 && hand.All(card => card.Suit == hand[0].Suit);
        }
    }

    [Serializable]
    public class DifferentColorsSequenceRule : BaseBonusRule
    {
        public DifferentColorsSequenceRule() : base("Sequence of 3 cards of different colors", 5) { }

        public override bool Evaluate(List<Card> hand)
        {
            return hand.Count == 3 && hand.Select(card => card.Suit).Distinct().Count() == 3;
        }
    }

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