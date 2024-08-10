using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        [OdinSerialize, ShowInInspector] public GameRulesContainer Examples { get; protected set; } = new GameRulesContainer();


        public virtual string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            return new[] { String.Empty, };
        }

        public void UpdateRule(int bonusValue, int priority)
        {
            BonusValue = bonusValue;
            Priority = priority;
        }

        public bool SetGameMode(GameMode gameMode)
        {
            GameMode = gameMode;
            return Initialize(gameMode);
        }

        public abstract bool Evaluate(Hand hand, out BonusDetail bonusDetail);


        [Button(ButtonSizes.Large)]
        [ShowIf(nameof(IsGameModeAssigned))]
        public void Initialize()
        {
            Initialize(GameMode);
        }

        public abstract bool Initialize(GameMode gameMode);

        private bool IsGameModeAssigned()
        {
            return GameMode != null;

        }

        protected Card GetTrumpCard() => DeckManager.Instance.WildCards.GetValueOrDefault("TrumpCard"); //todo put it in some const class no string literals



        protected bool HasTrumpCard(Hand hand) => hand.HasTrumpCard(GetTrumpCard());

        protected Dictionary<Rank, int> GetRankCounts(Hand hand)
        {
            return hand.Cards.GroupBy(card => card.Rank).ToDictionary(g => g.Key, g => g.Count());
        }

        protected bool IsRankAdjacent(Rank rank1, Rank rank2)
        {
            return Math.Abs((int)rank1 - (int)rank2) == 1 ||
                   (rank1 == Rank.A && rank2 == Rank.Two) ||
                   (rank1 == Rank.Two && rank2 == Rank.A);
        }


        protected Rank? FindNOfAKind(Hand hand, int numberOfCards)
        {
            return GetRankCounts(hand).Where(kv => kv.Value >= numberOfCards)
                .OrderByDescending(kv => kv.Key)
                .Select(kv => (Rank?)kv.Key)
                .FirstOrDefault();
        }



        protected bool IsNOfAKind(Hand hand, Rank rank, int numberOfCards)
        {
            return GetRankCounts(hand).TryGetValue(rank, out int count) && count == numberOfCards;
        }

        protected bool IsNOfAKind(Hand hand, int numberOfCards)
        {
            return FindNOfAKind(hand, numberOfCards).HasValue;
        }


        protected bool IsFullHouseOrTrumpOfKind(Hand hand)
        {
            Dictionary<Rank, int> rankCounts = GetRankCounts(hand);
            if (GameMode.UseTrump && rankCounts.TryGetValue(GetTrumpCard().Rank, out int trumpRankCount) && trumpRankCount == GameMode.NumberOfCards)
            {
                return true;

            }
            return rankCounts.TryGetValue(Rank.A, out int aceCount) && aceCount == GameMode.NumberOfCards;
        }




        protected bool IsNOfAKindOfTrump(Hand hand, int n)
        {
            return GetRankCounts(hand).TryGetValue(GetTrumpCard().Rank, out int trumpCount) && trumpCount == n;
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
                    suit = CardUtility.GetSuitFromChar(symbol[2]);
                }
                else if (symbol.Length == 2) // For "2♠", "J♠" type symbols
                {
                    rank = CardUtility.GetRankFromChar(symbol[0]);
                    suit = CardUtility.GetSuitFromChar(symbol[1]);
                }
                else
                {
                    convertedSymbols.Add(symbol); // Handle invalid or special cases
                    continue;
                }

                convertedSymbols.Add(CardUtility.GetRankSymbol(suit, rank, false));
            }

            return convertedSymbols.ToArray();
        }

        protected bool IsRoyalSequence(Hand hand)
        {
            if (IsSameSuits(hand))
            {
                List<int> ranks = hand.Cards.Select(card => (int)card.Rank).OrderBy(rank => rank).ToList();
                List<int> royalSequence = GetRoyalSequence();
                return ranks.SequenceEqual(royalSequence.Take(ranks.Count));
            }

            return false;
        }

        protected static bool IsSameSuits(Hand hand)
        {
            return hand.Cards.All(card => card.Suit == hand.Cards[0].Suit);
        }



        protected bool IsSequence(Hand hand)
        {
            if (hand == null || hand.Count() < 2) return false;

            List<int> ranks = hand.Cards.Select(card => card.GetRankValue()).OrderBy(rank => rank).ToList();
            return IsAscendingSequence(ranks) || IsWraparoundSequence(ranks);
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
            List<int> sortedRanks = ranks.OrderBy(r => r).ToList();
            return CheckWildSequence(sortedRanks) || CheckWraparoundWildSequence(sortedRanks);
        }

        private bool CheckWildSequence(List<int> sortedRanks)
        {
            return sortedRanks.Zip(sortedRanks.Skip(1), (a, b) => b <= a + 2).All(x => x);
        }

        private bool CheckWraparoundWildSequence(List<int> sortedRanks)
        {
            if (sortedRanks.Count < 3)
            {
                Debug.LogWarning("sortedRanks list has fewer than 3 elements.");
                return false;
            }

            return (sortedRanks[0] == 2 && sortedRanks[1] <= 4) || // x-2-3, x-2-4
                   (sortedRanks[0] == 2 && sortedRanks[2] == 14) || // 2-x-A
                   (sortedRanks[1] == 13 && sortedRanks[2] == 14) || // Q-K-x, x-K-A
                   (sortedRanks[0] == 12 && sortedRanks[2] == 14); // Q-x-A
        }

        protected bool IsSameColorAndDifferentSuits(Hand hand)
        {
            if (hand == null || hand.Count() == 0) return false;

            Color firstCardColor = CardUtility.GetColorValue(hand.Cards[0].Suit);


            foreach (Card card in hand.Cards)
            {
                if (CardUtility.GetColorValue(card.Suit) != firstCardColor)
                {
                    return false;
                }

            }

            return IsSameSuits(hand);
        }

        protected bool IsTrumpInMiddle(Hand orderedHand, Card trumpCard)
        {
            int handSize = orderedHand.Count();
            if (handSize % 2 == 1)
            {
                // Odd number of cards, one middle position
                int middleIndex = handSize / 2;
                return orderedHand.Cards[middleIndex].Equals(trumpCard);
            }
            else
            {
                // Even number of cards, two middle positions
                int firstMiddleIndex = (handSize / 2) - 1;
                int secondMiddleIndex = handSize / 2;
                return orderedHand.Cards[firstMiddleIndex].Equals(trumpCard) || orderedHand.Cards[secondMiddleIndex].Equals(trumpCard);
            }
        }

        protected bool IsRankAdjacentToTrump(Hand orderedHand, Card trumpCard)
        {
            foreach (Card card in orderedHand.Cards)
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
            List<int> sortedRanks = ranks.OrderBy(r => r).ToList();
            if (sortedRanks[0] == 2 && sortedRanks[1] == 3) return 4; // A-2-3
            if (sortedRanks[0] == 2 && sortedRanks[1] == 13) return 14; // K-A-2
            if (sortedRanks[0] == 12 && sortedRanks[1] == 13) return 14; // Q-K-A
            if (sortedRanks[1] == sortedRanks[0] + 1) return Math.Min(sortedRanks[1] + 1, 14);
            return sortedRanks[0] + 1;
        }

        protected List<Rank> SelectNonHighRanks(int handSize,Rank trumpRank)
        {
            // Exclude high cards (J, Q, K, A)
            List<Rank> availableRanks = Enum.GetValues(typeof(Rank))
                .Cast<Rank>()
                .Where(rank => rank != Rank.J && rank != Rank.Q && rank != Rank.K && rank != Rank.A && (trumpRank != Rank.None && rank != trumpRank))
                .ToList();

            return availableRanks.OrderBy(rank => UnityEngine.Random.value).Take(handSize).ToList();
        }

        protected BonusDetail CreateBonusDetails(string ruleName, int baseBonus, int priority, List<string> descriptions, string bonusCalculationDescriptions, int additionalBonus = 0)
        {
            return new BonusDetail
            {
                RuleName = ruleName,
                BaseBonus = baseBonus,
                AdditionalBonus = additionalBonus,
                BonusDescriptions = descriptions,
                Priority = priority,
                BonusCalculationDescriptions = bonusCalculationDescriptions
            };
        }

        protected bool VerifyNumberOfCards(Hand hand)
        {
            return GameMode != null && GameMode.NumberOfCards >= MinNumberOfCard && hand.Count() == GameMode.NumberOfCards;
        }


        protected List<int> GetSequence(int numberOfCards)
        {
            List<int> baseSequence = new List<int> { 6, 7, 8, 9, 10, 11, 12, 13, 14 };
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

        protected List<Rank> GetRoyalRankSequence()
        {
            List<Rank> royalRanks = new List<Rank>
            {
                Rank.A, Rank.K, Rank.Q, Rank.J, Rank.Ten,
                Rank.Nine, Rank.Eight, Rank.Seven, Rank.Six
            };

            return royalRanks.Take(GameMode.NumberOfCards).ToList();
        }


        protected bool TryCreateExample(string ruleName, string description, int bonusValue, List<string> playerExamples,
            List<string> llmExamples, List<string> playerTrumpExamples,
            List<string> llmTrumpExamples, bool useTrump)
        {
            bool IsApplicable(List<string> examplesList)
            {
                return examplesList is { Count: > 0 };
            }

            string GetExamplesDescription(List<string> examples, List<string> trumpExamples, bool useTrumpExamples)
            {
                if (!IsApplicable(examples))
                {
                    return "Rule Not Applicable to this GameMode";
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

            string playerDescription = $"{ruleName} {description}{Environment.NewLine}" +
                                       $"{ruleName} Bonus: {bonusValue}{Environment.NewLine}" +
                                       $"Examples:{Environment.NewLine}" +
                                       $"{GetExamplesDescription(playerExamples, playerTrumpExamples, useTrump)}";

            string llmDescription = $"{ruleName} {description}{Environment.NewLine}" +
                                    $"{ruleName} Bonus: {bonusValue}{Environment.NewLine}" +
                                    $"Examples:{Environment.NewLine}" +
                                    $"{GetExamplesDescription(llmExamples, llmTrumpExamples, useTrump)}";

            Examples = new GameRulesContainer { Player = playerDescription, LLM = llmDescription };
            return true;
        }



    }


}
