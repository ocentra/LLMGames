using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(SameColorsSequence), menuName = "GameMode/Rules/SameColorsSequence")]
    public class SameColorsSequence : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(SameColorsSequence)}";
        public override int BonusValue { get; protected set; } = 120;
        public override int Priority { get; protected set; } = 90;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;

            if (!hand.VerifyHand(GameMode, MinNumberOfCard))
            {
                return false;
            }

            if (!hand.IsSequence())
            {
                return false;
            }


            // Check for royal flush
            if (hand.IsRoyalSequence(GameMode))
            {
                return false;
            }

            if (hand.IsSameColorAndDifferentSuits())
            {
                bonusDetail = CalculateBonus(hand, false);
                return true;
            }

            // Check for trump-assisted same color sequence
            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = hand.HasTrumpCard(GetTrumpCard());
                if (hasTrumpCard)
                {
                    Hand nonTrumpHand = hand.Where(c => c != trumpCard);
                    bool canFormSequence = hand.CanFormSequenceWithWild();
                    bool sameColorNonTrump = nonTrumpHand.All(card =>
                        CardUtility.GetColorValue(card.Suit) ==
                        CardUtility.GetColorValue(nonTrumpHand.GetCard(0).Suit));

                    if (canFormSequence && sameColorNonTrump)
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
            int additionalBonus = 0;
            List<string> descriptions = new List<string> {"Same Colors Sequence:"};
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.SequenceBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.SequenceBonus}");

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

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions,
                additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 3)
            {
                Debug.LogError("Hand size must be at least 3 for a Same Colors Sequence.");
                return Array.Empty<string>();
            }

            string[] hand;
            int attempts = 0;
            const int maxAttempts = 100;
            bool isValidSameColorSequence = false;

            do
            {
                List<string> tempHand = new List<string>();
                bool isRed = Random.value > 0.5f;
                Suit[] suits = isRed ? new[] {Suit.Heart, Suit.Diamond} : new[] {Suit.Spade, Suit.Club};
                List<Rank> selectedRanks = CardUtility.SelectRanks(handSize, true, true);
                selectedRanks.Sort();

                for (int i = 0; i < handSize; i++)
                {
                    if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                    {
                        tempHand.Add(trumpCard);
                    }
                    else
                    {
                        Suit randomSuit = suits[Random.Range(0, 2)];
                        tempHand.Add(CardUtility.GetRankSymbol(randomSuit, selectedRanks[i], coloured));
                    }
                }

                hand = tempHand.ToArray();
                Hand handToValidate = HandUtility.ConvertFromSymbols(hand);

                // Validate if the generated hand forms a valid Same Color Sequence
                isValidSameColorSequence = handToValidate.IsSameColorAndDifferentSuits() && handToValidate.IsSequence();

                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError(
                        $"Failed to generate a valid Same Color Sequence hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            } while (!isValidSameColorSequence);

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
            Description =
                "A sequence of cards all of the same color (red or black), optionally considering Trump Wild Card.";

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

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples,
                llmTrumpExamples, gameMode.UseTrump);
        }
    }
}