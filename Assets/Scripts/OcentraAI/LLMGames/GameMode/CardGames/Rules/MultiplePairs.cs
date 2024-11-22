using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(MultiplePairsRule), menuName = "GameMode/Rules/MultiplePairsRule")]
    public class MultiplePairsRule : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 4;
        public override string RuleName { get; protected set; } = $"{nameof(MultiplePairsRule)}";
        public override int BonusValue { get; protected set; } = 130;
        public override int Priority { get; protected set; } = 92;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard))
            {
                return false;
            }

            Card trumpCard = GameMode.UseTrump ? GetTrumpCard() : null;

            if (hand.IsMultiplePairs(trumpCard, GameMode.UseTrump, out List<Rank> pairRanks))
            {
                bonusDetail = CalculateBonus(hand, pairRanks, trumpCard);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, List<Rank> pairs, Card trumpCard)
        {
            int value = pairs.Sum(rank => rank.Value);
            int baseBonus = BonusValue * pairs.Count * value;

            string bonusCalculationDescriptions = $"{BonusValue} * {pairs.Count} * {value}";

            int additionalBonus = 0;
            List<string> descriptions = new List<string>
            {
                $"Multiple Pairs: {string.Join(", ", pairs.Select(rank => CardUtility.GetRankSymbol(Suit.Spade, rank)))}"
            };

            if (GameMode.UseTrump && trumpCard != null && hand.HasTrumpCard(trumpCard))
            {
                if (pairs.Any(rank => rank == trumpCard.Rank))
                {
                    additionalBonus += GameMode.TrumpBonusValues.PairBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                }

                if (pairs.Any(rank => HandUtility.IsRankAdjacent(trumpCard.Rank, rank)))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent Bonus: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions,
                additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Two or more pairs of cards with different ranks.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = MinNumberOfCard; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string playerExample = CreateExampleString(cardCount, true);
                string llmExample = CreateExampleString(cardCount, false);

                playerExamples.Add(playerExample);
                llmExamples.Add(llmExample);

                if (gameMode.UseTrump)
                {
                    string playerTrumpExample = CreateExampleString(cardCount, true, true);
                    string llmTrumpExample = CreateExampleString(cardCount, false, true);
                    playerTrumpExamples.Add(playerTrumpExample);
                    llmTrumpExamples.Add(llmTrumpExample);
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples,
                llmTrumpExamples, gameMode.UseTrump);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize < MinNumberOfCard)
            {
                Debug.LogError($"Hand size must be at least {MinNumberOfCard} for Multiple Pairs.");
                return Array.Empty<string>();
            }

            List<string> hand;
            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardSymbol);
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                hand = GeneratePotentialMultiplePairsHand(handSize, trumpCard, coloured);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Multiple Pairs hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            } while (!HandUtility.ConvertFromSymbols(hand.ToArray())
                         .IsMultiplePairs(trumpCard, GameMode.UseTrump, out _));

            return hand.ToArray();
        }


        private List<string> GeneratePotentialMultiplePairsHand(int handSize, Card trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();
            List<Rank> usedRanks = new List<Rank>();
            List<Rank> availableRanks = new List<Rank>(Rank.GetStandardRanks());
            availableRanks.RemoveAll(r => r == Rank.None);

            for (int i = 0; i < 2; i++)
            {
                Rank pairRank;
                do
                {
                    pairRank = Rank.RandomBetweenStandard();
                } while (usedRanks.Contains(pairRank));

                usedRanks.Add(pairRank);
                availableRanks.Remove(pairRank);

                hand.Add(CardUtility.GetRankSymbol(Suit.Heart, pairRank, coloured));
                hand.Add(CardUtility.GetRankSymbol(Suit.Diamond, pairRank, coloured));
            }

            while (hand.Count < handSize)
            {
                if (trumpCard != null && hand.Count == handSize - 1)
                {
                    hand.Add(CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured));
                }
                else
                {
                    Rank randomRank;
                    do
                    {
                        randomRank = Rank.RandomBetweenStandard();
                    } while (usedRanks.Contains(randomRank));

                    Suit randomSuit = Suit.RandomBetweenStandard();
                    hand.Add(CardUtility.GetRankSymbol(randomSuit, randomRank, coloured));
                    usedRanks.Add(randomRank);
                    availableRanks.Remove(randomRank);
                }
            }

            return hand;
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Heart, Rank.Six, false) : null;
            string[] exampleHand = CreateExampleHand(cardCount, trumpCard, isPlayer);
            return string.Join(", ", exampleHand);
        }
    }
}