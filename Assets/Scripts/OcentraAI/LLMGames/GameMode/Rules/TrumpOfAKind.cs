using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(TrumpOfAKind), menuName = "GameMode/Rules/TrumpOfAKind")]
    public class TrumpOfAKind : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(TrumpOfAKind)}";
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override int BonusValue { get; protected set; } = 160;
        public override int Priority { get; protected set; } = 96;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            if (!GameMode.UseTrump) return false;
            if (!VerifyNumberOfCards(hand)) return false;

            Card trumpCard = GetTrumpCard();
            int trumpRank = (int)trumpCard.Rank;
            List<Card> trumpCards = hand.Where(card => (int)card.Rank == trumpRank).ToList();

            if (trumpCards.Count == GameMode.NumberOfCards && GameMode.NumberOfCards is >= 3 and <= 4) // case 3 and 4 card game
            {
                bonusDetails = CalculateBonus();
                return true;
            }

            if (trumpCards.Count == 4 && hand.Count > 4) // case 5 to 9 card game
            {
                List<Card> remainingCards = hand.Except(trumpCards).ToList();
                bonusDetails = CalculateBonus(remainingCards);
                return true;
            }

            return false;
        }



        private BonusDetails CalculateBonus(List<Card> remainingCards = null)
        {
            int baseBonus = BonusValue * CalculateHandValue(remainingCards);
            List<string> descriptions = new List<string> { $"Trump of a Kind: All {GameMode.NumberOfCards} of Trump Cards" };

            if (remainingCards != null)
            {

                if (IsRoyalSequence(remainingCards))
                {
                    descriptions.Add("Royal Sequence: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.RoyalFlushBonus;
                }
                else if (IsSequence(remainingCards) && IsSameSuits(remainingCards))
                {
                    descriptions.Add("Straight Flush: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.StraightFlushBonus;
                }
                else if (IsSequence(remainingCards) && IsSameColorAndDifferentSuits(remainingCards))
                {
                    descriptions.Add("Same Color Sequence: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.SameColorBonus;
                }
                else if (IsSequence(remainingCards))
                {
                    descriptions.Add("Sequence: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.SequenceBonus;
                }
                else if (remainingCards.Count == 5 && IsNOfAKind(remainingCards, Rank.A, 4) && remainingCards.Any(card => card.Rank == Rank.K)) // in 9 card game 4 trump remaining is 5 
                {
                    descriptions.Add("Four of a Kind: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.FourOfKindBonus;
                }
                else if (remainingCards.Count == 4 && IsNOfAKind(remainingCards, Rank.A, 4)) // in 8 card game 4 trump remaining is 4
                {
                    descriptions.Add("Four of a Kind: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.FourOfKindBonus;
                }
                else if (remainingCards.Count == 3 && IsNOfAKind(remainingCards, Rank.A, 3)) // in 7 card game 4 trump remaining is 3
                {
                    descriptions.Add("Three of a Kind: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.ThreeOfKindBonus;
                }
                else if (remainingCards.Count == 2 && IsNOfAKind(remainingCards, Rank.A, 2)) // in 6 card game 4 trump remaining is 2
                {
                    descriptions.Add("Two Pair: " + string.Join(", ", remainingCards.Select(card => Card.GetRankSymbol(card.Suit, card.Rank))));
                    baseBonus += GameMode.TrumpBonusValues.PairBonus;
                }
                else if (remainingCards.Count == 1 && IsNOfAKind(remainingCards, Rank.A, 1)) // in 5 card game 4 trump remaining is 1
                {
                    descriptions.Add($"High card: ");
                    baseBonus += GameMode.TrumpBonusValues.HighCardBonus;
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions);
        }

        public override bool Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump) return false;

            Description = $"All {gameMode.NumberOfCards} Trump cards Rank in the hand.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string playerExample = CreateExampleString(cardCount, true);
                string llmExample = CreateExampleString(cardCount, false);

                playerExamples.Add(playerExample);
                llmExamples.Add(llmExample);
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer)
        {
            List<string[]> examples = new List<string[]>();

            switch (cardCount)
            {
                case 3:
                    examples.Add(new[] { "6♥", "6♠", "6♦" });
                    break;
                case 4:
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣" });
                    break;
                case 5:
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♠" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "2♣" });
                    break;
                case 6:
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♠", "A♦" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "2♣", "3♦" });
                    break;
                case 7:
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♠", "A♦", "A♣" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♥", "K♥", "Q♥" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "5♣", "3♦", "9♥" });
                    break;
                case 8:
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♠", "A♦", "A♣", "A♥" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♥", "K♥", "Q♥", "J♥" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "5♣", "3♦", "9♥", "2♠" });
                    break;
                case 9:
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♠", "A♦", "A♣", "A♥", "K♣" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "A♥", "K♥", "Q♥", "J♥", "10♥" });
                    examples.Add(new[] { "6♥", "6♠", "6♦", "6♣", "5♣", "3♦", "9♥", "2♠", "4♦" });
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
