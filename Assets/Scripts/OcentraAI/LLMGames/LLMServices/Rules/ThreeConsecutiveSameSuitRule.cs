using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.LLMServices.Rules
{
    [Serializable]
    public class ThreeConsecutiveSameSuitRule : BaseBonusRule
    {
        public ThreeConsecutiveSameSuitRule() : base(nameof(ThreeConsecutiveSameSuitRule), "Sequence of 3 cards of the same suit with Trump Wild Card. Example: 4, 5, 6 of Hearts", 10) { }

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
            bool trumpUsedAsWild = false;

            for (int i = 0; i < hand.Count; i++)
            {
                Card card = hand[i];
                if (card.Suit != firstCardSuit && !card.Equals(GetTrumpCard()))
                {
                    allSameSuit = false;
                    break;
                }
                if (card.Equals(GetTrumpCard()))
                {
                    trumpUsedAsWild = true;
                }
                ranks.Add(card.GetRankValue());
            }

            if (!allSameSuit)
            {
                return false;
            }

            bool isSequence = IsSequence(ranks);
            if (isSequence)
            {
                bonus = CalculateBonus(hand, hasTrumpCard, trumpUsedAsWild);
            }

            return isSequence;
        }

        private int CalculateBonus(List<Card> hand, bool hasTrumpCard, bool trumpUsedAsWild)
        {
            int bonus = BonusValue; // Base bonus

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

                if (trumpUsedAsWild)
                {
                    bonus += GameInfo.Instance.CommonBonuses.WildCardBonus;
                }
            }

            return bonus;
        }
    }
}