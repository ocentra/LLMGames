using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FullHouse), menuName = "GameMode/Rules/FullHouse")]
    public class FullHouse : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(FullHouse)}";
        public override int BonusValue { get; protected set; } = 150;
        public override int Priority { get; protected set; } = 95;

        private readonly Rank[] ranksInOrder = { Rank.A, Rank.K, Rank.Q };



        public override bool Initialize(GameMode gameMode)
        {
            Description = "Full House with exactly A, K, Q in order, using trump as replacement if available.";

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



        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!VerifyNumberOfCards(hand)) return false;

            if (IsNOfAKindOfTrump(hand, GameMode.NumberOfCards))
            {
                return false; // trump of kind will handle this
            }

            Dictionary<Rank, int> rankCounts = GetRankCounts(hand);
            Card trumpCard = GameMode.UseTrump ? GetTrumpCard() : null;
            bool hasTrump = trumpCard != null && hand.Contains(trumpCard);

            int requiredCount = Math.Min(3, hand.Count() - 2);
            int availableSlots = hand.Count();

            foreach (Rank rank in ranksInOrder)
            {
                int count = rankCounts.GetValueOrDefault(rank);
                if (count > requiredCount)
                    count = requiredCount;

                if (count < requiredCount && hasTrump)
                {
                    count++;
                    hasTrump = false; // Use trump card only once
                }

                if (count == requiredCount)
                {
                    availableSlots -= count;
                    requiredCount = Math.Min(3, availableSlots);

                    if (availableSlots == 0)
                    {
                        bonusDetail = CalculateBonus(hand);
                        return true;
                    }
                }
                else
                {
                    return false; // Not enough cards of this rank, and can't form Full House
                }
            }

            return false; // Shouldn't reach here, but just in case
        }

        private BonusDetail CalculateBonus(Hand hand)
        {
            int baseBonus = BonusValue * hand.Sum();
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";

            int additionalBonus = 0;
            List<string> descriptions = new List<string> { "Full House" };

            if (GameMode.UseTrump && HasTrumpCard(hand))
            {
                additionalBonus += GameMode.TrumpBonusValues.FullHouseBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FullHouseBonus}");
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
                Debug.LogError("Hand size must be at least 3 for a Full House.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();

            Rank trumpRank = !string.IsNullOrEmpty(trumpCard) ? CardUtility.GetRankFromSymbol(trumpCard) : Rank.None;

            Rank highestRank = ranksInOrder.FirstOrDefault(rank => rank != trumpRank);
            Rank secondHighestRank = ranksInOrder.FirstOrDefault(rank => rank != highestRank && rank != trumpRank);

            if (highestRank == Rank.None || secondHighestRank == Rank.None)
            {
                Debug.LogError("Invalid rank assignment, check the logic.");
                return Array.Empty<string>();
            }

            int highestRankCount = Math.Min(handSize, 4); 
            for (int i = 0; i < highestRankCount; i++)
            {
                Suit suit = (Suit)((i % 4) + 1); 
                hand.Add($"{CardUtility.GetRankSymbol(suit, highestRank, coloured)}");
            }

            for (int i = highestRankCount; i < handSize; i++)
            {
                if (i == handSize - 1 && !string.IsNullOrEmpty(trumpCard))
                {
                    hand.Add(trumpCard); 
                }
                else
                {
                    Rank randomRank;
                    do
                    {
                        randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                    } while (randomRank == highestRank || randomRank == trumpRank); 

                    Suit randomSuit = (Suit)UnityEngine.Random.Range(1, 5); 
                    hand.Add($"{CardUtility.GetRankSymbol(randomSuit, randomRank, coloured)}");
                }
            }

            return hand.ToArray();
        }


        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            string trumpCard = "6♥";

            List<string[]> examples = new List<string[]> { CreateExampleHand(cardCount, trumpCard, isPlayer) };

            if (useTrump)
            {
                examples.Add(CreateExampleHand(cardCount, trumpCard, isPlayer));
            }

            IEnumerable<string> exampleStrings = examples.Select(example => string.Join(", ", example.Where(card => !card.Contains("None")))
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
