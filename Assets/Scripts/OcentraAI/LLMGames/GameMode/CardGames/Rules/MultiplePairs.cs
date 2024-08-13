using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(MultiplePairsRule), menuName = "GameMode/Rules/MultiplePairsRule")]
    public class MultiplePairsRule : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 4;
        public override string RuleName { get; protected set; } = $"{nameof(MultiplePairsRule)}";
        public override int BonusValue { get; protected set; } = 130;
        public override int Priority { get; protected set; } = 92;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            // Check for valid hand size (4 to 9 cards)
            if (hand.Count() < 4)
            {
                return false;
            }



            if (hand.IsPair(GetTrumpCard(), GameMode.UseTrump, out List<Rank> pairRank))
            {
                if (pairRank.Count > 1)
                {
                    bonusDetail = CalculateBonus(hand,pairRank);

                }
                return true;
            }


            return false;
        }



        private BonusDetail CalculateBonus(Hand hand, List<Rank> pairs)
        {
            var value = 0;
            foreach (var rank in pairs)
            {
                value += (int)rank;
            }

            int baseBonus = BonusValue * pairs.Count * value;

            string bonusCalculationDescriptions = $"{BonusValue} * {pairs.Count} * {value}";


            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Multiple Pairs: {string.Join(", ", pairs.Select(rank => CardUtility.GetRankSymbol(Suit.Spades, rank)))}" };

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = hand.HasTrumpCard( GetTrumpCard());

                if (hasTrumpCard)
                {
                    if (pairs.Any(rank => rank == trumpCard.Rank))
                    {
                        additionalBonus += GameMode.TrumpBonusValues.PairBonus;
                        descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                    }

                    if (pairs.Any(rank => HandUtility.IsRankAdjacent(trumpCard.Rank, rank)))
                    {
                        additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                        descriptions.Add($"Trump Rank Adjacent Bonus: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                    }
                }
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * {pairs.Count} * {value} + {additionalBonus} ";
            }


            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

       
        public override bool Initialize(GameMode gameMode)
        {
            Description = "Two or more pairs of cards with different ranks.";

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
            {
                Debug.LogError("Hand size must be at least 4 for Multiple Pairs.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();
            List<Rank> usedRanks = new List<Rank>();

            for (int i = 0; i < 2; i++)
            {
                Rank pairRank;
                do
                {
                    pairRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (usedRanks.Contains(pairRank));

                usedRanks.Add(pairRank);

                hand.Add($"{CardUtility.GetRankSymbol(Suit.Hearts, pairRank, coloured)}");
                hand.Add($"{CardUtility.GetRankSymbol(Suit.Diamonds, pairRank, coloured)}");
            }

            for (int i = 4; i < handSize; i++)
            {
                Rank randomRank;
                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (usedRanks.Contains(randomRank));

                Suit randomSuit = (Suit)UnityEngine.Random.Range(0, 4);

                if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                {
                    hand.Add(trumpCard);
                }
                else
                {
                    hand.Add($"{CardUtility.GetRankSymbol(randomSuit, randomRank, coloured)}");
                    usedRanks.Add(randomRank);
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
