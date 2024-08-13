using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FourOfAKind), menuName = "GameMode/Rules/FourOfAKind")]
    public class FourOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 4;
        public override string RuleName { get; protected set; } = $"{nameof(FourOfAKind)}";
        public override int BonusValue { get; protected set; } = 135;
        public override int Priority { get; protected set; } = 93;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GetTrumpCard();

            if (hand.IsFourOfAKind( trumpCard, GameMode.UseTrump))
            {
                bonusDetail = CalculateBonus(hand, trumpCard);
                return true;
            }

            return false;
        }



        private BonusDetail CalculateBonus(Hand hand, Card trumpCard)
        {
            if (hand == null) return null;

            Rank fourOfAKindRank = hand.GetFourOfAKindRank(trumpCard, GameMode.UseTrump);
            int baseBonus;
            string bonusCalculationDescriptions;

            if (GameMode.UseTrump && hand.Contains(trumpCard))
            {
                Rank tripleRank = hand.FirstNonTrumpRankOrDefault(trumpCard);
                baseBonus = BonusValue * ((int)tripleRank + (int)trumpCard.Rank);
                bonusCalculationDescriptions = $"{BonusValue} * ({(int)tripleRank} + {(int)trumpCard.Rank})";
            }
            else
            {
                baseBonus = BonusValue * ((int)fourOfAKindRank * 4);
                bonusCalculationDescriptions = $"{BonusValue} * ({(int)fourOfAKindRank} * 4)";
            }

            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Four of a Kind: {CardUtility.GetRankSymbol(Suit.Spades, fourOfAKindRank)}" };

            if (GameMode.UseTrump && hand.Contains(trumpCard))
            {
                additionalBonus += GameMode.TrumpBonusValues.FourOfKindBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FourOfKindBonus}");
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }



        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize != 4)
            {
                Debug.LogError($"Hand size must be exactly 4 for Four of a Kind.");
                return Array.Empty<string>();
            }

            string[] hand;
            Card trumpCard = GameMode.UseTrump ? CardUtility.GetCardFromSymbol(trumpCardSymbol) : null;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                hand = GeneratePotentialFourOfAKindHand(trumpCard, coloured, GameMode.UseTrump);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Four of a Kind hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            }
            while (!HandUtility.ConvertFromSymbols(hand).IsFourOfAKind(trumpCard, GameMode.UseTrump));

            return hand;
        }

        public static string[] GeneratePotentialFourOfAKindHand(Card trumpCard, bool coloured, bool useTrump)
        {
            List<string> hand = new List<string>();
            Rank fourOfAKindRank;
            do
            {
                fourOfAKindRank = (Rank)UnityEngine.Random.Range((int)Rank.Two, (int)Rank.K + 1);
            } while (fourOfAKindRank == Rank.A || (trumpCard != null && fourOfAKindRank == trumpCard.Rank));

            List<Suit> availableSuits = new List<Suit> { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };


            // Add four cards of the same rank (or three if using trump)
            int cardsToAdd = useTrump && trumpCard != null ? 3 : 4;
            for (int i = 0; i < cardsToAdd; i++)
            {
                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(suit, fourOfAKindRank, coloured));
            }

            // Add trump if we're using it
            if (useTrump && trumpCard != null)
            {
                hand.Add(CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured));
            }

            return hand.ToArray();
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Four cards of the same rank (2 to K, excluding A and trump rank), optionally using one Trump card to upgrade Three of a Kind.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string exampleHand = CreateExampleString(4, false);
            if (!string.IsNullOrEmpty(exampleHand))
            {
                llmExamples.Add(exampleHand);
                playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList(), true));
            }

            if (gameMode.UseTrump)
            {
                string exampleTrumpHand = CreateExampleString(4, true);
                if (!string.IsNullOrEmpty(exampleTrumpHand))
                {
                    llmTrumpExamples.Add(exampleTrumpHand);
                    playerTrumpExamples.Add(HandUtility.GetHandAsSymbols(exampleTrumpHand.Split(", ").ToList(), true));
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool useTrump = false)
        {
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Hearts, Rank.Six, false) : null;
            string[] exampleHand = CreateExampleHand(cardCount, trumpCard, false);
            return exampleHand.Length > 0 ? string.Join(", ", exampleHand) : string.Empty;
        }
    }
}