using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FullHouse), menuName = "GameMode/Rules/FullHouse")]
    public class FullHouse : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(FullHouse)}";
        public override int BonusValue { get; protected set; } = 190;
        public override int Priority { get; protected set; } = 95;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (hand == null || !hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            if (hand.IsFullHouse(GetTrumpCard(), GameMode))
            {
                bonusDetail = CalculateBonus(hand);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand)
        {
            if (hand == null) return null;

            int totalBonus = BonusValue * hand.Sum();
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { "Full House" };
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";

            if (GameMode.UseTrump && hand.Contains(GetTrumpCard()))
            {
                additionalBonus += GameMode.TrumpBonusValues.FullHouseBonus;
                descriptions.Add($"Trump Card Bonus: +{BonusValue + GameMode.TrumpBonusValues.FullHouseBonus}");
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, totalBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize < MinNumberOfCard)
            {
                Debug.LogError($"Hand size must be at least {MinNumberOfCard} for a Full House.");
                return Array.Empty<string>();
            }

            List<string> hand;

            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardSymbol);
            int attempts = 0;
            const int maxAttempts = 100;
            bool isFullHouse = false;
            do
            {
                

                hand = GeneratePotentialFullHouseHand(handSize, trumpCard, coloured);
                Hand convertFromSymbols = HandUtility.ConvertFromSymbols(hand.ToArray());
                isFullHouse = convertFromSymbols.IsFullHouse(trumpCard, GameMode);



                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Full House hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
                attempts++;
            }
            while (!isFullHouse);

            return hand.ToArray();
        }

        private List<string> GeneratePotentialFullHouseHand(int handSize, Card trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();

            Rank primaryRank = CardUtility.GetRank(trumpCard, GameMode.UseTrump, Rank.A, Rank.K);
            Rank secondaryRank = CardUtility.GetRank(trumpCard, GameMode.UseTrump, Rank.K, Rank.Q);

            List<Suit> availableSuits = CardUtility.GetAvailableSuits().ToList();

            for (int i = 0; i < Math.Min(handSize, 4); i++)
            {
                if (availableSuits.Count == 0)
                {
                    availableSuits = CardUtility.GetAvailableSuits().ToList();
                }

                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(suit, primaryRank, coloured));
            }

            while (hand.Count < handSize)
            {
                if (availableSuits.Count == 0)
                {
                    availableSuits = CardUtility.GetAvailableSuits().ToList();
                }

                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(suit, secondaryRank, coloured));
            }

            if (trumpCard != null)
            {
                hand[hand.Count - 1] = CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured);
            }

            return hand;
        }







        public override bool Initialize(GameMode gameMode)
        {
            Description = "Full House with at least 3 of the highest non-trump rank, adjusted according to the trump card's rank.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = MinNumberOfCard; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string exampleHand = CreateExampleString(cardCount);
                if (!string.IsNullOrEmpty(exampleHand))
                {
                    llmExamples.Add(exampleHand);
                    playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList()));
                }

                if (gameMode.UseTrump)
                {
                    string exampleTrumpHand = CreateExampleString(cardCount, true);
                    if (!string.IsNullOrEmpty(exampleTrumpHand))
                    {
                        llmTrumpExamples.Add(exampleTrumpHand);
                        playerTrumpExamples.Add(HandUtility.GetHandAsSymbols(exampleTrumpHand.Split(", ").ToList()));
                    }
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool useTrump = false)
        {
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Heart, Rank.Six, false) : null;
            string[] exampleHand = CreateExampleHand(cardCount, trumpCard, false);
            return exampleHand.Length > 0 ? string.Join(", ", exampleHand) : string.Empty;
        }
    }
}