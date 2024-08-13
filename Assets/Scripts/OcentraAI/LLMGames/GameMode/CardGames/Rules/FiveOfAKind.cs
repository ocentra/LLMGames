using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FiveOfAKind), menuName = "GameMode/Rules/FiveOfAKind")]
    public class FiveOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 5;
        public override string RuleName { get; protected set; } = $"{nameof(FiveOfAKind)}";
        public override int BonusValue { get; protected set; } = 140;
        public override int Priority { get; protected set; } = 94;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!GameMode.UseTrump || !hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GetTrumpCard();

            if (IsFiveOfAKind(hand, trumpCard))
            {
                bonusDetail = CalculateBonus(hand, trumpCard);
                return true;
            }

            return false;
        }

        private bool IsFiveOfAKind(Hand hand, Card trumpCard)
        {
            if (hand == null || hand.Count() != 5 || trumpCard == null)
            {
                return false;
            }

            Rank[] ranksToExclude = new[] { Rank.A, trumpCard.Rank };
            Dictionary<Rank, int> rankCounts = hand.GetRankCounts(ranksToExclude);
            int trumpCount = hand.Count(c => c != null && c == trumpCard);

            foreach (KeyValuePair<Rank, int> kvp in rankCounts)
            {
                if (kvp.Value == 4 && trumpCount == 1)
                {
                    return true; // Four of a kind plus trump card
                }
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, Card trumpCard)
        {
            if (hand == null) return null;

            Rank fourOfAKindRank = hand.GetFourOfAKindRank(trumpCard, GameMode.UseTrump);
            int baseBonus = BonusValue * ((int)fourOfAKindRank * 5);
            string bonusCalculationDescriptions = $"{BonusValue} * ({(int)fourOfAKindRank} * 5)";

            int additionalBonus = GameMode.TrumpBonusValues.FiveOfKindBonus;
            List<string> descriptions = new List<string>
            {
                $"Five of a Kind: {CardUtility.GetRankSymbol(Suit.Spades, fourOfAKindRank)}",
                $"Trump Card Bonus: +{GameMode.TrumpBonusValues.FiveOfKindBonus}"
            };

            bonusCalculationDescriptions += $" + {additionalBonus}";

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize != 5 || string.IsNullOrEmpty(trumpCardSymbol))
            {
                Debug.LogError($"Hand size must be exactly 5 and trump card must be specified for Five of a Kind.");
                return Array.Empty<string>();
            }

            string[] hand;
            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardSymbol);
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                hand = GeneratePotentialFiveOfAKindHand(trumpCard, coloured);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Five of a Kind hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            }
            while (!IsFiveOfAKind(HandUtility.ConvertFromSymbols(hand), trumpCard));

            return hand;
        }

        private string[] GeneratePotentialFiveOfAKindHand(Card trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();
            Rank fourOfAKindRank;
            do
            {
                fourOfAKindRank = (Rank)UnityEngine.Random.Range((int)Rank.Two, (int)Rank.K + 1);
            } while (fourOfAKindRank == Rank.A || fourOfAKindRank == trumpCard.Rank);

            List<Suit> availableSuits = new List<Suit> { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };

            // Add four cards of the same rank
            for (int i = 0; i < 4; i++)
            {
                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(suit, fourOfAKindRank, coloured));
            }

            // Add the trump card
            hand.Add(CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured));

            return hand.ToArray();
        }

        public override bool Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump || gameMode.NumberOfCards < 5) return false;

            Description = "Four cards of the same rank (2 to K, excluding A and trump rank) plus the trump card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            string exampleHand = CreateExampleString(5);
            if (!string.IsNullOrEmpty(exampleHand))
            {
                llmExamples.Add(exampleHand);
                playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList(), true));
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount)
        {
            string trumpCard = CardUtility.GetRankSymbol(Suit.Hearts, Rank.Six, false);
            string[] exampleHand = CreateExampleHand(cardCount, trumpCard, false);
            return exampleHand.Length > 0 ? string.Join(", ", exampleHand) : string.Empty;
        }
    }
}