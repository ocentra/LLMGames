using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public abstract class BaseBonusRule
    {
        [OdinSerialize, ShowInInspector] public string RuleName { get; protected set; }
        [OdinSerialize, ShowInInspector] public string Description { get; protected set; }
        [OdinSerialize, ShowInInspector] public int BonusValue { get; protected set; }
        [OdinSerialize, ShowInInspector] public int Priority { get; protected set; }
        [OdinSerialize, ShowInInspector] public GameMode GameMode { get; protected set; }
        [OdinSerialize,ShowInInspector] public GameRulesContainer Examples { get; protected set; }
        protected BaseBonusRule(string ruleName, string description, int bonusValue, int priority, GameMode gameMode)
        {
            RuleName = ruleName;
            Description = description;
            BonusValue = bonusValue;
            Priority = priority;
            GameMode = gameMode;
        }

        public abstract void InitializeExamples();
        public abstract bool Evaluate(List<Card> hand, out BonusDetails bonusDetails);

        protected Card GetTrumpCard() => DeckManager.Instance.WildCards.GetValueOrDefault("TrumpCard");

        protected int CalculateHandValue(List<Card> hand) => hand.Sum(card => card.GetRankValue());

        protected bool HasTrumpCard(List<Card> hand) => hand.Any(card => card.Equals(GetTrumpCard()));

        protected Dictionary<Rank, int> GetRankCounts(List<Card> hand)
        {
            return hand.GroupBy(card => card.Rank).ToDictionary(g => g.Key, g => g.Count());
        }

        protected bool IsRankAdjacent(Rank rank1, Rank rank2)
        {
            return Math.Abs((int)rank1 - (int)rank2) == 1 ||
                   (rank1 == Rank.A && rank2 == Rank.Two) ||
                   (rank1 == Rank.Two && rank2 == Rank.A);
        }

        protected bool IsSequence(List<int> ranks)
        {
            var sortedRanks = ranks.OrderBy(r => r).ToList();
            return IsAscendingSequence(sortedRanks) || IsWraparoundSequence(sortedRanks);
        }

        private bool IsAscendingSequence(List<int> sortedRanks)
        {
            return sortedRanks.Zip(sortedRanks.Skip(1), (a, b) => b == a + 1).All(x => x);
        }

        private bool IsWraparoundSequence(List<int> sortedRanks)
        {
            return (sortedRanks[0] == 2 && sortedRanks[1] == 3 && sortedRanks[2] == 14) || // A-2-3
                   (sortedRanks[0] == 2 && sortedRanks[1] == 13 && sortedRanks[2] == 14) || // K-A-2
                   (sortedRanks[0] == 12 && sortedRanks[1] == 13 && sortedRanks[2] == 14); // Q-K-A
        }

        protected bool CanFormSequenceWithWild(List<int> ranks)
        {
            var sortedRanks = ranks.OrderBy(r => r).ToList();
            return CheckWildSequence(sortedRanks) || CheckWraparoundWildSequence(sortedRanks);
        }

        private bool CheckWildSequence(List<int> sortedRanks)
        {
            return sortedRanks.Zip(sortedRanks.Skip(1), (a, b) => b <= a + 2).All(x => x);
        }

        private bool CheckWraparoundWildSequence(List<int> sortedRanks)
        {
            return (sortedRanks[0] == 2 && sortedRanks[1] <= 4) || // x-2-3, x-2-4
                   (sortedRanks[0] == 2 && sortedRanks[2] == 14) || // 2-x-A
                   (sortedRanks[1] == 13 && sortedRanks[2] == 14) || // Q-K-x, x-K-A
                   (sortedRanks[0] == 12 && sortedRanks[2] == 14); // Q-x-A
        }

        protected int GetOptimalWildCardValue(List<int> ranks)
        {
            var sortedRanks = ranks.OrderBy(r => r).ToList();
            if (sortedRanks[0] == 2 && sortedRanks[1] == 3) return 4; // A-2-3
            if (sortedRanks[0] == 2 && sortedRanks[1] == 13) return 14; // K-A-2
            if (sortedRanks[0] == 12 && sortedRanks[1] == 13) return 14; // Q-K-A
            if (sortedRanks[1] == sortedRanks[0] + 1) return Math.Min(sortedRanks[1] + 1, 14);
            return sortedRanks[0] + 1;
        }

        protected List<List<Card>> GetAllCombinations(List<Card> hand, int combinationSize)
        {
            return GetCombinations(hand, combinationSize).ToList();
        }

        private IEnumerable<List<Card>> GetCombinations(List<Card> list, int length)
        {
            if (length == 1) return list.Select(t => new List<Card> { t });
            return GetCombinations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                            (t1, t2) => t1.Concat(new List<Card> { t2 }).ToList());
        }
    }
}
