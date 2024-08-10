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
            if (!VerifyNumberOfCards(hand)) return false;

            Rank? fourOfAKind = FindNOfAKind(hand, 4);

            // Check for TrumpOfAKind or full house rule
            if (IsFullHouseOrTrumpOfKind(hand))
            {
                return false; // TrumpOfAKind or FullHouse will handle this
            }

            if (fourOfAKind.HasValue)
            {
                bonusDetail = CalculateBonus(hand, fourOfAKind.Value);
                return true;
            }

            Rank? threeOfAKind = FindNOfAKind(hand, 3);

            if (threeOfAKind.HasValue && HasTrumpCard(hand))
            {
                bonusDetail = CalculateBonus(hand, threeOfAKind.Value);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, Rank rank)
        {
            int baseBonus = BonusValue * ((int)rank *4);
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Four of a Kind: {CardUtility.GetRankSymbol(Suit.Spades, rank)}" };
            string bonusCalculationDescriptions = $"{BonusValue} * ( {(int)rank} * 4)";

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.FourOfKindBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FourOfKindBonus}");
                }
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * ( {(int)rank} * 4) + {additionalBonus} ";
            }
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Four cards of the same rank, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 4; cardCount <= gameMode.NumberOfCards; cardCount++)
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

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 4)
                throw new ArgumentException("Hand size must be at least 4 for Four of a Kind.");

            List<string> hand = new List<string>();

            Rank fourOfAKindRank = (Rank)UnityEngine.Random.Range(2, 15);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                hand.Add($"{CardUtility.GetRankSymbol(suit, fourOfAKindRank, coloured)}");
            }

            for (int i = 4; i < handSize; i++)
            {
                Rank randomRank;
                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (randomRank == fourOfAKindRank);

                Suit randomSuit = (Suit)UnityEngine.Random.Range(0, 4);

                if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                {
                    hand.Add(trumpCard);
                }
                else
                {
                    hand.Add($"{CardUtility.GetRankSymbol(randomSuit, randomRank, coloured)}");
                }
            }

            return hand.ToArray();
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();
            string trumpCard = useTrump ? "6♥" : null;

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

    }
}
