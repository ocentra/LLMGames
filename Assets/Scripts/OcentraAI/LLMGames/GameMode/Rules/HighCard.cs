using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(HighCard), menuName = "GameMode/Rules/HighCard")]
    public class HighCard : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(HighCard)}";
        public override int BonusValue { get; protected set; } = 90;
        public override int Priority { get; protected set; } = 86;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (!VerifyNumberOfCards(hand)) return false;

            Card trumpCard = GetTrumpCard();

            // Check for pairs, three of a kind, or four of a kind
            for (int i = 2; i <= 4; i++)
            {
                Rank? rank = FindNOfAKind(hand, i);
                if (rank.HasValue)
                {
                    return false;
                }
            }

            // Check for sequence
            if (IsSequence(hand))
            {
                return false;
            }

            // Check for TrumpOfAKind or full house rule
            if (IsFullHouseOrTrumpOfKind(hand))
            {
                return false;
            }

            // Check if hand contains the trump card
            if (hand.Contains(trumpCard))
            {
                bonusDetails = CalculateBonus(trumpCard, true);
                return true;
            }

            // Check for highest card from ranks J, Q, K, A
            Card highCard = FindHighestCard(hand);

            if (highCard != null)
            {
                List<Player> activePlayers = PlayerManager.Instance.GetActivePlayers();
                bool isHighestCard = true;

                foreach (Player player in activePlayers)
                {
                    if (player.Hand != hand)
                    {
                        Card otherPlayerHighCard = FindHighestCard(player.Hand);
                        if (otherPlayerHighCard != null && otherPlayerHighCard.Rank > highCard.Rank)
                        {
                            isHighestCard = false;
                            break;
                        }
                    }
                }

                if (isHighestCard)
                {
                    bonusDetails = CalculateBonus(highCard, false);
                    return true;
                }
            }

            return false;
        }

        private Card FindHighestCard(List<Card> cards)
        {
            Card highestCard = null;
            foreach (Card card in cards)
            {
                if (card.Rank >= Rank.J && (highestCard == null || card.Rank > highestCard.Rank))
                {
                    highestCard = card;
                }
            }
            return highestCard;
        }


        private BonusDetails CalculateBonus(Card highCard, bool isTrump)
        {
            int baseBonus = BonusValue;
            int additionalBonus = isTrump ? GameMode.TrumpBonusValues.HighCardBonus : 0;
            List<string> descriptions = new List<string> { $"High Card: {Card.GetRankSymbol(highCard.Suit, highCard.Rank)}" };

            if (isTrump)
            {
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.HighCardBonus}");
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "The highest card in the hand, with the trump card being the highest possible.";

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

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(" ", isPlayer ? ConvertCardSymbols(example) : example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
