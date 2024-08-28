using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(ThreeOfAKind), menuName = "GameMode/Rules/ThreeOfAKind")]
    public class ThreeOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(ThreeOfAKind)}";
        public override int BonusValue { get; protected set; } = 125;
        public override int Priority { get; protected set; } = 91;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GetTrumpCard();

            if (hand.IsThreeOfAKind( trumpCard, GameMode.UseTrump))
            {
                bonusDetail = CalculateBonus(hand, trumpCard);
                return true;
            }

            return false;
        }



        private BonusDetail CalculateBonus(Hand hand, Card trumpCard)
        {
            if (hand == null) return null;

            Rank threeOfAKindRank = hand.GetThreeOfAKindRank(trumpCard, GameMode.UseTrump);
            int baseBonus;
            string bonusCalculationDescriptions;

            if (GameMode.UseTrump && hand.Contains(trumpCard))
            {
                Rank pairRank = hand.FirstNonTrumpRankOrDefault(trumpCard);
                baseBonus = BonusValue * ((int)pairRank.Value + (int)trumpCard.Rank.Value);
                bonusCalculationDescriptions = $"{BonusValue} * ({(int)pairRank.Value} + {(int)trumpCard.Rank.Value})";
            }
            else
            {
                baseBonus = BonusValue * ((int)threeOfAKindRank.Value * 3);
                bonusCalculationDescriptions = $"{BonusValue} * ({(int)threeOfAKindRank.Value} * 3)";
            }

            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Three of a Kind: {CardUtility.GetRankSymbol(Suit.Spade, threeOfAKindRank)}" };

            if (GameMode.UseTrump && hand.Contains(trumpCard))
            {
                additionalBonus += GameMode.TrumpBonusValues.ThreeOfKindBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.ThreeOfKindBonus}");
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }



        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize != 3)
            {
                Debug.LogError("Hand size must be exactly 3 for Three of a Kind.");
                return Array.Empty<string>();
            }

            string[] hand;
            Card trumpCard = GameMode.UseTrump ? CardUtility.GetCardFromSymbol(trumpCardSymbol) : null;
            int attempts = 0;
            const int maxAttempts = 100;

            bool isThreeOfAKind = false;
            do
            {
                hand = GeneratePotentialThreeOfAKindHand(handSize, trumpCard, coloured);
                Hand convertFromSymbols = HandUtility.ConvertFromSymbols(hand);
                isThreeOfAKind = convertFromSymbols.IsThreeOfAKind(trumpCard, GameMode.UseTrump);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Three of a Kind hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            }
            while (!isThreeOfAKind);

            return hand;
        }

        private string[] GeneratePotentialThreeOfAKindHand(int handSize, Card trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();
            Rank threeOfAKindRank = Rank.RandomBetween(Rank.Two, Rank.K);
            List<Suit> availableSuits = new List<Suit> { Suit.Heart, Suit.Diamond, Suit.Club, Suit.Spade };

            bool useTrump = GameMode.UseTrump && trumpCard != null;

            // Add three cards of the same rank (or two if using trump)
            int cardsToAdd = useTrump ? 2 : 3;
            for (int i = 0; i < cardsToAdd; i++)
            {
                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(suit, threeOfAKindRank, coloured));
            }

            // Add trump if we're using it
            if (useTrump)
            {
                hand.Add(CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured));
            }

            return hand.ToArray();
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Three cards of the same rank (2 to K), optionally using one Trump card as a wild card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string exampleHand = CreateExampleString(3);
            if (!string.IsNullOrEmpty(exampleHand))
            {
                llmExamples.Add(exampleHand);
                playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList()));
            }

            if (gameMode.UseTrump)
            {
                string exampleTrumpHand = CreateExampleString(3, true);
                if (!string.IsNullOrEmpty(exampleTrumpHand))
                {
                    llmTrumpExamples.Add(exampleTrumpHand);
                    playerTrumpExamples.Add(HandUtility.GetHandAsSymbols(exampleTrumpHand.Split(", ").ToList()));
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