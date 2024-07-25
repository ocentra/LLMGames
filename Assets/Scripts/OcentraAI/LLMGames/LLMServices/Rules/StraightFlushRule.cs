using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.LLMServices.Rules
{
    [Serializable]
    public class StraightFlushRule : BaseBonusRule
    {
        public StraightFlushRule() : base(nameof(StraightFlushRule), "Straight Flush with Trump Wild Card. Example: 9, 10, J of Spades", 12) { }

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

            Suit firstCardSuit = hand[0].Suit;
            List<int> ranks = new List<int>();
            bool allSameSuit = true;

            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];
                if (card.Suit != firstCardSuit && !card.Equals(GetTrumpCard()))
                {
                    allSameSuit = false;
                    break;
                }
                ranks.Add(card.GetRankValue());
            }

            if (!allSameSuit)
            {
                return false;
            }

            int normalValue = CalculateNormalValue(hand, ranks, hasTrumpCard);
            int wildCardValue = hasTrumpCard ? CalculateWildCardValue(hand, ranks) : 0;

            if (wildCardValue > normalValue)
            {
                bonus = wildCardValue - handValue;
            }
            else
            {
                bonus = normalValue - handValue;
            }

            return IsSequence(ranks) || (hasTrumpCard && CanFormSequenceWithWild(ranks));
        }

        private int CalculateNormalValue(List<Card> hand, List<int> ranks, bool hasTrumpCard)
        {
            int bonus = 0;
            if (IsSequence(ranks))
            {
                bonus += BonusValue; // Base bonus for Straight Flush

                if (hasTrumpCard)
                {
                    bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus;

                    bool isAdjacent = false;
                    for (int i = 0; i < hand.Count; i++)
                    {
                        if (IsRankAdjacent(hand[i].Rank, GetTrumpCard().Rank))
                        {
                            isAdjacent = true;
                            break;
                        }
                    }

                    if (isAdjacent)
                    {
                        bonus += GameInfo.Instance.CommonBonuses.RankAdjacentBonus;
                    }
                }
            }

            return CalculateHandValue(hand) + bonus;
        }

        private int CalculateWildCardValue(List<Card> hand, List<int> ranks)
        {
            if (!CanFormSequenceWithWild(ranks))
            {
                return 0;
            }

            int straightFlushValue = 0;
            for (int i = 0; i < hand.Count; i++)
            {
                if (!hand[i].Equals(GetTrumpCard()))
                {
                    straightFlushValue += hand[i].GetRankValue();
                }
            }
            straightFlushValue += GetOptimalWildCardValue(ranks);

            int bonus = BonusValue; // Base bonus for Straight Flush
            bonus += GameInfo.Instance.CommonBonuses.WildCardBonus; // Bonus for using Trump Card as wild
            bonus += GameInfo.Instance.CommonBonuses.TrumpCardBonus; // Bonus for Trump Card in hand

            return straightFlushValue + bonus;
        }
    }
}