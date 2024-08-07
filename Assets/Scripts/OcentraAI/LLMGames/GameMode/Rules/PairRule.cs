using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
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
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (GameMode == null || (GameMode != null && GameMode.NumberOfCards > MinNumberOfCard))
                return false;

            var rankCounts = GetRankCounts(hand);
            var pair = FindHighestPair(rankCounts);

            if (pair.HasValue)
            {
                bonusDetails = CalculateBonus(hand, pair.Value);
                return true;
            }

            return false;
        }

        private Rank? FindHighestPair(Dictionary<Rank, int> rankCounts)
        {
            return rankCounts.Where(kv => kv.Value >= 2)
                             .OrderByDescending(kv => kv.Key)
                             .Select(kv => (Rank?)kv.Key)
                             .FirstOrDefault();
        }

        private BonusDetails CalculateBonus(List<Card> hand, Rank pairRank)
        {
            int baseBonus = BonusValue;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Pair of {Card.GetRankSymbol(Suit.Spades, pairRank)}" };

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && pairRank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.PairBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                }

                if (hasTrumpCard && IsRankAdjacent(trumpCard.Rank, pairRank))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent Bonus: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
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

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();

            switch (cardCount)
            {
                case 3:
                    examples.Add(new[] { "A♠", "A♥", "7♣" });
                    if (useTrump) examples.Add(new[] { "6♥", "6♠", "9♣" });
                    break;
                case 4:
                    examples.Add(new[] { "5♦", "5♣", "Q♠", "3♥" });
                    if (useTrump) examples.Add(new[] { "7♠", "7♦", "6♥", "2♣" });
                    break;
                case 5:
                    examples.Add(new[] { "J♠", "J♥", "10♦", "2♣", "7♥" });
                    if (useTrump) examples.Add(new[] { "K♠", "K♦", "6♥", "Q♣", "8♥" });
                    break;
                case 6:
                    examples.Add(new[] { "A♠", "A♥", "Q♠", "10♥", "7♠", "3♦" });
                    if (useTrump) examples.Add(new[] { "3♠", "3♥", "6♥", "5♠", "9♦", "2♣" });
                    break;
                case 7:
                    examples.Add(new[] { "2♠", "2♥", "J♠", "10♥", "8♠", "5♦", "3♣" });
                    if (useTrump) examples.Add(new[] { "K♠", "K♥", "6♥", "Q♠", "J♦", "10♠", "7♣" });
                    break;
                case 8:
                    examples.Add(new[] { "4♠", "4♥", "A♠", "Q♥", "9♠", "5♦", "2♣", "K♦" });
                    if (useTrump) examples.Add(new[] { "J♠", "J♥", "6♥", "K♠", "3♦", "Q♠", "8♣", "2♥" });
                    break;
                case 9:
                    examples.Add(new[] { "A♠", "A♥", "2♠", "Q♥", "J♠", "5♦", "10♣", "3♦", "7♠" });
                    if (useTrump) examples.Add(new[] { "6♥", "6♠", "7♠", "10♦", "J♠", "3♥", "Q♠", "8♣", "2♦" });
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
