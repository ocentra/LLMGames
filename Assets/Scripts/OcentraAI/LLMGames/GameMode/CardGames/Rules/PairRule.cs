using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(PairRule), menuName = "GameMode/Rules/PairRule")]
    public class PairRule : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(PairRule)}";
        public override int BonusValue { get; protected set; } = 100;
        public override int Priority { get; protected set; } = 87;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard))
            {
                return false;
            }

            Card trumpCard = GetTrumpCard();


            if (hand.IsPair(trumpCard, GameMode.UseTrump, out List<Rank> pairRank))
            {
                if (pairRank.Count == 1)
                {
                    bonusDetail = CalculateBonus(pairRank[0]);
                }

                return true;
            }

            return false;
        }


        private BonusDetail CalculateBonus(Rank pairRank)
        {
            int baseBonus = BonusValue * pairRank.Value * 2;
            List<string> descriptions = new List<string> {$"Pair of {CardUtility.GetRankSymbol(Suit.Spade, pairRank)}"};
            string bonusCalculationDescriptions = $"{BonusValue} * ({pairRank.Value} * 2)";

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize is < 3 or > 9)
            {
                Debug.LogError($"Hand size must be between 3 and 9 for a Pair. Received: {handSize}");
                return Array.Empty<string>();
            }

            string[] hand;
            int attempts = 0;
            const int maxAttempts = 100;

            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardSymbol);
            do
            {
                hand = GeneratePotentialPairHand(handSize, coloured);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Pair hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            } while (!HandUtility.ConvertFromSymbols(hand)
                         .IsPair(trumpCard, GameMode.UseTrump, out List<Rank> pairRanks) && pairRanks.Count == 1);

            return hand;
        }

        private string[] GeneratePotentialPairHand(int handSize, bool coloured)
        {
            List<string> hand = new List<string>();
            Rank pairRank = Rank.RandomBetweenStandard();
            List<Suit> availableSuits = CardUtility.GetAvailableSuits().ToList();
            List<(Suit, Rank)> usedCombinations = new List<(Suit, Rank)>();

            // Generate the pair
            for (int i = 0; i < 2; i++)
            {
                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(suit, pairRank, coloured));
                usedCombinations.Add((suit, pairRank));
            }

            // Fill the rest with random cards, making sure not to repeat rank and suit combination
            while (hand.Count < handSize)
            {
                if (availableSuits.Count == 0)
                {
                    availableSuits = CardUtility.GetAvailableSuits().ToList(); // Reset if needed
                }

                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                Rank rank;
                do
                {
                    rank = Rank.RandomBetweenStandard();
                } while (usedCombinations.Contains((suit, rank)));

                hand.Add(CardUtility.GetRankSymbol(suit, rank, coloured));
                usedCombinations.Add((suit, rank));
            }

            return hand.ToArray();
        }


        public override bool Initialize(GameMode gameMode)
        {
            Description =
                "Exactly one pair of cards with the same rank (2 to A), valid only for hands of 3 to 9 cards, when no trump card is present, no other pairs or higher combinations exist, and the hand is not a potential sequence.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string exampleHand = CreateExampleString(cardCount);
                if (!string.IsNullOrEmpty(exampleHand))
                {
                    llmExamples.Add(exampleHand);
                    playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList()));
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null,
                gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount)
        {
            string[] exampleHand = CreateExampleHand(cardCount, null, false);
            return exampleHand.Length > 0 ? string.Join(", ", exampleHand) : string.Empty;
        }
    }
}