using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(SameColorsSequence), menuName = "GameMode/Rules/SameColorsSequence")]
    public class SameColorsSequence : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(SameColorsSequence)}";
        public override int BonusValue { get; protected set; } = 120;
        public override int Priority { get; protected set; } = 90;

        public override bool Evaluate(List<Card> hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;

            if (!VerifyNumberOfCards(hand)) return false;

            if (!IsSequence(hand)) return false;


            // Check for royal flush
            if (IsRoyalSequence(hand))
            {
                return false;
            }

            if (IsSameColorAndDifferentSuits(hand))
            {
                bonusDetail = CalculateBonus(hand, false);
                return true;
            }

            // Check for trump-assisted same color sequence
            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);
                if (hasTrumpCard)
                {
                    List<Card> nonTrumpCards = hand.Where(c => c != trumpCard).ToList();
                    bool canFormSequence = CanFormSequenceWithWild(nonTrumpCards.Select(c => c.GetRankValue()).ToList());
                    bool sameColorNonTrump = nonTrumpCards.All(card => Card.GetColorValue(card.Suit) == Card.GetColorValue(nonTrumpCards[0].Suit));

                    if (canFormSequence && sameColorNonTrump)
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
            List<string> descriptions = new List<string> { $"Same Colors Sequence:" };
            string bonusCalculationDescriptions = $"{BonusValue} * {CalculateHandValue(hand)}";

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.SequenceBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.SequenceBonus}");

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

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string playerTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six);
            string llmTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six, false);

            List<int> sequence = GetSequence(gameMode.NumberOfCards);
            Rank fromRank = (Rank)sequence.First();
            Rank toRank = (Rank)sequence.Last();
            Description = $"A sequence of {gameMode.NumberOfCards} cards from {fromRank} to {toRank}, all of the same color.";

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
