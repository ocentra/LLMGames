using OcentraAI.LLMGames.Scriptable;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(StraightFlush), menuName = "GameMode/Rules/StraightFlush")]
    public class StraightFlush : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(StraightFlush)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;
            if (GameMode == null || (GameMode != null && GameMode.NumberOfCards > MinNumberOfCard))
                return false;

            var ranks = hand.Select(card => (int)card.Rank).ToList();
            var suit = hand[0].Suit;
            List<int> royalSequence = GetRoyalSequence();

            // Check for royal flush
            if (IsSequence(ranks, royalSequence))
            {
                return false;
            }

            // Check for natural straight flush
            if (hand.All(card => card.Suit == suit) && IsSequence(ranks))
            {
                bonusDetails = CalculateBonus(hand, false);
                return true;
            }

            // Check for trump-assisted straight flush
            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);
                if (hasTrumpCard)
                {
                    var nonTrumpCards = hand.Where(c => c != trumpCard).ToList();
                    bool canFormStraightFlush = (nonTrumpCards.All(card => card.Suit == suit) && CanFormSequenceWithWild(nonTrumpCards.Select(c => (int)c.Rank).ToList())) ||
                                                (IsSequence(ranks) && nonTrumpCards.All(c => c.Suit == suit));

                    if (canFormStraightFlush)
                    {
                        bonusDetails = CalculateBonus(hand, true);
                        return true;
                    }
                }
            }

            return false;
        }

        private BonusDetails CalculateBonus(List<Card> hand, bool isTrumpAssisted)
        {
            int baseBonus = BonusValue;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Straight Flush:" };

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.StraightFlushBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.StraightFlushBonus}");

                var orderedHand = hand.OrderBy(card => (int)card.Rank).ToList();
                var trumpCard = GetTrumpCard();

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

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "A sequence of cards in rank and all of the same suit, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string playerTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six);
            string llmTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six, false);

            List<int> sequence = GetSequence(gameMode.NumberOfCards);
            Rank fromRank = (Rank)sequence.First();
            Rank toRank = (Rank)sequence.Last();
            Description = $"A sequence of cards from {fromRank} to {toRank} in the same suit";

            string playerExample = string.Join(", ", sequence.Select(rank => Card.GetRankSymbol(Suit.Spades, (Rank)rank)));
            string llmExample = string.Join(", ", sequence.Select(rank => Card.GetRankSymbol(Suit.Spades, (Rank)rank, false)));

            playerExamples.Add(playerExample);
            llmExamples.Add(llmExample);

            if (gameMode.UseTrump)
            {
                IEnumerable<string> trumpSequence = sequence.Take(gameMode.NumberOfCards - 1).Concat(new[] { (int)Rank.Six }).Select(rank => Card.GetRankSymbol(Suit.Hearts, (Rank)rank));
                string trumpPlayerExample = string.Join(", ", trumpSequence);
                playerTrumpExamples.Add($"{trumpPlayerExample} (Trump: {playerTrumpCardSymbol})");

                IEnumerable<string> sequenceLLM = sequence.Take(gameMode.NumberOfCards - 1).Concat(new[] { (int)Rank.Six }).Select(rank => Card.GetRankSymbol(Suit.Hearts, (Rank)rank, false));
                string llmTrumpExample = string.Join(", ", sequenceLLM);
                llmTrumpExamples.Add($"{llmTrumpExample} (Trump: {llmTrumpCardSymbol})");
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}
