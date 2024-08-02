using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(MultiplePairsRule), menuName = "Rules/MultiplePairsRule")]
    public class MultiplePairsRule : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(MultiplePairsRule)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            // Check for valid hand size (4 to 9 cards)
            if (hand.Count < 4)
            {
                return false;
            }

            var rankCounts = GetRankCounts(hand);
            var pairs = FindPairs(rankCounts);

            if (pairs.Count >= 2)
            {
                bonusDetails = CalculateBonus(hand, pairs);
                return true;
            }

            return false;
        }

        private List<Rank> FindPairs(Dictionary<Rank, int> rankCounts)
        {
            return rankCounts.Where(kv => kv.Value >= 2)
                             .OrderByDescending(kv => kv.Key)
                             .Select(kv => kv.Key)
                             .ToList();
        }

        private BonusDetails CalculateBonus(List<Card> hand, List<Rank> pairs)
        {
            int baseBonus = BonusValue * pairs.Count * 2;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Multiple Pairs: {string.Join(", ", pairs.Select(rank => Card.GetRankSymbol(Suit.Spades, rank)))}" };

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard)
                {
                    if (pairs.Any(rank => rank == trumpCard.Rank))
                    {
                        additionalBonus += GameMode.TrumpBonusValues.PairBonus;
                        descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                    }

                    if (pairs.Any(rank => IsRankAdjacent(trumpCard.Rank, rank)))
                    {
                        additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                        descriptions.Add($"Trump Rank Adjacent Bonus: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                    }
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        public override void Initialize(GameMode gameMode)
        {
            RuleName = "Multiple Pairs Rule";
            Description = "Two or more pairs of cards with different ranks.";

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
                    examples.Add(new[] { "A♠", "A♥", "K♠", "K♥" });
                    if (useTrump) examples.Add(new[] { "6♥", "6♠", "Q♠", "Q♥" });
                    break;
                case 5:
                    examples.Add(new[] { "J♠", "J♥", "10♠", "10♥", "7♣" });
                    if (useTrump) examples.Add(new[] { "K♠", "K♦", "6♥", "6♠", "8♥" });
                    break;
                case 6:
                    examples.Add(new[] { "A♠", "A♥", "Q♠", "Q♥", "10♠", "10♣" });
                    if (useTrump) examples.Add(new[] { "3♠", "3♥", "6♥", "6♠", "9♦", "2♣" });
                    break;
                case 7:
                    examples.Add(new[] { "2♠", "2♥", "J♠", "J♥", "10♠", "10♣", "3♦" });
                    if (useTrump) examples.Add(new[] { "K♠", "K♥", "6♥", "6♠", "Q♠", "J♦", "8♣" });
                    break;
                case 8:
                    examples.Add(new[] { "4♠", "4♥", "A♠", "A♥", "Q♠", "Q♥", "9♠", "9♦" });
                    if (useTrump) examples.Add(new[] { "J♠", "J♥", "6♥", "6♠", "K♠", "Q♠", "8♣", "2♥" });
                    break;
                case 9:
                    examples.Add(new[] { "A♠", "A♥", "2♠", "2♥", "Q♠", "Q♥", "J♠", "J♦", "10♣" });
                    if (useTrump) examples.Add(new[] { "6♥", "6♠", "7♠", "7♦", "10♠", "10♥", "J♠", "Q♠", "2♦" });
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
