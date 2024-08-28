using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(TrumpOfAKind), menuName = "GameMode/Rules/TrumpOfAKind")]
    public class TrumpOfAKind : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(TrumpOfAKind)}";
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override int BonusValue { get; protected set; } = 160;
        public override int Priority { get; protected set; } = 96;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!GameMode.UseTrump || !hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GetTrumpCard();
            int trumpCount = hand.Count(card => card.Rank == trumpCard.Rank);

            if (trumpCount == GameMode.NumberOfCards || (trumpCount == 4 && GameMode.NumberOfCards > 4))
            {
                Hand remainingCards = trumpCount == 4 && GameMode.NumberOfCards > 4
                    ? hand.Where(card => card.Rank != trumpCard.Rank)
                    : null;

                bonusDetail = CalculateBonus(hand, remainingCards);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, Hand remainingCards = null)
        {
            int baseBonus = BonusValue * hand.Sum();
            List<string> descriptions = new List<string> { $"Trump of a Kind: All {GameMode.NumberOfCards} of Trump Cards" };

            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";
            int additionalBonus = 0;

            if (remainingCards != null)
            {
                (string desc, int bonus) = EvaluateRemainingCards(remainingCards);
                if (!string.IsNullOrEmpty(desc))
                {
                    descriptions.Add(desc);
                    additionalBonus += bonus;
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        private (string description, int bonus) EvaluateRemainingCards(Hand remainingHand)
        {
            if (remainingHand.IsRoyalSequence(GameMode))
                return ("Royal Sequence: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.RoyalFlushBonus);

            if (remainingHand.IsSequence())
            {
                if (remainingHand.IsSameSuits())
                    return ("Straight Flush: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.StraightFlushBonus);

                return remainingHand.IsSameColorAndDifferentSuits() ?
                    ("Same Color Sequence: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.SameColorBonus) :
                    ("Sequence: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.SequenceBonus);
            }

            switch (remainingHand.Count())
            {
                case 5 when remainingHand.IsNOfAKind(Rank.A, 4) && remainingHand.Any(card => card.Rank == Rank.K):
                    return ("Four of a Kind: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.FourOfKindBonus);
                case 4 when remainingHand.IsNOfAKind(Rank.A, 4):
                    return ("Four of a Kind: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.FourOfKindBonus);
                case 3 when remainingHand.IsNOfAKind(Rank.A, 3):
                    return ("Three of a Kind: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.ThreeOfKindBonus);
                case 2 when remainingHand.IsNOfAKind(Rank.A, 2):
                    return ("Two Pair: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.PairBonus);
                case 1 when remainingHand.IsNOfAKind(Rank.A, 1):
                    return ("High card: " + remainingHand.GetHandAsSymbols(), GameMode.TrumpBonusValues.HighCardBonus);
                default:
                    return ("", 0);
            }
        }

        public override bool Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump) return false;

            Description = $"All {gameMode.NumberOfCards} Trump cards Rank in the hand.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = MinNumberOfCard; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string exampleHand = CreateExampleString(cardCount, false);

                if (!string.IsNullOrEmpty(exampleHand))
                {
                    llmExamples.Add(exampleHand);
                    playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList()));
                }


            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 3 || string.IsNullOrEmpty(trumpCard))
            {
                Debug.LogError("Hand size must be at least 3 and trump card must be specified for Trump of a Kind.");
                return Array.Empty<string>();
            }

            List<string> hand;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                hand = GeneratePotentialHand(handSize, trumpCard, coloured);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning($"Could not generate a valid hand meeting the condition after {maxAttempts} attempts.");
                    break;
                }
            }
            while (!HandMeetsCriteria(hand, handSize, CardUtility.GetRankFromSymbol(trumpCard)));

            return hand.ToArray();
        }




        private List<string> GeneratePotentialHand(int handSize, string trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();
            Rank trumpRank = CardUtility.GetRankFromSymbol(trumpCard);
            List<Suit> availableSuits = CardUtility.GetAvailableSuits().ToList();

            int trumpCardsToInclude = Math.Min(4, handSize);
            for (int i = 0; i < trumpCardsToInclude; i++)
            {
                Suit randomSuit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(randomSuit, trumpRank, coloured));
            }

            if (handSize > 4)
            {
                // Add sequences or other combinations for the remaining cards
                int remainingCards = handSize - 4;
                int ruleChoice = Random.Range(0, 3);
                List<string> additionalCards = ruleChoice switch
                {
                    0 => HandUtility.GetRoyalSequenceAsRank(remainingCards).ToCardSymbols(coloured),
                    1 => HandUtility.GetHighestSequence(remainingCards).ToCardSymbols(coloured),
                    2 => HandUtility.GetStraightFlushAsSymbol(remainingCards, coloured),
                    _ => GenerateRandomHand(remainingCards, trumpRank, coloured)
                };
                hand.AddRange(additionalCards);
            }
            else
            {
                // If handSize <= 4, the entire hand should consist of trump cards or a random hand
                hand = GenerateRandomHand(handSize, trumpRank, coloured);
            }

            return hand;
        }


        private bool HandMeetsCriteria(List<string> hand, int numberOfCards, Rank trumpRank)
        {
            Hand generatedHand = HandUtility.ConvertFromSymbols(hand.ToArray());

            if (numberOfCards > 4)
            {
                // Ensure there are at least 4 trump cards in the hand
                int trumpCount = generatedHand.Count(card => card.Rank == trumpRank);
                return trumpCount >= 4;
            }

            // Ensure all cards are trump cards for smaller hands
            return generatedHand.All(card => card.Rank == trumpRank);
        }





        private List<string> GenerateRandomHand(int handSize, Rank trumpRank, bool coloured)
        {
            List<string> hand = new List<string>();

            List<Suit> availableSuits = CardUtility.GetAvailableSuits().ToList();

            int trumpCardsToInclude = GameMode.NumberOfCards == 3 ? 3 :4;

            for (int i = 0; i < trumpCardsToInclude; i++)
            {
                Suit randomSuit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                hand.Add(CardUtility.GetRankSymbol(randomSuit, trumpRank, coloured));
            }

            while (hand.Count < handSize)
            {
                Rank randomRank;
                Suit randomSuit;
                do
                {
                    randomRank = Rank.RandomBetweenStandard();
                    randomSuit = Suit.RandomBetweenStandard();
                } while (randomRank == trumpRank && availableSuits.Contains(randomSuit));

                hand.Add(CardUtility.GetRankSymbol(randomSuit, randomRank, coloured));
            }

            return hand;
        }

        private string CreateExampleString(int cardCount, bool isPlayer)
        {
            string trumpCard = "6♥";
            string[] example = CreateExampleHand(cardCount, trumpCard, isPlayer);
            return string.Join(", ", example);
        }
    }
}
