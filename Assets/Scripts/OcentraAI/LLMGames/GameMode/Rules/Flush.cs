using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
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

        public override bool Evaluate(List<Card> hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!VerifyNumberOfCards(hand)) return false;

            // Check for natural flush
            if (IsSameSuits(hand))
            {
                bonusDetail = CalculateBonus(hand, false);
                return true;
            }

            

            // Check for trump-assisted flush
            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);
                if (hasTrumpCard)
                {
                    List<Card> nonTrumpCards = hand.Where(c => c != trumpCard).ToList();
                    if (nonTrumpCards.All(card => card.Suit == nonTrumpCards[0].Suit))
                    {
                        bonusDetail = CalculateBonus(hand, true);
                        return true;
                    }
                }
            }

            return false;
        }

        private BonusDetail CalculateBonus(List<Card> hand, bool isTrumpAssisted)
        {
            int baseBonus = BonusValue * CalculateHandValue(hand);
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Flush:" };
            string bonusCalculationDescriptions = $"{BonusValue} * {CalculateHandValue(hand)}";

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.FlushBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FlushBonus}");

                List<Card> orderedHand = hand.OrderBy(card => card.GetRankValue()).ToList();
                Card trumpCard = GetTrumpCard();

                // Check for CardInMiddleBonus
                if (IsTrumpInMiddle(orderedHand, trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.CardInMiddleBonus;
                    descriptions.Add($"Trump Card in Middle: +{GameMode.TrumpBonusValues.CardInMiddleBonus}");
                }

                // Check for RankAdjacentBonus
                if (IsRankAdjacentToTrump(orderedHand, trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * {CalculateHandValue(hand)} + {additionalBonus} ";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

       
        public override bool Initialize(GameMode gameMode)
        {
            Description = "All cards of the same suit, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string playerTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six);
            string llmTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six, false);

            Suit[] suits = new[] { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            foreach (Suit suit in suits)
            {
                string playerExample = string.Join(", ", Enumerable.Range(2, 5).Select(rank => Card.GetRankSymbol(suit, (Rank)rank)));
                playerExamples.Add(playerExample);

                string llmExample = string.Join(", ", Enumerable.Range(2, 5).Select(rank => Card.GetRankSymbol(suit, (Rank)rank, false)));
                llmExamples.Add(llmExample);
            }

            if (gameMode.UseTrump)
            {
                string trumpPlayerExample = $"{Card.GetRankSymbol(Suit.Hearts, Rank.Two)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Three)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Four)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Six)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Seven)} (Trump: {playerTrumpCardSymbol})";
                playerTrumpExamples.Add(trumpPlayerExample);

                string llmTrumpExample = $"{Card.GetRankSymbol(Suit.Hearts, Rank.Two, false)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Three, false)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Four, false)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Six, false)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Seven, false)} (Trump: {llmTrumpCardSymbol})";
                llmTrumpExamples.Add(llmTrumpExample);
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}
