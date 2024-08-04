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
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (GameMode == null || (GameMode != null && GameMode.NumberOfCards > MinNumberOfCard))
                return false;

            var rankCounts = GetRankCounts(hand);
            var trumpCard = GetTrumpCard();

            switch (hand.Count)
            {
                case 3:
                    if (IsFullHouseOrTrumpOfKind(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                case 4:
                    if (IsFullHouseOrTrumpOfKind(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                case 5:
                    if (IsFiveCardFullHouse(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                case 6:
                    if (IsSixCardFullHouse(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                case 7:
                    if (IsSevenCardFullHouse(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                case 8:
                    if (IsEightCardFullHouse(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                case 9:
                    if (IsNineCardFullHouse(rankCounts, trumpCard))
                    {
                        bonusDetails = CalculateBonus(hand);
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }


        private bool IsFiveCardFullHouse(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            return (IsNOfAKind(rankCounts, Rank.A, 4) && rankCounts.Values.Count(v => v == 1) == 1) ||
                   (IsNOfAKindOfTrump(rankCounts, trumpCard, 4) && rankCounts.Values.Count(v => v == 1) == 1);
        }

        private bool IsSixCardFullHouse(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            return (IsNOfAKind(rankCounts, Rank.A, 4) && rankCounts.Values.Count(v => v == 2) == 1) ||
                   (IsNOfAKindOfTrump(rankCounts, trumpCard, 4) && rankCounts.Values.Count(v => v == 2) == 1);
        }

        private bool IsSevenCardFullHouse(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            return (IsNOfAKind(rankCounts, Rank.A, 4) && rankCounts.Values.Count(v => v == 3) == 1) ||
                   (IsNOfAKindOfTrump(rankCounts, trumpCard, 4) && rankCounts.Values.Count(v => v == 3) == 1);
        }

        private bool IsEightCardFullHouse(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            return (IsNOfAKind(rankCounts, Rank.A, 4) && rankCounts.Values.Count(v => v == 4) == 1) ||
                   (IsNOfAKindOfTrump(rankCounts, trumpCard, 4) && rankCounts.Values.Count(v => v == 4) == 1);
        }

        private bool IsNineCardFullHouse(Dictionary<Rank, int> rankCounts, Card trumpCard)
        {
            return (IsNOfAKind(rankCounts, Rank.A, 4) && rankCounts.Values.Count(v => v == 5) == 1) ||
                   (IsNOfAKindOfTrump(rankCounts, trumpCard, 4) && rankCounts.Values.Count(v => v == 5) == 1);
        }

        private BonusDetails CalculateBonus(List<Card> hand)
        {
            int baseBonus = BonusValue;
            int additionalBonus = 0;
            var descriptions = new List<string> { "Full House" };

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard)
                {
                    additionalBonus += GameMode.TrumpBonusValues.FullHouseBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FullHouseBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }


        public override bool Initialize(GameMode gameMode)
        {
            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            var cardCount = gameMode.NumberOfCards;
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

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();

            switch (cardCount)
            {
                case 3:
                    examples.Add(new[] { "A♠", "A♦", "A♠" });
                    if (useTrump) examples.Add(new[] { "6♠", "6♦", "6♥" });
                    break;
                case 4:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠" });
                    if (useTrump) examples.Add(new[] { "6♠", "6♦", "6♣", "6♥" });
                    break;
                case 5:
                    examples.Add(new[] { "6♠", "6♦", "6♣", "6♥", "A♦" });
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "6♥" });
                    if (useTrump) examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♥" });
                    break;
                case 6:
                    examples.Add(new[] { "6♠", "6♦", "6♣", "6♥", "A♦", "A♣" });
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "6♣", "6♥" });
                    if (useTrump)
                    {
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "6♥" });
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♥" });
                    }
                    break;
                case 7:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "6♥", "6♣", "6♦" });
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "6♣", "6♥" });
                    if (useTrump)
                    {
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♣", "6♥" });
                        examples.Add(new[] { "A♠", "A♦", "A♣", "6♠", "6♦", "6♣", "6♥" });
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♣", "K♥" });
                    }
                    break;
                case 8:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "6♠", "6♦", "6♣", "6♥" });
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "6♣", "6♥" });
                    if (useTrump)
                    {
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "K♥", "6♥" });
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "K♥", "K♣" });
                    }
                    break;
                case 9:
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "6♠", "6♦", "6♣", "6♥", "K♠" });
                    examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "6♣", "6♣", "6♥" });
                    if (useTrump)
                    {
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "K♥", "6♣", "6♥" });
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "K♥", "K♣", "6♥" });
                        examples.Add(new[] { "A♠", "A♦", "A♣", "A♠", "K♠", "K♦", "K♥", "K♣", "Q♥" });
                    }
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
