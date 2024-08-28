using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(Flush), menuName = "GameMode/Rules/Flush")]
    public class Flush : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(Flush)}";
        public override int BonusValue { get; protected set; } = 110;
        public override int Priority { get; protected set; } = 88;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (hand == null || !hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GetTrumpCard();

            if (IsFlushValid(hand, trumpCard))
            {
                bonusDetail = CalculateBonus(hand, false);
                return true;
            }

            if (GameMode.UseTrump && hand.HasTrumpCard(trumpCard))
            {
                Hand nonTrumpHand = hand.Where(c => c != trumpCard);
                if (IsFlushValid(nonTrumpHand, trumpCard))
                {
                    bonusDetail = CalculateBonus(hand, true);
                    return true;
                }
            }

            return false;
        }

        private bool IsFlushValid(Hand hand, Card trumpCard)
        {
            if (!hand.IsFlush() || hand.IsSequence() || hand.IsNOfAKind(2))
            {
                return false;
            }

            List<Card> nonHighRanks = hand.SelectHighRankingNonTrumpCards(trumpCard);
            if (hand.Contains(nonHighRanks))
            {
                return false;
            }

            return true;
        }

        private BonusDetail CalculateBonus(Hand hand, bool isTrumpAssisted)
        {
            int totalBonus = BonusValue * hand.Sum();
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { "Flush" };
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";

            if (isTrumpAssisted && GameMode.UseTrump && hand.Contains(GetTrumpCard()))
            {
                additionalBonus += GameMode.TrumpBonusValues.FlushBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FlushBonus}");

                Card trumpCard = GetTrumpCard();

                if (hand.IsTrumpInMiddle(trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.CardInMiddleBonus;
                    descriptions.Add($"Trump Card in Middle: +{GameMode.TrumpBonusValues.CardInMiddleBonus}");
                }

                if (hand.IsRankAdjacentToTrump(trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, totalBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardSymbol = null, bool coloured = true)
        {
            if (handSize < MinNumberOfCard)
            {
                Debug.LogError($"Hand size must be at least {MinNumberOfCard} for a Flush.");
                return Array.Empty<string>();
            }

            List<string> hand;

            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardSymbol);
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                hand = GeneratePotentialFlushHand(handSize, trumpCard, coloured);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Flush hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            }
            while (!IsFlushValid(HandUtility.ConvertFromSymbols(hand.ToArray()), trumpCard));

            return hand.ToArray();
        }

        private List<string> GeneratePotentialFlushHand(int handSize, Card trumpCard, bool coloured)
        {
            List<string> hand = new List<string>();
            Suit flushSuit = CardUtility.GetRandomSuit();
            List<Rank> selectedRanks = AvoidSequencesAndDuplicates(Rank.GetStandardRanks().Except(new[] { Rank.None }).ToList());

            for (int i = 0; i < handSize - 1; i++)
            {
                hand.Add(CardUtility.GetRankSymbol(flushSuit, selectedRanks[i], coloured));
            }

            if (trumpCard != null)
            {
                hand.Add(CardUtility.GetRankSymbol(trumpCard.Suit, trumpCard.Rank, coloured));
            }
            else
            {
                hand.Add(CardUtility.GetRankSymbol(flushSuit, selectedRanks[handSize - 1], coloured));
            }

            return hand;
        }

        private List<Rank> AvoidSequencesAndDuplicates(List<Rank> ranks)
        {
            ranks = ranks.OrderBy(rank => rank.Value).ToList();
            for (int i = 0; i < ranks.Count - 1; i++)
            {
                if (HandUtility.IsRankAdjacent(ranks[i], ranks[i + 1]) || ranks[i] == ranks[i + 1])
                {
                    ranks.RemoveAt(i + 1);
                    i--;
                }
            }

            return ranks;
        }

        private string CreateExampleString(int cardCount, bool useTrump = false)
        {
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Heart, Rank.Six, false) : null;
            string[] exampleHand = CreateExampleHand(cardCount, trumpCard, false);
            return exampleHand.Length > 0 ? string.Join(", ", exampleHand) : string.Empty;
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "All cards of the same suit, without forming a sequence, pair, or n of a kind, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = MinNumberOfCard; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string exampleHand = CreateExampleString(cardCount);
                if (!string.IsNullOrEmpty(exampleHand))
                {
                    llmExamples.Add(exampleHand);
                    playerExamples.Add(HandUtility.GetHandAsSymbols(exampleHand.Split(", ").ToList()));
                }

                if (gameMode.UseTrump)
                {
                    string exampleTrumpHand = CreateExampleString(cardCount, true);
                    if (!string.IsNullOrEmpty(exampleTrumpHand))
                    {
                        llmTrumpExamples.Add(exampleTrumpHand);
                        playerTrumpExamples.Add(HandUtility.GetHandAsSymbols(exampleTrumpHand.Split(", ").ToList()));
                    }
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}
