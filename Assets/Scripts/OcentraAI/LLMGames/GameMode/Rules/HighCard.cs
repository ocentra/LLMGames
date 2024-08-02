using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(HighCard), menuName = "Rules/HighCard")]
    public class HighCard : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(HighCard)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 50;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            var trumpCard = GetTrumpCard();

            // Check if hand contains the trump card
            if (hand.Contains(trumpCard))
            {
                bonusDetails = CalculateBonus(trumpCard, true);
                return true;
            }

            // If no trump card is found, check for the highest card
            var highCard = hand.OrderByDescending(card => card.Rank).FirstOrDefault();
            if (highCard != null)
            {
                bonusDetails = CalculateBonus(highCard, false);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(Card highCard, bool isTrump)
        {
            int baseBonus = BonusValue;
            int additionalBonus = isTrump ? GameMode.TrumpBonusValues.HighCardBonus : 0;
            var descriptions = new List<string> { $"High Card: {Card.GetRankSymbol(highCard.Suit, highCard.Rank)}" };

            if (isTrump)
            {
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.HighCardBonus}");
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override void Initialize(GameMode gameMode)
        {
            RuleName = "High Card Rule";
            Description = "The highest card in the hand, with the trump card being the highest possible.";

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
                    examples.Add(new[] { "2♠", "3♥", "J♦" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "6♥" });
                    break;
                case 4:
                    examples.Add(new[] { "2♠", "3♥", "5♦", "J♣" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "5♦", "6♥" });
                    break;
                case 5:
                    examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "J♠" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "6♥" });
                    break;
                case 6:
                    examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "J♠", "9♦" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "6♥", "9♦" });
                    break;
                case 7:
                    examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "J♠", "9♦", "10♣" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "6♥", "9♦", "10♣" });
                    break;
                case 8:
                    examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "J♠", "9♦", "10♣", "K♥" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "6♥", "9♦", "10♣", "K♥" });
                    break;
                case 9:
                    examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "J♠", "9♦", "10♣", "K♥", "Q♠" });
                    if (useTrump) examples.Add(new[] { "2♠", "3♥", "5♦", "7♣", "6♥", "9♦", "10♣", "K♥", "Q♠" });
                    break;
                default:
                    break;
            }

            var exampleStrings = examples.Select(example =>
                string.Join(" ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }


    }
}
