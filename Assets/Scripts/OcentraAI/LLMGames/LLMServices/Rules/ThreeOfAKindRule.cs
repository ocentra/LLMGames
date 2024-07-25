using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.LLMServices.Rules
{
    [Serializable]
    public class ThreeOfAKindRule : BaseBonusRule
    {
        public ThreeOfAKindRule() : base(nameof(ThreeOfAKindRule), "Three of a Kind with Trump Wild Card. Example: AAA or KKK", 15) { }

        public override bool Evaluate(List<Card> hand, out int bonus )
        {
            bonus = 0;
            GetTrumpCard();
            if (hand.Count != 3)
            {
                return false;
            }

            int handValue = CalculateHandValue(hand);
            var rankCounts = GetRankCounts(hand);
            bool hasTrumpCard = HasTrumpCard(hand);

            int normalValue = CalculateNormalValue(hand, rankCounts, hasTrumpCard);
            int wildCardValue = hasTrumpCard ? CalculateWildCardValue(hand, rankCounts) : 0;

            if (wildCardValue > normalValue)
            {
                bonus = wildCardValue - handValue;
            }
            else
            {
                bonus = normalValue - handValue;
            }

            return rankCounts.ContainsValue(3) || (hasTrumpCard && rankCounts.ContainsValue(2));
        }

        private int CalculateNormalValue(List<Card> hand, Dictionary<Rank, int> rankCounts, bool hasTrumpCard)
        {
            int bonus = 0;
            bool isThreeOfAKind = false;

            foreach (var count in rankCounts.Values)
            {
                if (count == 3)
                {
                    isThreeOfAKind = true;
                    break;
                }
            }

            if (isThreeOfAKind)
            {
                bonus += BonusValue; // Base bonus for Three of a Kind

                if (hasTrumpCard)
                {
                    bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus;

                    bool hasAdjacentRank = false;
                    for (int i = 0; i < hand.Count; i++)
                    {
                        if (IsRankAdjacent(hand[i].Rank, GetTrumpCard().Rank))
                        {
                            hasAdjacentRank = true;
                            break;
                        }
                    }

                    if (hasAdjacentRank)
                    {
                        bonus += GameInfo.Instance.CommonBonuses.RankAdjacentBonus;
                    }
                }
            }

            return CalculateHandValue(hand) + bonus;
        }

        private int CalculateWildCardValue(List<Card> hand, Dictionary<Rank, int> rankCounts)
        {
            int maxValue = 0;

            foreach (var kvp in rankCounts)
            {
                if (kvp.Value == 2)
                {
                    int threeOfAKindValue = 0;
                    for (int i = 0; i < hand.Count; i++)
                    {
                        if (hand[i].Rank == kvp.Key)
                        {
                            threeOfAKindValue += hand[i].GetRankValue();
                        }
                    }
                    threeOfAKindValue += GetTrumpCard().GetRankValue();

                    int bonus = BonusValue; // Base bonus for Three of a Kind
                    bonus += GameInfo.Instance.CommonBonuses.WildCardBonus; // Bonus for using Trump Card as wild
                    bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus; // Bonus for Trump Card in hand

                    if (IsRankAdjacent(kvp.Key, GetTrumpCard().Rank))
                    {
                        bonus += GameInfo.Instance.CommonBonuses.RankAdjacentBonus;
                    }

                    int totalValue = threeOfAKindValue + bonus;
                    if (totalValue > maxValue)
                    {
                        maxValue = totalValue;
                    }
                }
            }

            return maxValue;
        }
    }
}