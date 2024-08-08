using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(ThreeOfAKind), menuName = "GameMode/Rules/ThreeOfAKind")]
    public class ThreeOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(ThreeOfAKind)}";
        public override int BonusValue { get; protected set; } = 125;
        public override int Priority { get; protected set; } = 91;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            if (!VerifyNumberOfCards(hand)) return false;

            Dictionary<Rank, int> rankCounts = GetRankCounts(hand);
            Rank? threeOfAKind = FindNOfAKind(hand, 3);

            // Check for TrumpOfAKind or full house rule
            if (IsFullHouseOrTrumpOfKind(hand))
            {
                return false; // TrumpOfAKind or FullHouse will handle this
            }



            if (threeOfAKind.HasValue)
            {
                bonusDetails = CalculateBonus(hand, threeOfAKind.Value);
                return true;
            }

            Rank? twoOfAKind = FindNOfAKind(hand, 2);

            if (twoOfAKind.HasValue && HasTrumpCard(hand))
            {
                bonusDetails = CalculateBonus(hand, twoOfAKind.Value);
                return true;
            }

            return false;
        }



        private BonusDetails CalculateBonus(List<Card> hand, Rank rank)
        {
            int baseBonus = BonusValue * (int)rank;
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Three of a Kind: {Card.GetRankSymbol(Suit.Spades, rank)}" };

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && rank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.ThreeOfKindBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.ThreeOfKindBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "Three cards of the same rank, optionally considering Trump Wild Card.";

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

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
