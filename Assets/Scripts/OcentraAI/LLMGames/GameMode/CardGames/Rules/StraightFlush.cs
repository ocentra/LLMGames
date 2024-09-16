using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(StraightFlush), menuName = "GameMode/Rules/StraightFlush")]
    public class StraightFlush : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(StraightFlush)}";
        public override int BonusValue { get; protected set; } = 180;
        public override int Priority { get; protected set; } = 98;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;


            // Check for royal flush
            if (hand.IsRoyalSequence(GameMode))
            {
                return false;
            }

            // Check for natural straight flush
            if (hand.IsSameSuits() && hand.IsSequence())
            {
                bonusDetail = CalculateBonus(hand, false);
                return true;
            }

            // Check for trump-assisted straight flush
            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = hand.HasTrumpCard(trumpCard);
                if (hasTrumpCard)
                {
                    Hand trumpHand = hand.Where(c => c != trumpCard);

                    bool canFormStraightFlush = trumpHand.IsSameSuits() && trumpHand.CanFormSequenceWithWild() || (hand.IsSequence() && trumpHand.IsSameSuits());

                    if (canFormStraightFlush)
                    {
                        bonusDetail = CalculateBonus(hand, true);
                        return true;
                    }
                }
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, bool isTrumpAssisted)
        {
            int baseBonus = BonusValue * hand.Sum();
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";

            int additionalBonus = 0;
            List<string> descriptions = new List<string> { "Straight Flush:" };

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.StraightFlushBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.StraightFlushBonus}");

                Hand orderedHand = hand.OrderBy(card => card.Rank.Value);
                Card trumpCard = GetTrumpCard();

                // Check for CardInMiddleBonus
                if (orderedHand.IsTrumpInMiddle(trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.CardInMiddleBonus;
                    descriptions.Add($"Trump Card in Middle: +{GameMode.TrumpBonusValues.CardInMiddleBonus}");
                }

                // Check for RankAdjacentBonus
                if (orderedHand.IsRankAdjacentToTrump(trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()} + {additionalBonus} ";
            }
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 3)
            {
                Debug.LogError("Hand size must be at least 3 for a Straight Flush.");
                return Array.Empty<string>();
            }

            string[] hand;
            int attempts = 0;
            const int maxAttempts = 100;
            bool isValidStraightFlush = false;

            do
            {
                List<string> tempHand = new List<string>();
                Suit flushSuit = CardUtility.GetRandomSuit();
                List<Rank> selectedRanks = CardUtility.SelectRanks(handSize, allowSequence: true, sameSuit: true, fixedSuit: flushSuit);
                selectedRanks.Sort();

                for (int i = 0; i < handSize; i++)
                {
                    if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                    {
                        tempHand.Add(trumpCard);
                    }
                    else
                    {
                        tempHand.Add(CardUtility.GetRankSymbol(flushSuit, selectedRanks[i], coloured));
                    }
                }

                hand = tempHand.ToArray();
                Hand handToValidate = HandUtility.ConvertFromSymbols(hand);

                // Validate if the generated hand forms a valid Straight Flush
                isValidStraightFlush = handToValidate.IsSameSuits() && handToValidate.IsSequence();

                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Straight Flush hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }

            } while (!isValidStraightFlush);

            return hand;
        }


        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Heart, Rank.Six, isPlayer) : null;

            examples.Add(CreateExampleHand(cardCount, null, isPlayer));
            if (useTrump)
            {
                examples.Add(CreateExampleHand(cardCount, trumpCard, isPlayer));
            }

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "A sequence of cards in the same suit, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                playerExamples.Add(CreateExampleString(cardCount, true));
                llmExamples.Add(CreateExampleString(cardCount, false));

                if (gameMode.UseTrump)
                {
                    playerTrumpExamples.Add(CreateExampleString(cardCount, true, true));
                    llmTrumpExamples.Add(CreateExampleString(cardCount, false, true));
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}
