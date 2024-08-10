using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
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

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!GameMode.UseTrump || !VerifyNumberOfCards(hand)) return false;

            Card trumpCard = GetTrumpCard();
            int trumpCount = hand.Cards.Count(card => card.Rank == trumpCard.Rank);

            if (trumpCount == GameMode.NumberOfCards && GameMode.NumberOfCards is >= 3 and <= 4)
            {
                bonusDetail = CalculateBonus(hand);
                return true;
            }

            if (trumpCount == 4 && hand.Count() > 4)
            {
                var cards = hand.Cards.Where(card => card.Rank != trumpCard.Rank);
                Hand remainingCards = new Hand(cards.ToArray()) ;
                bonusDetail = CalculateBonus(hand, remainingCards);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand, Hand remainingCards = null)
        {
            int baseBonus = BonusValue * hand.Sum();
            List<string> descriptions = new List<string> { $"Trump of a Kind: All {GameMode.NumberOfCards} of Trump Cards" };

            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";
            int additionalBonus = 0;

            if (remainingCards != null)
            {
                (string desc, int bonus) = EvaluateRemainingCards(remainingCards);
                if (!string.IsNullOrEmpty(desc))
                {
                    descriptions.Add(desc);
                    additionalBonus += bonus;
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        private (string description, int bonus) EvaluateRemainingCards(Hand remainingHand)
        {
            if (IsRoyalSequence(remainingHand))
                return ("Royal Sequence: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.RoyalFlushBonus);

            if (IsSequence(remainingHand))
            {
                if (IsSameSuits(remainingHand))
                    return ("Straight Flush: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.StraightFlushBonus);

                return IsSameColorAndDifferentSuits(remainingHand) ? ("Same Color Sequence: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.SameColorBonus) : ("Sequence: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.SequenceBonus);
            }

            switch (remainingHand.Count())
            {
                case 5 when IsNOfAKind(remainingHand, Rank.A, 4) && remainingHand.Cards.Any(card => card.Rank == Rank.K):
                    return ("Four of a Kind: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.FourOfKindBonus);
                case 4 when IsNOfAKind(remainingHand, Rank.A, 4):
                    return ("Four of a Kind: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.FourOfKindBonus);
                case 3 when IsNOfAKind(remainingHand, Rank.A, 3):
                    return ("Three of a Kind: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.ThreeOfKindBonus);
                case 2 when IsNOfAKind(remainingHand, Rank.A, 2):
                    return ("Two Pair: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.PairBonus);
                case 1 when IsNOfAKind(remainingHand, Rank.A, 1):
                    return ("High card: " + FormatCards(remainingHand), GameMode.TrumpBonusValues.HighCardBonus);
                default:
                    return ("", 0);
            }
        }

        private string FormatCards(Hand hand) => string.Join(", ", hand.Cards.Select(card => CardUtility.GetRankSymbol(card.Suit, card.Rank)));

        public override bool Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump) return false;

            Description = $"All {gameMode.NumberOfCards} Trump cards Rank in the hand.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                playerExamples.Add(CreateExampleString(cardCount, true));
                llmExamples.Add(CreateExampleString(cardCount, false));
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 3 || string.IsNullOrEmpty(trumpCard))
            {
                Debug.LogError("Hand size must be at least 3 and trump card must be specified for Trump of a Kind.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();
            Rank trumpRank = CardUtility.GetRankFromSymbol(trumpCard);

            for (int i = 0; i < Math.Min(handSize, 4); i++)
            {
                hand.Add($"{CardUtility.GetRankSymbol((Suit)i, trumpRank, coloured)}");
            }

            for (int i = 4; i < handSize; i++)
            {
                Rank randomRank;
                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (randomRank == trumpRank);

                Suit randomSuit = (Suit)UnityEngine.Random.Range(0, 4);
                hand.Add($"{CardUtility.GetRankSymbol(randomSuit, randomRank, coloured)}");
            }

            return hand.ToArray();
        }

        private string CreateExampleString(int cardCount, bool isPlayer)
        {
            string trumpCard = "6♥";
            string[] example = CreateExampleHand(cardCount, trumpCard, isPlayer);
            return string.Join(", ", example);
        }
    }
}