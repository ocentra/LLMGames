using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (!VerifyNumberOfCards(hand)) return false;

            // Check for valid hand size (8 or 9 cards)
            if (hand.Count < 8)
            {
                return false;
            }

            Dictionary<Rank, int> rankCounts = GetRankCounts(hand);
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
 
                bonusDetails = CalculateBonus(fourOfAKinds);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Rank> fourOfAKinds)
        {
            var value = 0;
            foreach (var rank in fourOfAKinds)
            {
                value += (int)rank;
            }

            int baseBonus = BonusValue * fourOfAKinds.Count * value;
            List<string> descriptions = new List<string> { $"Multiple Four of a Kinds: {string.Join(", ", fourOfAKinds.Select(rank => Card.GetRankSymbol(Suit.Spades, rank)))}" };

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions);
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

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();

            switch (cardCount)
            {
                case 8:
                    examples.Add(new[] { "A♠", "A♥", "A♦", "A♣", "K♠", "K♥", "K♦", "K♣" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♥", "A♦", "A♣", "K♠", "K♥", "K♦", "6♥" });
                    break;
                case 9:
                    examples.Add(new[] { "Q♠", "Q♥", "Q♦", "Q♣", "J♠", "J♥", "J♦", "J♣", "9♥" });
                    if (useTrump) examples.Add(new[] { "Q♠", "Q♥", "Q♦", "Q♣", "J♠", "J♥", "J♦", "6♥", "10♣" });
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
