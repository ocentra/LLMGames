using System;
using System.Collections.Generic;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;

namespace OcentraAI.LLMGames.LLMServices.Rules
{
    [Serializable]
    public class PairInHandRule : BaseBonusRule
    {
        public PairInHandRule() : base(nameof(PairInHandRule), "Pair in hand with bonuses for trump card and same color. Example: 5 of Hearts, 5 of Spades, and any card", 5) { }

        public override bool Evaluate(List<Card> hand, out int bonus)
        {
            bonus = 0;
            GetTrumpCard();

            int handValue = CalculateHandValue(hand);
            bool hasTrumpCard = HasTrumpCard(hand);
            var rankCounts = GetRankCounts(hand);
            var colorCounts = GetColorCounts(hand);

            bool hasPair = HasPair(rankCounts);
            bool hasThreeOfAKind = HasThreeOfAKind(rankCounts);

            int normalBonus = CalculateBonus(hasPair, hasThreeOfAKind, hasTrumpCard, rankCounts, colorCounts);
            int normalValue = handValue + normalBonus;

            int wildCardValue = 0;
            if (hasTrumpCard && !hasThreeOfAKind)
            {
                wildCardValue = CalculateWildCardValue(hand, rankCounts);
            }

            if (wildCardValue > normalValue)
            {
                bonus = wildCardValue - handValue;
            }
            else
            {
                bonus = normalBonus;
            }

            return hasPair || hasThreeOfAKind || (hasTrumpCard && HasSingleCard(rankCounts));
        }

        private Dictionary<string, int> GetColorCounts(List<Card> hand)
        {
            var colorCounts = new Dictionary<string, int>();
            foreach (var card in hand)
            {
                var color = card.GetColorString();
                if (!colorCounts.TryAdd(color, 1))
                {
                    colorCounts[color]++;
                }
            }
            return colorCounts;
        }

        private bool HasPair(Dictionary<Rank, int> rankCounts)
        {
            foreach (var count in rankCounts.Values)
            {
                if (count == 2) return true;
            }
            return false;
        }

        private bool HasThreeOfAKind(Dictionary<Rank, int> rankCounts)
        {
            foreach (var count in rankCounts.Values)
            {
                if (count == 3) return true;
            }
            return false;
        }

        private bool HasSingleCard(Dictionary<Rank, int> rankCounts)
        {
            foreach (var count in rankCounts.Values)
            {
                if (count == 1) return true;
            }
            return false;
        }

        private int CalculateBonus(bool hasPair, bool hasThreeOfAKind, bool hasTrumpCard, Dictionary<Rank, int> rankCounts, Dictionary<string, int> colorCounts)
        {
            int bonus = 0;

            if (hasTrumpCard)
            {
                bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus;
            }

            if (hasPair || hasThreeOfAKind)
            {
                bonus += BonusValue; // Base bonus for pair

                if (hasTrumpCard && rankCounts[GetTrumpCard().Rank] >= 2)
                {
                    bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus;
                }

                foreach (var count in colorCounts.Values)
                {
                    if (count >= 2)
                    {
                        bonus += GameInfo.Instance.CommonBonuses.SameColorBonus;
                        break;
                    }
                }
            }

            return bonus;
        }

        private int CalculateWildCardValue(List<Card> hand, Dictionary<Rank, int> rankCounts)
        {
            int maxValue = 0;

            foreach (var rank in rankCounts.Keys)
            {
                if (rankCounts[rank] == 2)
                {
                    int threeOfAKindValue = 0;
                    foreach (var card in hand)
                    {
                        if (card.Rank == rank) threeOfAKindValue += card.GetRankValue();
                    }
                    threeOfAKindValue += GetTrumpCard().GetRankValue();

                    int bonus = GameInfo.Instance.GetBonusRule<ThreeOfAKindRule>().BonusValue; // Base bonus for Three of a Kind
                    bonus += GameInfo.Instance.CommonBonuses.WildCardBonus; // Bonus for using Trump Card as wild
                    if (IsRankAdjacent(rank, GetTrumpCard().Rank))
                    {
                        bonus += GameInfo.Instance.CommonBonuses.RankAdjacentBonus;
                    }
                    int totalValue = threeOfAKindValue + bonus;
                    if (totalValue > maxValue) maxValue = totalValue;
                }
                else if (rankCounts[rank] == 1 && rank != GetTrumpCard().Rank)
                {
                    int pairValue = 0;
                    string cardColor = "";
                    foreach (var card in hand)
                    {
                        if (card.Rank == rank)
                        {
                            pairValue = card.GetRankValue();
                            cardColor = card.GetColorString();
                            break;
                        }
                    }
                    pairValue += GetTrumpCard().GetRankValue();

                    int bonus = BonusValue; // Base bonus for Pair
                    bonus += GameInfo.Instance.CommonBonuses.WildCardBonus; // Bonus for using Trump Card as wild
                    if (cardColor == GetTrumpCard().GetColorString())
                    {
                        bonus += GameInfo.Instance.CommonBonuses.SameColorBonus; // Bonus for same color
                    }
                    int totalValue = pairValue + bonus;
                    if (totalValue > maxValue) maxValue = totalValue;
                }
            }

            return maxValue;
        }
    }
}