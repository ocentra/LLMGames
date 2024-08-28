using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(MultipleTriplets), menuName = "GameMode/Rules/MultipleTriplets")]
    public class MultipleTriplets : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 6;
        public override string RuleName { get; protected set; } = $"{nameof(MultipleTriplets)}";
        public override int BonusValue { get; protected set; } = 170;
        public override int Priority { get; protected set; } = 97;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GameMode.UseTrump ? GetTrumpCard() : null;

            if (hand.IsMultipleTriplets(trumpCard, GameMode))
            {
                bonusDetail = CalculateBonus(hand, trumpCard);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, Card trumpCard)
        {
            List<Rank> triplets = hand.GetTripletRanks(trumpCard, GameMode.UseTrump);
            int value = triplets.Sum(rank => rank.Value);
            int baseBonus = BonusValue * triplets.Count * value;

            string bonusCalculationDescriptions = $"{BonusValue} * {triplets.Count} * {value}";

            List<string> descriptions = new List<string> { $"Multiple Triplets: {string.Join(", ", triplets.Select(rank => CardUtility.GetRankSymbol(Suit.Spade, rank)))}" };

            int additionalBonus = 0;
            if (GameMode.UseTrump && hand.Contains(trumpCard))
            {
                additionalBonus += GameMode.TrumpBonusValues.TripletsBonus;
                descriptions.Add($"Trump Assisted: +{GameMode.TrumpBonusValues.TripletsBonus}");
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize < 6)
            {
                Debug.LogError("Hand size must be at least 6 for Multiple Triplets.");
                return Array.Empty<string>();
            }

            List<string> hand;
            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardSymbol);
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                hand = GeneratePotentialMultipleTripletHand(handSize, trumpCard, coloured);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Multiple Triplets hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            }
            while (!HandUtility.ConvertFromSymbols(hand.ToArray()).IsMultipleTriplets(trumpCard, GameMode));

            return hand.ToArray();
        }

        private List<string> GeneratePotentialMultipleTripletHand(int handSize, Card trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();
            List<Rank> usedRanks = new List<Rank>();
            List<Rank> availableRanks = Rank.GetStandardRanks()
                .Where(r => r != Rank.None && r != Rank.A && r != Rank.K)
                .ToList();

            // Generate two triplets
            for (int i = 0; i < 2; i++)
            {
                Rank tripletRank = availableRanks[Random.Range(0, availableRanks.Count)];
                usedRanks.Add(tripletRank);
                availableRanks.Remove(tripletRank);

                for (int j = 0; j < 3; j++)
                {
                    Suit suit = Suit.RandomBetweenStandard();
                    hand.Add(CardUtility.GetRankSymbol(suit, tripletRank, coloured));
                }
            }

            // Fill remaining slots
            while (hand.Count < handSize)
            {
                if (trumpCard != null && hand.Count == handSize - 1)
                {
                    hand.Add(CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured));
                }
                else
                {
                    Rank randomRank = availableRanks[Random.Range(0, availableRanks.Count)];
                    Suit randomSuit = Suit.RandomBetweenStandard();
                    hand.Add(CardUtility.GetRankSymbol(randomSuit, randomRank, coloured));
                }
            }

            return hand;
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Two or more triplets of cards with different ranks.";

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

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Heart, Rank.Six, false) : null;

            examples.Add(CreateExampleHand(cardCount, trumpCard, isPlayer));
            if (useTrump)
            {
                examples.Add(CreateExampleHand(cardCount, trumpCard, isPlayer));
            }

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}