using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FourOfAKind), menuName = "GameMode/Rules/FourOfAKind")]
    public class FourOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 4;

        public override string RuleName { get; protected set; } = $"{nameof(FourOfAKind)}";
        public override int BonusValue { get; protected set; } = 135;
        public override int Priority { get; protected set; } = 93;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (!VerifyNumberOfCards(hand)) return false;

            Rank? fourOfAKind = FindNOfAKind(hand, 4);

            // Check for TrumpOfAKind or full house rule
            if (IsFullHouseOrTrumpOfKind(hand))
            {
                return false; // TrumpOfAKind or FullHouse will handle this
            }

            if (fourOfAKind.HasValue)
            {
                bonusDetails = CalculateBonus(hand, fourOfAKind.Value);
                return true;
            }

            Rank? threeOfAKind = FindNOfAKind(hand, 3);

            if (threeOfAKind.HasValue && HasTrumpCard(hand))
            {
                bonusDetails = CalculateBonus(hand, threeOfAKind.Value);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Card> hand, Rank rank)
        {
            int baseBonus = BonusValue * (int)rank;
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Four of a Kind: {Card.GetRankSymbol(Suit.Spades, rank)}" };

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.FourOfKindBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FourOfKindBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Four cards of the same rank, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 4; cardCount <= gameMode.NumberOfCards; cardCount++)
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

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
