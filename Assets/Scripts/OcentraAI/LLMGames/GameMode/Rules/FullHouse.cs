using OcentraAI.LLMGames.Scriptable;
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

        public override bool Evaluate(List<Card> hand, out BonusDetail bonusDetail)
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



            int requiredCount = Math.Min(4, hand.Count);
            int availableSlots = hand.Count;

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
                    requiredCount = Math.Min(4, availableSlots);

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

        private BonusDetail CalculateBonus(List<Card> hand)
        {
            int baseBonus = BonusValue * CalculateHandValue(hand);
            string bonusCalculationDescriptions = $"{BonusValue} * {CalculateHandValue(hand)}";

            int additionalBonus = 0;
            List<string> descriptions = new List<string> { "Full House" };

            if (GameMode.UseTrump && HasTrumpCard(hand))
            {
                additionalBonus += GameMode.TrumpBonusValues.FullHouseBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FullHouseBonus}");
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * {CalculateHandValue(hand)} + {additionalBonus} ";
            }
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Full House with exactly A, K, Q in order, using trump as replacement if available";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            int cardCount = gameMode.NumberOfCards;
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

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();

            switch (cardCount)
            {
                case 3:
                    examples.Add(new[] { "A♠", "A♦", "A♣" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "6♥" });
                    break;
                case 4:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♥" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "6♥" });
                    break;
                case 5:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "6♥" });
                    break;
                case 6:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "6♥" });
                    break;
                case 7:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦", "K♣" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦", "6♥" });
                    break;
                case 8:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦", "K♣", "K♥" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦", "K♣", "6♥" });
                    break;
                case 9:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦", "K♣", "K♥", "Q♠" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "A♥", "K♠", "K♦", "K♣", "K♥", "6♥" });
                    break;
                default:
                    break;
            }

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}