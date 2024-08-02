using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(ThreeOfAKind), menuName = "Rules/ThreeOfAKind")]
    public class ThreeOfAKind : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(ThreeOfAKind)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            var rankCounts = GetRankCounts(hand);
            var threeOfAKind = FindNOfAKind(rankCounts, 3);
            var trumpCard = GetTrumpCard();

            // Check for TrumpOfAKind rule
            if (rankCounts.TryGetValue(trumpCard.Rank, out int trumpRankCount) && trumpRankCount >= 3)
            {
                return false; // TrumpOfAKind will handle this
            }

            if (threeOfAKind.HasValue)
            {
                bonusDetails = CalculateBonus(hand, threeOfAKind.Value);
                return true;
            }

            var twoOfAKind = FindNOfAKind(rankCounts, 2);

            if (twoOfAKind.HasValue && HasTrumpCard(hand))
            {
                bonusDetails = CalculateBonus(hand, twoOfAKind.Value);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Card> hand, Rank rank)
        {
            int baseBonus = BonusValue * 3;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Three of a Kind: {Card.GetRankSymbol(Suit.Spades, rank)}" };

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.ThreeOfKindBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.ThreeOfKindBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override void Initialize(GameMode gameMode)
        {
            RuleName = "Three of a Kind Rule";
            Description = "Three cards of the same rank, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                var playerExample = CreateExampleString(cardCount, true);
                var llmExample = CreateExampleString(cardCount, false);

                playerExamples.Add(playerExample);
                llmExamples.Add(llmExample);

                if (gameMode.UseTrump)
                {
                    var playerTrumpExample = CreateExampleString(cardCount, true, true);
                    var llmTrumpExample = CreateExampleString(cardCount, false, true);
                    playerTrumpExamples.Add(playerTrumpExample);
                    llmTrumpExamples.Add(llmTrumpExample);
                }
            }

            CreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }




        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();

            switch (cardCount)
            {
                case 3:
                    examples.Add(new[] { "5♠", "5♦", "5♣" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥" });
                    break;
                case 4:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "K♥" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥", "8♥" });
                    break;
                case 5:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "10♠", "8♦" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥", "8♦", "10♠" });
                    break;
                case 6:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "10♠", "8♦", "J♣" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥", "8♦", "J♣", "10♠" });
                    break;
                case 7:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "10♠", "8♦", "J♣", "Q♠" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥", "8♦", "J♣", "Q♠", "10♠" });
                    break;
                case 8:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "10♠", "8♦", "J♣", "Q♠", "9♣" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥", "8♦", "J♣", "Q♠", "9♣", "10♠" });
                    break;
                case 9:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "10♠", "8♦", "J♣", "Q♠", "9♣", "K♥" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "6♥", "8♦", "J♣", "Q♠", "9♣", "10♠", "K♥" });
                    break;
                default:
                    break;
            }

            var exampleStrings = examples.Select(example =>
                string.Join(", ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
