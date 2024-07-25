using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices.Rules
{
    [Serializable]
    public class DifferentColorsSequenceRule : BaseBonusRule
    {
        public DifferentColorsSequenceRule() : base(nameof(DifferentColorsSequenceRule),"Sequence of 3 cards of different colors with Trump Wild Card. Example: 4 of Hearts, 5 of Spades, 6 of Diamonds", 5) { }

        public override bool Evaluate(List<Card> hand , out int bonusValue)
        {
            bonusValue = 0;
            if (hand.Count != 3)
            {
                return false;
            }

            int handValue = CalculateHandValue(hand);
            bool hasTrumpCard = HasTrumpCard(hand);

            string[] colors = new string[3];
            int[] ranks = new int[3];
            bool trumpUsedAsWild = false;
            int uniqueColors = 0;

            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];
                string color = card.GetColorString();
                bool newColor = true;
                for (int j = 0; j < i; j++)
                {
                    if (colors[j] == color)
                    {
                        newColor = false;
                        break;
                    }
                }
                if (newColor) uniqueColors++;

                colors[i] = color;
                ranks[i] = card.GetRankValue();

                if (card.Equals(GetTrumpCard()))
                {
                    trumpUsedAsWild = true;
                }
            }

            if (uniqueColors != 3)
            {
                return false;
            }

            bool isSequence = IsSequence(ranks.ToList());
            if (isSequence)
            {
                int bonus = CalculateBonus(hand, hasTrumpCard, trumpUsedAsWild);
                bonusValue = bonus;
            }
            else if (hasTrumpCard && CanFormSequenceWithWild(ranks.ToList()))
            {
                int wildCardValue = CalculateWildCardValue(hand, ranks);
                bonusValue = wildCardValue - handValue;
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

        private int CalculateWildCardValue(List<Card> hand, int[] ranks)
        {
            int wildCardValue = 0;
            for (int i = 0; i < hand.Count; i++)
            {
                if (!hand[i].Equals(GetTrumpCard()))
                {
                    wildCardValue += hand[i].GetRankValue();
                }
            }
            wildCardValue += GetOptimalWildCardValue(new List<int>(ranks));

            int bonus = BonusValue; // Base bonus
            bonus += GameInfo.Instance.CommonBonuses.WildCardBonus; // Bonus for using Trump Card as wild
            bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus; // Bonus for Trump Card in hand

            return wildCardValue + bonus;
        }
    }
}