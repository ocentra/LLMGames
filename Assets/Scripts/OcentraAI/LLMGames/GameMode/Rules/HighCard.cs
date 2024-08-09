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

        public override bool Evaluate(List<Card> hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!VerifyNumberOfCards(hand)) return false;

            Card trumpCard = GetTrumpCard();

            // Check if the current hand has any other rule applied
            for (int i = 2; i <= 4; i++)
            {
                Rank? rank = FindNOfAKind(hand, i);
                if (rank.HasValue)
                {
                    return false;
                }
            }

            if (IsSequence(hand) || IsFullHouseOrTrumpOfKind(hand) || IsSameSuits(hand))
            {
                return false;
            }

            // Collect all players' high cards if no other rules are applied
            List<(Player player, Card highCard)> highCardPlayers = new List<(Player player, Card highCard)>();

            List<Player> activePlayers = PlayerManager.Instance.GetActivePlayers();

            foreach (Player player in activePlayers)
            {
                List<Card> playerHand = player.Hand;

                // Check for other rules in the player's hand
                bool playerHasOtherRule = false;
                for (int i = 2; i <= 4; i++)
                {
                    Rank? rank = FindNOfAKind(playerHand, i);
                    if (rank.HasValue)
                    {
                        playerHasOtherRule = true;
                        break;
                    }
                }

                if (playerHasOtherRule || IsSequence(playerHand) || IsFullHouseOrTrumpOfKind(playerHand) || IsSameSuits(playerHand))
                {
                    continue;
                }

                // Check if player's hand contains the trump card
                if (playerHand.Contains(trumpCard))
                {
                    bonusDetail = CalculateBonus(trumpCard, true);
                    return true;
                }

                // Find the highest card in the player's hand
                Card playerHighCard = FindHighestCard(playerHand);
                if (playerHighCard != null)
                {
                    highCardPlayers.Add((player, playerHighCard));
                }
            }

            // If there are no other rules applied, award the highest card bonus
            if (highCardPlayers.Count > 0)
            {
                (Player player, Card highCard) highestCardPlayer = highCardPlayers.OrderByDescending(p => p.highCard.Rank).First();
                if (highestCardPlayer.player.Hand == hand)
                {
                    bonusDetail = CalculateBonus(highestCardPlayer.highCard, false);
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

        private BonusDetail CalculateBonus(Card highCard, bool isTrump)
        {
            int baseBonus = BonusValue * (int)highCard.Rank;
            int additionalBonus = isTrump ? GameMode.TrumpBonusValues.HighCardBonus : 0;
            string bonusCalculationDescriptions = $"{BonusValue} * {(int)highCard.Rank}";

            List<string> descriptions = new List<string> { $"High Card: {Card.GetRankSymbol(highCard.Suit, highCard.Rank)}" };

            if (isTrump)
            {
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.HighCardBonus}");
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * {(int)highCard.Rank} + {additionalBonus} ";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
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
