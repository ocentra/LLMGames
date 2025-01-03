using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(DifferentColorsSequence), menuName = "GameMode/Rules/DifferentColorsSequence")]
    public class DifferentColorsSequence : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(DifferentColorsSequence)}";
        public override int BonusValue { get; protected set; } = 115;
        public override int Priority { get; protected set; } = 89;

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

            if (hand.IsRoyalSequence(GameMode))
            {
                return false;
            }

            if (hand.IsSameColorAndDifferentSuits())
            {
                return false;
            }

            bonusDetail = CalculateBonus(hand, false);


            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                if (hand.HasTrumpCard(trumpCard))
                {
                    Hand nonTrumpCards = hand.Where(c => c != trumpCard);
                    bool canFormSequence = hand.CanFormSequenceWithWild();

                    bool differentColorsNonTrump =
                        nonTrumpCards.Select(card => CardUtility.GetColorValue(card.Suit)).Distinct().Count() ==
                        nonTrumpCards.Count();

                    if (canFormSequence && differentColorsNonTrump)
                    {
                        bonusDetail = CalculateBonus(hand, true);
                        return true;
                    }
                }
            }

            return true;
        }

        private BonusDetail CalculateBonus(Hand hand, bool isTrumpAssisted)
        {
            int baseBonus = BonusValue * hand.Sum();
            int additionalBonus = 0;
            List<string> descriptions = new List<string> {"Different Colors Sequence:"};
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
                Debug.LogError("Hand size must be at least 3 for a Different Colors Sequence.");
                return Array.Empty<string>();
            }

            string[] hand;
            int attempts = 0;
            const int maxAttempts = 100;
            bool isValidSequence = false;

            do
            {
                List<string> tempHand = new List<string>();

                // Use SelectRanks to generate a valid sequence of ranks
                List<Rank> selectedRanks = CardUtility.SelectRanks(handSize, true);
                selectedRanks.Sort();

                Suit[] redSuits = {Suit.Heart, Suit.Diamond};
                Suit[] blackSuits = {Suit.Spade, Suit.Club};

                // Generate a sequence with alternating colors
                for (int i = 0; i < handSize; i++)
                {
                    if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                    {
                        tempHand.Add(trumpCard);
                    }
                    else
                    {
                        // Alternate between red and black suits
                        Suit[] currentSuits = i % 2 == 0 ? redSuits : blackSuits;
                        Suit randomSuit = currentSuits[Random.Range(0, currentSuits.Length)];
                        tempHand.Add(CardUtility.GetRankSymbol(randomSuit, selectedRanks[i], coloured));
                    }
                }

                hand = tempHand.ToArray();
                Hand handToValidate = HandUtility.ConvertFromSymbols(hand);

                // Validate if the generated hand is a sequence and alternates colors
                isValidSequence = handToValidate.IsSequence() &&
                                  handToValidate.Select(card => CardUtility.GetColorValue(card.Suit)).Distinct()
                                      .Count() == 2;

                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid sequence after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            } while (!isValidSequence);

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
                "A sequence of cards with alternating colors (red and black), optionally considering Trump Wild Card.";

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