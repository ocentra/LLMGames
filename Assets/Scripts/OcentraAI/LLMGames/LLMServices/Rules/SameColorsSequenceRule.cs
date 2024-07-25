using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.LLMServices.Rules
{
    [Serializable]
    public class SameColorsSequenceRule : BaseBonusRule
    {
        public SameColorsSequenceRule() : base(nameof(SameColorsSequenceRule), "Sequence of 3 cards of the same color with Trump Wild Card. Example: 7 of Hearts, 8 of Diamonds, 9 of Hearts", 10) { }

        public override bool Evaluate(List<Card> hand, out int bonus)
        {
            bonus = 0;
            GetTrumpCard();
            if (hand.Count != 3)
            {
                return false;
            }

            int handValue = CalculateHandValue(hand);
            bool hasTrumpCard = HasTrumpCard(hand);

            string firstCardColor = hand[0].GetColorString();
            List<int> ranks = new List<int>();
            bool allSameColor = true;
            bool trumpUsedAsWild = false;

            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];
                if (card.GetColorString() != firstCardColor && !card.Equals(GetTrumpCard()))
                {
                    allSameColor = false;
                    break;
                }
                if (card.Equals(GetTrumpCard()))
                {
                    trumpUsedAsWild = true;
                }
                ranks.Add(card.GetRankValue());
            }   

            if (!allSameColor)
            {
                return false;
            }

            bool isSequence = IsSequence(ranks);
            if (isSequence)
            {
                 bonus = CalculateBonus(hand, hasTrumpCard, trumpUsedAsWild);
            }
            else if (hasTrumpCard && CanFormSequenceWithWild(ranks))
            {
                int wildCardValue = CalculateWildCardValue(hand, ranks);
                bonus = wildCardValue - handValue;
                return true;
            }

            return isSequence;
        }

        private int CalculateBonus(List<Card> hand, bool hasTrumpCard, bool trumpUsedAsWild)
        {
            int bonus = BonusValue; // Base bonus

            if (hasTrumpCard)
            {
                bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus;

                if (trumpUsedAsWild)
                {
                    bonus += GameInfo.Instance.CommonBonuses.WildCardBonus;
                }

                bool isAdjacent = false;
                bool isMiddle = false;
                for (int i = 0; i < hand.Count; i++)
                {
                    if (IsRankAdjacent(hand[i].Rank, GetTrumpCard().Rank))
                    {
                        isAdjacent = true;
                    }
                    if (i == 1 && hand[i].Equals(GetTrumpCard()))
                    {
                        isMiddle = true;
                    }
                }

                if (isAdjacent)
                {
                    bonus += GameInfo.Instance.CommonBonuses.RankAdjacentBonus;
                }

                if (isMiddle)
                {
                    bonus += 5; // Additional bonus for Trump card in the middle
                }
            }

            return bonus;
        }

        private int CalculateWildCardValue(List<Card> hand, List<int> ranks)
        {
            int wildCardValue = 0;
            for (int i = 0; i < hand.Count; i++)
            {
                if (!hand[i].Equals(GetTrumpCard()))
                {
                    wildCardValue += hand[i].GetRankValue();
                }
            }
            wildCardValue += GetOptimalWildCardValue(ranks);

            int bonus = BonusValue; // Base bonus
            bonus += GameInfo.Instance.CommonBonuses.WildCardBonus; // Bonus for using Trump Card as wild
            bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus; // Bonus for Trump Card in hand

            return wildCardValue + bonus;
        }
    }
}