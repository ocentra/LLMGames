using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(MultipleFourOfAKind), menuName = "GameMode/Rules/MultipleFourOfAKind")]
    public class MultipleFourOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 8;

        public override string RuleName { get; protected set; } = $"{nameof(MultipleFourOfAKind)}";
        public override int BonusValue { get; protected set; } = 190;
        public override int Priority { get; protected set; } = 99;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            // Check for valid hand size (8 or 9 cards)
            if (hand.Count() < 8)
            {
                return false;
            }

            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();
            List<Rank> fourOfAKinds = rankCounts.Where(kv => kv.Value >= 4).Select(kv => kv.Key).ToList();

            if (fourOfAKinds.Count >= 2)
            {
                foreach (Rank rank in fourOfAKinds)
                {
                    if (rank is Rank.A or Rank.K)
                    {
                        return false; // full house will deal this
                    }
                }
 
                bonusDetail = CalculateBonus(fourOfAKinds);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(List<Rank> fourOfAKinds)
        {
            var value = 0;
            foreach (var rank in fourOfAKinds)
            {
                value += (int)rank;
            }

            int baseBonus = BonusValue * fourOfAKinds.Count * value;
            string bonusCalculationDescriptions = $"{BonusValue} * {fourOfAKinds.Count} * {value}";

            List<string> descriptions = new List<string> { $"Multiple Four of a Kinds: {string.Join(", ", fourOfAKinds.Select(rank => CardUtility.GetRankSymbol(Suit.Spades, rank)))}" };

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions);
        }

       
        public override bool Initialize(GameMode gameMode)
        {
            Description = "Two or more sets of four cards with the same rank.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 8; cardCount <= gameMode.NumberOfCards; cardCount++)
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
            if (handSize < 8)
            {
                Debug.LogError("Hand size must be at least 8 for Multiple Four of a Kind.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();
            List<Rank> usedRanks = new List<Rank>();

            for (int i = 0; i < 2; i++)
            {
                Rank fourOfAKindRank;
                do
                {
                    fourOfAKindRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (usedRanks.Contains(fourOfAKindRank));

                usedRanks.Add(fourOfAKindRank);

                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    hand.Add($"{CardUtility.GetRankSymbol(suit, fourOfAKindRank, coloured)}");
                }
            }

            for (int i = 8; i < handSize; i++)
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
