using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(ThreeOfAKind), menuName = "GameMode/Rules/ThreeOfAKind")]
    public class ThreeOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(ThreeOfAKind)}";
        public override int BonusValue { get; protected set; } = 125;
        public override int Priority { get; protected set; } = 91;

        public override bool Evaluate(List<Card> hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;

            if (!VerifyNumberOfCards(hand)) return false;

            Dictionary<Rank, int> rankCounts = GetRankCounts(hand);
            Rank? threeOfAKind = FindNOfAKind(hand, 3);

            // Check for TrumpOfAKind or full house rule
            if (IsFullHouseOrTrumpOfKind(hand))
            {
                return false; // TrumpOfAKind or FullHouse will handle this
            }



            if (threeOfAKind.HasValue)
            {
                bonusDetail = CalculateBonus(hand, threeOfAKind.Value);
                return true;
            }

            Rank? twoOfAKind = FindNOfAKind(hand, 2);

            if (twoOfAKind.HasValue && HasTrumpCard(hand))
            {
                bonusDetail = CalculateBonus(hand, twoOfAKind.Value);
                return true;
            }

            return false;
        }



        private BonusDetail CalculateBonus(List<Card> hand, Rank rank)
        {
            int baseBonus = BonusValue * ((int)rank * 3);

            string bonusCalculationDescriptions = $"{BonusValue} * ({(int)rank} * 3)";


            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Three of a Kind: {Card.GetRankSymbol(Suit.Spades, rank)}" };

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.ThreeOfKindBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.ThreeOfKindBonus}");
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * ({(int)rank} * 3) + {additionalBonus} ";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Three cards of the same rank, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
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
            if (handSize < 3)
                throw new ArgumentException("Hand size must be at least 3 for Three of a Kind.");

            List<string> hand = new List<string>();

            Rank threeOfAKindRank = (Rank)UnityEngine.Random.Range(2, 15);

            hand.Add($"{Card.GetRankSymbol(Suit.Hearts, threeOfAKindRank, coloured)}");
            hand.Add($"{Card.GetRankSymbol(Suit.Diamonds, threeOfAKindRank, coloured)}");

            hand.Add(!string.IsNullOrEmpty(trumpCard)
                ? trumpCard
                : $"{Card.GetRankSymbol(Suit.Clubs, threeOfAKindRank, coloured)}");

            for (int i = 3; i < handSize; i++)
            {
                Rank randomRank;

                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (randomRank == threeOfAKindRank);

                Suit randomSuit = (Suit)UnityEngine.Random.Range(0, 4);

                hand.Add($"{Card.GetRankSymbol(randomSuit, randomRank, coloured)}");
            }

            return hand.ToArray();
        }



        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();
            string trumpCard = useTrump ? "6♥" : null;

            examples.Add(CreateExampleHand(cardCount, null));

            if (useTrump)
            {
                examples.Add(CreateExampleHand(cardCount, trumpCard));
            }

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }

    }
}
