using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(MultipleTriplets), menuName = "GameMode/Rules/MultipleTriplets")]
    public class MultipleTriplets : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 6;
        public override string RuleName { get; protected set; } = $"{nameof(MultipleTriplets)}";
        public override int BonusValue { get; protected set; } = 170;
        public override int Priority { get; protected set; } = 97;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            // Check for valid hand size (6 to 9 cards)
            if (hand.Count() < 6)
            {
                return false;
            }

            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();
            List<Rank> triplets = rankCounts.Where(kv => kv.Value >= 3).Select(kv => kv.Key).ToList();

            if (triplets.Count >= 2)
            {
                foreach (Rank rank in triplets)
                {
                    if (rank is Rank.A or Rank.K)
                    {
                        return false;
                    }
                }
                bonusDetail = CalculateBonus(triplets);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(List<Rank> triplets)
        {
            int value = 0;
            foreach (Rank rank in triplets)
            {
                value += (int)rank;
            }
            int baseBonus = BonusValue * triplets.Count * value;

            string bonusCalculationDescriptions = $"{BonusValue} * {triplets.Count} * {value}";


            List<string> descriptions = new List<string> { $"Multiple Triplets: {string.Join(", ", triplets.Select(rank => CardUtility.GetRankSymbol(Suit.Spades, rank)))}" };

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions);
        }


        public override bool Initialize(GameMode gameMode)
        {
            Description = "Two or more triplets of cards with different ranks.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 6; cardCount <= gameMode.NumberOfCards; cardCount++)
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
            if (handSize < 6)
            {
                Debug.LogError("Hand size must be at least 6 for Multiple Triplets.");
                return Array.Empty<string>();

            }

            List<string> hand = new List<string>();
            List<Rank> usedRanks = new List<Rank>();

            for (int i = 0; i < 2; i++)
            {
                Rank tripletRank;
                do
                {
                    tripletRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (usedRanks.Contains(tripletRank));

                usedRanks.Add(tripletRank);

                for (int j = 0; j < 3; j++)
                {
                    hand.Add($"{CardUtility.GetRankSymbol((Suit)j, tripletRank, coloured)}");
                }
            }

            for (int i = 6; i < handSize; i++)
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
