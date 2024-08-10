using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(PairRule), menuName = "GameMode/Rules/PairRule")]
    public class PairRule : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 2;
        public override string RuleName { get; protected set; } = $"{nameof(PairRule)}";
        public override int BonusValue { get; protected set; } = 100;
        public override int Priority { get; protected set; } = 87;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!VerifyNumberOfCards(hand)) return false;


            if (IsNOfAKind(hand, GameMode.NumberOfCards))
            {
                return false;
            }

            Rank? pair = FindNOfAKind(hand, 2);

            if (pair.HasValue)
            {
                bonusDetail = CalculateBonus(hand, pair.Value);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, Rank rank)
        {
            int baseBonus = BonusValue * ((int)rank * 2);
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Pair of {CardUtility.GetRankSymbol(Suit.Spades, rank)}" };
            string bonusCalculationDescriptions = $"{BonusValue} * ({(int)rank} *2)";

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.PairBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                }

                if (hasTrumpCard && IsRankAdjacent(trumpCard.Rank, rank))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent Bonus: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * ({(int)rank} *2) + {additionalBonus} ";
            }
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }


        public override bool Initialize(GameMode gameMode)
        {
            Description = "Pair of cards with the same rank, optionally considering Trump Wild Card.";

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
            if (handSize < 2)
            {
                Debug.LogError("Hand size must be at least 2 for a Pair.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();

            Rank pairRank = (Rank)UnityEngine.Random.Range(2, 15);

            hand.Add($"{CardUtility.GetRankSymbol(Suit.Hearts, pairRank, coloured)}");
            hand.Add($"{CardUtility.GetRankSymbol(Suit.Diamonds, pairRank, coloured)}");

            for (int i = 2; i < handSize; i++)
            {
                Rank randomRank;
                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (randomRank == pairRank);

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
