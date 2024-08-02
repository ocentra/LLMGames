using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FourOfAKind), menuName = "Rules/FourOfAKind")]
    public class FourOfAKind : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(FourOfAKind)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            var rankCounts = GetRankCounts(hand);
            var fourOfAKind = FindNOfAKind(rankCounts, 4);
            var trumpCard = GetTrumpCard();

            // Check for TrumpOfAKind rule
            if (rankCounts.TryGetValue(trumpCard.Rank, out int trumpRankCount) && trumpRankCount >= 4)
            {
                return false; // TrumpOfAKind will handle this
            }

            if (fourOfAKind.HasValue)
            {
                bonusDetails = CalculateBonus(hand, fourOfAKind.Value);
                return true;
            }

            var threeOfAKind = FindNOfAKind(rankCounts, 3);

            if (threeOfAKind.HasValue && HasTrumpCard(hand))
            {
                bonusDetails = CalculateBonus(hand, threeOfAKind.Value);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Card> hand, Rank rank)
        {
            int baseBonus = BonusValue ;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Four of a Kind: {Card.GetRankSymbol(Suit.Spades, rank)}" };

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.FourOfKindBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FourOfKindBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override void Initialize(GameMode gameMode)
        {
            RuleName = "Four of a Kind Rule";
            Description = "Four cards of the same rank, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 4; cardCount <= gameMode.NumberOfCards; cardCount++)
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
                case 4:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "5♥" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "5♣", "6♥" });
                    break;
                case 5:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "5♥", "A♦" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "5♣", "6♥", "9♥" });
                    break;
                case 6:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "5♥", "A♦", "A♣" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "5♣", "6♥", "A♦", "K♠" });
                    break;
                case 7:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "5♥", "A♦", "A♣", "K♠" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "5♣", "6♥", "A♦", "K♠", "Q♠" });
                    break;
                case 8:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "5♥", "A♦", "A♣", "K♠", "Q♦" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "5♣", "6♥", "A♦", "K♠", "Q♠", "J♦" });
                    break;
                case 9:
                    examples.Add(new[] { "5♠", "5♦", "5♣", "5♥", "A♦", "A♣", "K♠", "Q♦", "J♣" });
                    if (useTrump) examples.Add(new[] { "5♠", "5♦", "5♣", "6♥", "A♦", "K♠", "Q♠", "J♦", "10♠" });
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
