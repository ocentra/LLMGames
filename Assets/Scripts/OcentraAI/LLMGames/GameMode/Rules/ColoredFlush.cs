using OcentraAI.LLMGames.Scriptable;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(ColoredFlush), menuName = "Rules/ColoredFlush")]
    public class ColoredFlush : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = "Colored Flush";
        public override int BonusValue { get; protected set; } = 25;
        public override int Priority { get; protected set; } = 75; // Between Flush and StraightFlush

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            bool isRed = hand.All(card => card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds);
            bool isBlack = hand.All(card => card.Suit == Suit.Spades || card.Suit == Suit.Clubs);

            if (isRed || isBlack)
            {
                string color = isRed ? "Red" : "Black";
                bonusDetails = CalculateBonus(hand, color);
                return true;
            }

            // Check for trump-assisted colored flush
            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);
                if (hasTrumpCard)
                {
                    var nonTrumpCards = hand.Where(c => c != trumpCard).ToList();
                    isRed = nonTrumpCards.All(card => card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds);
                    isBlack = nonTrumpCards.All(card => card.Suit == Suit.Spades || card.Suit == Suit.Clubs);

                    if (isRed || isBlack)
                    {
                        string color = isRed ? "Red" : "Black";
                        bonusDetails = CalculateBonus(hand, color, true);
                        return true;
                    }
                }
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Card> hand, string color, bool isTrumpAssisted = false)
        {
            int baseBonus = BonusValue;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"{color} Colored Flush" };

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.FlushBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FlushBonus}");

                var orderedHand = hand.OrderBy(card => card.GetRankValue()).ToList();
                var trumpCard = GetTrumpCard();

                if (IsTrumpInMiddle(orderedHand, trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.CardInMiddleBonus;
                    descriptions.Add($"Trump Card in Middle: +{GameMode.TrumpBonusValues.CardInMiddleBonus}");
                }

                if (IsRankAdjacentToTrump(orderedHand, trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override void Initialize(GameMode gameMode)
        {
            RuleName = "Colored Flush Rule";
            Description = "All cards of the same color (red or black), optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string playerTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six);
            string llmTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six, false);

            playerExamples.Add("Red: 2♥, 5♥, 7♦, J♦, A♥");
            playerExamples.Add("Black: A♠, 3♠, 8♣, K♣, 2♠");

            llmExamples.Add("Red: 2♥, 5♥, 7♦, J♦, A♥");
            llmExamples.Add("Black: A♠, 3♠, 8♣, K♣, 2♠");

            if (gameMode.UseTrump)
            {
                playerTrumpExamples.Add($"Red: 2♥, 5♥, 7♦, J♦, {playerTrumpCardSymbol} (Trump: {playerTrumpCardSymbol})");
                playerTrumpExamples.Add($"Black: A♠, 3♠, 8♣, K♣, {playerTrumpCardSymbol} (Trump: {playerTrumpCardSymbol})");

                llmTrumpExamples.Add($"Red: 2♥, 5♥, 7♦, J♦, {llmTrumpCardSymbol} (Trump: {llmTrumpCardSymbol})");
                llmTrumpExamples.Add($"Black: A♠, 3♠, 8♣, K♣, {llmTrumpCardSymbol} (Trump: {llmTrumpCardSymbol})");
            }

            CreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}