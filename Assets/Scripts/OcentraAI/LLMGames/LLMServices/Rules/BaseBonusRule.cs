using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public abstract class BaseBonusRule
    {
        public string Description;
        public int BonusValue;
        public Card TrumpCard;

        protected BaseBonusRule(string description, int bonusValue)
        {
            Description = description;
            BonusValue = bonusValue;
        }

        public abstract bool Evaluate(List<Card> hand);

        public virtual void GetTrumpCard()
        {
            TrumpCard = GameManager.Instance.DeckManager.TrumpCard;
        }
        protected int CalculateHandValue(List<Card> hand)
        {
            return hand.Sum(card => card.GetRankValue());
        }

        protected bool HasTrumpCard(List<Card> hand)
        {
            return hand.Any(card => card.Equals(TrumpCard));
        }

        protected Dictionary<Rank, int> GetRankCounts(List<Card> hand)
        {
            var rankCounts = new Dictionary<Rank, int>();
            foreach (var card in hand)
            {
                if (!rankCounts.TryAdd(card.Rank, 1))
                {
                    rankCounts[card.Rank]++;
                }
            }
            return rankCounts;
        }

        protected bool IsRankAdjacent(Rank rank1, Rank rank2)
        {
            return Math.Abs((int)rank1 - (int)rank2) == 1 ||
                   (rank1 == Rank.A && rank2 == Rank.Two) ||
                   (rank1 == Rank.Two && rank2 == Rank.A);
        }
        protected bool IsAscendingSequence(List<int> sortedRanks)
        {
            return (sortedRanks[1] == sortedRanks[0] + 1 && sortedRanks[2] == sortedRanks[1] + 1);
        }

        protected bool IsWraparoundSequence(List<int> sortedRanks)
        {
            return (sortedRanks[0] == 2 && sortedRanks[1] == 3 && sortedRanks[2] == 14) || // A-2-3
                   (sortedRanks[0] == 2 && sortedRanks[1] == 13 && sortedRanks[2] == 14) || // K-A-2
                   (sortedRanks[0] == 12 && sortedRanks[1] == 13 && sortedRanks[2] == 14); // Q-K-A
        }

        protected bool IsSequence(List<int> ranks)
        {
            var sortedRanks = new List<int>(ranks);
            sortedRanks.Sort();
            return IsAscendingSequence(sortedRanks) || IsWraparoundSequence(sortedRanks);
        }

        protected bool CanFormSequenceWithWild(List<int> ranks)
        {
            var sortedRanks = new List<int>(ranks);
            sortedRanks.Sort();
            return CheckWildSequence(sortedRanks) || CheckWraparoundWildSequence(sortedRanks);
        }

        protected bool CheckWildSequence(List<int> sortedRanks)
        {
            return (sortedRanks[1] == sortedRanks[0] + 1 && sortedRanks[2] <= sortedRanks[1] + 2) ||
                   (sortedRanks[1] <= sortedRanks[0] + 2 && sortedRanks[2] == sortedRanks[1] + 1);
        }

        protected bool CheckWraparoundWildSequence(List<int> sortedRanks)
        {
            return (sortedRanks[0] == 2 && sortedRanks[1] <= 4) || // x-2-3, x-2-4
                   (sortedRanks[0] == 2 && sortedRanks[2] == 14) || // 2-x-A
                   (sortedRanks[1] == 13 && sortedRanks[2] == 14) || // Q-K-x, x-K-A
                   (sortedRanks[0] == 12 && sortedRanks[2] == 14); // Q-x-A
        }

        protected int GetOptimalWildCardValue(List<int> ranks)
        {
            var sortedRanks = new List<int>(ranks);
            sortedRanks.Sort();
            if (sortedRanks[0] == 2 && sortedRanks[2] == 14) // A-2-x
            {
                return 3;
            }
            else if (sortedRanks[0] == 2 && sortedRanks[1] == 13) // K-A-2
            {
                return 14; // Ace
            }
            else if (sortedRanks[1] == 13 && sortedRanks[2] == 14) // Q-K-A
            {
                return 12; // Queen
            }
            else if (sortedRanks[1] == sortedRanks[0] + 1)
            {
                return Math.Min(sortedRanks[1] + 1, 14); // Complete the sequence at the high end, max of Ace (14)
            }
            else
            {
                return sortedRanks[0] + 1; // Complete the sequence in the middle
            }
        }
    }
}