using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
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

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GetTrumpCard();

            // Check if the current hand has any other rule applied
            for (int i = 2; i <= 4; i++)
            {
                Rank? rank = hand.TryGetHighestNOfKindRank(i);
                if (rank.HasValue)
                {
                    return false;
                }
            }

            if (hand.IsSequence() || hand.IsFullHouseOrTrumpOfKind(trumpCard, GameMode) || hand.IsSameSuits())
            {
                return false;
            }

            // Collect all players' high cards if no other rules are applied
            List<(Player player, Card highCard)> highCardPlayers = new List<(Player player, Card highCard)>();

            List<Player> activePlayers = PlayerManager.Instance.GetActivePlayers();

            foreach (Player player in activePlayers)
            {
                Hand playerHand = player.Hand;

                // Check for other rules in the player's hand
                bool playerHasOtherRule = false;
                for (int i = 2; i <= 4; i++)
                {
                    Rank? rank = playerHand.TryGetHighestNOfKindRank(i);
                    if (rank.HasValue)
                    {
                        playerHasOtherRule = true;
                        break;
                    }
                }

                if (playerHasOtherRule || playerHand.IsSequence() || playerHand.IsFullHouseOrTrumpOfKind(trumpCard, GameMode) || playerHand.IsSameSuits())
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
                Card playerHighCard = playerHand.FindHighestCard();
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



        private BonusDetail CalculateBonus(Card highCard, bool isTrump)
        {
            int baseBonus = BonusValue * (int)highCard.Rank;
            int additionalBonus = isTrump ? GameMode.TrumpBonusValues.HighCardBonus : 0;
            string bonusCalculationDescriptions = $"{BonusValue} * {(int)highCard.Rank}";

            List<string> descriptions = new List<string> { $"High Card: {CardUtility.GetRankSymbol(highCard.Suit, highCard.Rank)}" };

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

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 1)
            {
                Debug.LogError("Hand size must be at least 1 for High Card.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();
            List<Rank> usedRanks = new List<Rank>();

            // Ensure at least one high card (J or higher)
            Rank highCard = (Rank)UnityEngine.Random.Range((int)Rank.J, 15);
            Suit highCardSuit = (Suit)UnityEngine.Random.Range(0, 4);
            hand.Add($"{CardUtility.GetRankSymbol(highCardSuit, highCard, coloured)}");
            usedRanks.Add(highCard);

            for (int i = 1; i < handSize; i++)
            {
                Rank randomRank;
                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (usedRanks.Contains(randomRank));

                Suit randomSuit = (Suit)UnityEngine.Random.Range(0, 4);

                if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                {
                    hand.Add(trumpCard);
                }
                else
                {
                    hand.Add($"{CardUtility.GetRankSymbol(randomSuit, randomRank, coloured)}");
                    usedRanks.Add(randomRank);
                }
            }

            return hand.ToArray();
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();
            string trumpCard = useTrump ? "6♥" : null;

            examples.Add(CreateExampleHand(cardCount, null, isPlayer));
            if (useTrump)
            {
                examples.Add(CreateExampleHand(cardCount, trumpCard, isPlayer));
            }

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
