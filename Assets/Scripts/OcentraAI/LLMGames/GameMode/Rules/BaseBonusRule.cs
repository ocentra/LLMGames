using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    public abstract class BaseBonusRule : SerializedScriptableObject
    {
        [OdinSerialize, ShowInInspector, ReadOnly] public abstract int MinNumberOfCard { get; protected set; }
        [OdinSerialize, ShowInInspector] public abstract int BonusValue { get; protected set; }
        [OdinSerialize, ShowInInspector] public abstract int Priority { get; protected set; }
        [OdinSerialize, ShowInInspector, ReadOnly] public abstract string RuleName { get; protected set; }
        [OdinSerialize, ShowInInspector, ReadOnly] public string Description { get; protected set; }
        [OdinSerialize, ShowInInspector, ReadOnly] public GameMode GameMode { get; protected set; }
        [OdinSerialize, ShowInInspector] public GameRulesContainer Examples { get; protected set; }

        public void UpdateRule(BaseBonusRule rule)
        {
            BonusValue = rule.BonusValue;
            Priority = rule.Priority;
        }

        public bool SetGameMode(GameMode gameMode)
        {
            GameMode = gameMode;
            return Initialize(gameMode);
        }

        public abstract bool Evaluate(List<Card> hand, out BonusDetails bonusDetails);


        public abstract bool Initialize(GameMode gameMode);

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


        protected Rank? FindNOfAKind(Dictionary<Rank, int> rankCounts)
        {
            return rankCounts.Where(kv => kv.Value >= GameMode.NumberOfCards)
                .OrderByDescending(kv => kv.Key)
                .Select(kv => (Rank?)kv.Key)
                .FirstOrDefault();
        }

        protected Rank? FindNOfAKind(Dictionary<Rank, int> rankCounts, int numberOfCards)
        {
            return rankCounts.Where(kv => kv.Value >= numberOfCards)
                .OrderByDescending(kv => kv.Key)
                .Select(kv => (Rank?)kv.Key)
                .FirstOrDefault();
        }


        protected bool IsNOfAKind(Dictionary<Rank, int> rankCounts, Rank rank, int n)
        {
            return rankCounts.TryGetValue(rank, out int count) && count == n;
        }


        protected bool IsNOfAKind(Dictionary<Rank, int> rankCounts)
        {
            return FindNOfAKind(rankCounts).HasValue;
        }

        protected bool IsNOfAKind(Dictionary<Rank, int> rankCounts,Rank rank)
        {
            return FindNOfAKind(rankCounts) == rank;
        }

        protected bool IsFullHouseOrTrumpOfKind(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            if (GameMode.UseTrump && rankCounts.TryGetValue(trumpCard.Rank, out int trumpRankCount) && trumpRankCount == GameMode.NumberOfCards)
            {
                return true;

            }


            return rankCounts.TryGetValue(Rank.A, out int aceCount) && aceCount == GameMode.NumberOfCards;
        }

        protected bool IsNOfAKindOfTrump(Dictionary<Rank, int> rankCounts, Card trumpCard, int n)
        {
            return rankCounts.TryGetValue(trumpCard.Rank, out int trumpCount) && trumpCount == n;
        }

        protected bool IsNOfAKindOfTrump(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            return FindNOfAKind(rankCounts) == trumpCard.Rank;
        }

        protected string[] ConvertCardSymbols(string[] cardSymbols)
        {
            List<string> convertedSymbols = new List<string>();

            foreach (string symbol in cardSymbols)
            {
                Rank rank;
                Suit suit;

                if (symbol.Length == 3) // For "10♠" type symbols
                {
                    rank = Rank.Ten;
                    suit = Card.GetSuitFromChar(symbol[2]);
                }
                else if (symbol.Length == 2) // For "2♠", "J♠" type symbols
                {
                    rank = Card.GetRankFromChar(symbol[0]);
                    suit = Card.GetSuitFromChar(symbol[1]);
                }
                else
                {
                    convertedSymbols.Add(symbol); // Handle invalid or special cases
                    continue;
                }

                convertedSymbols.Add(Card.GetRankSymbol(suit, rank, false));
            }

            return convertedSymbols.ToArray();
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

        protected bool IsTrumpInMiddle(List<Card> orderedHand, Card trumpCard)
        {
            int handSize = orderedHand.Count;
            if (handSize % 2 == 1)
            {
                // Odd number of cards, one middle position
                int middleIndex = handSize / 2;
                return orderedHand[middleIndex].Equals(trumpCard);
            }
            else
            {
                // Even number of cards, two middle positions
                int firstMiddleIndex = (handSize / 2) - 1;
                int secondMiddleIndex = handSize / 2;
                return orderedHand[firstMiddleIndex].Equals(trumpCard) || orderedHand[secondMiddleIndex].Equals(trumpCard);
            }
        }

        protected bool IsRankAdjacentToTrump(List<Card> orderedHand, Card trumpCard)
        {
            foreach (var card in orderedHand)
            {
                if (IsRankAdjacent(card.Rank, trumpCard.Rank))
                {
                    return true;
                }
            }
            return false;
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

        protected BonusDetails CreateBonusDetails(string ruleName, int baseBonus, int priority, List<string> descriptions, int additionalBonus = 0)
        {
            return new BonusDetails
            {
                RuleName = ruleName,
                BaseBonus = baseBonus,
                AdditionalBonus = additionalBonus,
                BonusDescriptions = descriptions,
                Priority = priority
            };
        }

        protected List<int> GetSequence(int numberOfCards)
        {
            var baseSequence = new List<int> { 6, 7, 8, 9, 10, 11, 12, 13, 14 }; // 6, 7, 8, 9, 10, J, Q, K, A
            return baseSequence.Skip(baseSequence.Count - numberOfCards).ToList();
        }

        protected List<int> GetRoyalSequence()
        {
            List<int> royalRanks = new List<int>
            {
                (int)Rank.A, (int)Rank.K, (int)Rank.Q, (int)Rank.J, (int)Rank.Ten,
                (int)Rank.Nine, (int)Rank.Eight, (int)Rank.Seven, (int)Rank.Six
            };

            return royalRanks.Take(GameMode.NumberOfCards).ToList();
        }
        protected bool IsSequence(List<int> ranks, List<int> sequence)
        {
            return !sequence.Except(ranks).Any();
        }

        protected bool TryCreateExample(string ruleName, string description, int bonusValue, List<string> playerExamples,
            List<string> llmExamples, List<string> playerTrumpExamples,
            List<string> llmTrumpExamples, bool useTrump)
        {
            bool IsApplicable(List<string> examplesList)
            {
                return examplesList != null && examplesList.Count > 0;
            }

            string GetExamplesDescription(List<string> examples, List<string> trumpExamples, bool useTrumpExamples)
            {
                if (!IsApplicable(examples))
                {
                    return "Rule Not Applicable to this gamemode";
                }

                string examplesDescription = string.Join($"{Environment.NewLine}", examples);
                if (useTrumpExamples && IsApplicable(trumpExamples))
                {
                    examplesDescription += $"{Environment.NewLine}Trump Examples:{Environment.NewLine}" +
                                           $"{string.Join($"{Environment.NewLine}", trumpExamples)}{Environment.NewLine}";
                }

                return examplesDescription;
            }

            bool hasPlayerExamples = IsApplicable(playerExamples);
            bool hasLlmExamples = IsApplicable(llmExamples);

            if (!hasPlayerExamples && !hasLlmExamples)
            {
                return false;
            }

            var playerDescription = $"{ruleName} {description}{Environment.NewLine}" +
                                    $"{ruleName} Bonus: {bonusValue}{Environment.NewLine}" +
                                    $"Examples:{Environment.NewLine}" +
                                    $"{GetExamplesDescription(playerExamples, playerTrumpExamples, useTrump)}";

            var llmDescription = $"{ruleName} {description}{Environment.NewLine}" +
                                 $"{ruleName} Bonus: {bonusValue}{Environment.NewLine}" +
                                 $"Examples:{Environment.NewLine}" +
                                 $"{GetExamplesDescription(llmExamples, llmTrumpExamples, useTrump)}";

            Examples = new GameRulesContainer { Player = playerDescription, LLM = llmDescription };
            return true;
        }



    }


}
