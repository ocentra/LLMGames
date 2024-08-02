using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(DifferentColorsSequence), menuName = "Rules/DifferentColorsSequence")]
    public class DifferentColorsSequence : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(DifferentColorsSequence)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            for (int combinationSize = 3; combinationSize <= hand.Count; combinationSize++)
            {
                var combinations = GetAllCombinations(hand, combinationSize);
                foreach (var combination in combinations)
                {
                    if (EvaluateCombination(combination, out bonusDetails))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool EvaluateCombination(List<Card> combination, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            var orderedHand = combination.OrderBy(card => card.GetRankValue()).ToList();
            var ranks = orderedHand.Select(card => card.GetRankValue()).ToList();
            bool isSequence = IsSequence(ranks);
            bool differentColors = combination.Select(card => Card.GetColorValue(card.Suit)).Distinct().Count() == combination.Count;

            if (isSequence && differentColors)
            {
                bonusDetails = CalculateBonus(combination, false);
                return true;
            }

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(combination);

                if (hasTrumpCard)
                {
                    var nonTrumpCards = combination.Where(c => c != trumpCard).ToList();
                    bool canFormSequence = CanFormSequenceWithWild(nonTrumpCards.Select(c => c.GetRankValue()).ToList());
                    bool differentColorsNonTrump = nonTrumpCards.Select(card => Card.GetColorValue(card.Suit)).Distinct().Count() == nonTrumpCards.Count;

                    if (canFormSequence && differentColorsNonTrump)
                    {
                        bonusDetails = CalculateBonus(combination, true);
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
            var descriptions = new List<string> { $"Different Colors Sequence:" };

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.SequenceBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.SequenceBonus}");

                var orderedHand = hand.OrderBy(card => card.GetRankValue()).ToList();
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

        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        public override void Initialize(GameMode gameMode)
        {
            RuleName = "Different Colors Sequence Rule";
            Description = "A sequence of 3 to 9 cards of different colors, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string playerTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six);
            string llmTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six, false);

            List<int> sequence = GetSequence(gameMode.NumberOfCards);
            Rank fromRank = (Rank)sequence.First();
            Rank toRank = (Rank)sequence.Last();
            Description = $"A sequence of cards from {fromRank} to {toRank}, all of different colors.";

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

            CreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}
