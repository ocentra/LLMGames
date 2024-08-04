using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
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
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (GameMode == null || (GameMode != null && GameMode.NumberOfCards > MinNumberOfCard))
                return false;

            // Check for valid hand size (6 to 9 cards)
            if (hand.Count < 6)
            {
                return false;
            }

            var rankCounts = GetRankCounts(hand);
            var triplets = rankCounts.Where(kv => kv.Value >= 3).Select(kv => kv.Key).ToList();

            if (triplets.Count >= 2)
            {
                bonusDetails = CalculateBonus(triplets);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Rank> triplets)
        {
            int baseBonus = BonusValue * triplets.Count * 3;
            var descriptions = new List<string> { $"Multiple Triplets: {string.Join(", ", triplets.Select(rank => Card.GetRankSymbol(Suit.Spades, rank)))}" };

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions);
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        public override bool Initialize(GameMode gameMode)
        {
            Description = "Two or more triplets of cards with different ranks.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 6; cardCount <= gameMode.NumberOfCards; cardCount++)
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
                case 6:
                    examples.Add(new[] { "A♠", "A♥", "A♦", "K♠", "K♥", "K♦" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♥", "A♦", "K♠", "K♥", "6♥" });
                    break;
                case 7:
                    examples.Add(new[] { "Q♠", "Q♥", "Q♦", "J♠", "J♥", "J♦", "9♠" });
                    if (useTrump) examples.Add(new[] { "Q♠", "Q♥", "Q♦", "J♠", "J♥", "6♥", "9♠" });
                    break;
                case 8:
                    examples.Add(new[] { "A♠", "A♥", "A♦", "K♠", "K♥", "K♦", "Q♠", "Q♥" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♥", "A♦", "K♠", "K♥", "K♦", "Q♠", "6♥" });
                    break;
                case 9:
                    examples.Add(new[] { "A♠", "A♥", "A♦", "Q♠", "Q♥", "Q♦", "J♠", "J♥", "J♦" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♥", "A♦", "Q♠", "Q♥", "Q♦", "J♠", "J♥", "6♥" });
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
