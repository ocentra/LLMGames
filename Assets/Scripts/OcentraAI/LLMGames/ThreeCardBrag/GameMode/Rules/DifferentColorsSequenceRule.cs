using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class DifferentColorsSequenceRule : BaseBonusRule
    {
        public DifferentColorsSequenceRule(int bonusValue, int priority,GameMode gameMode) :
            base(nameof(DifferentColorsSequenceRule),
                "Sequence of 3 cards of different colors with Trump Wild Card",
                bonusValue,
                priority,gameMode)
        { }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = new BonusDetails { RuleName = RuleName, BaseBonus = BonusValue, Priority = Priority };

            var combinations = GetAllCombinations(hand, 3);
            foreach (var combination in combinations)
            {
                if (EvaluateCombination(combination, out bonusDetails))
                {
                    return true;
                }
            }
            return false;
        }

        private bool EvaluateCombination(List<Card> combination, out BonusDetails bonusDetails)
        {
            bonusDetails = new BonusDetails { RuleName = RuleName, BaseBonus = BonusValue, Priority = Priority };

            var trumpCard = GetTrumpCard();
            bool hasTrumpCard = HasTrumpCard(combination);
            var orderedHand = combination.OrderBy(card => card.GetRankValue()).ToList();

            bool isSequence = IsSequence(orderedHand.Select(c => c.GetRankValue()).ToList());
            bool differentColors = combination.Select(card => card.GetColorString()).Distinct().Count() == 3;

            if (isSequence && differentColors)
            {
                if (hasTrumpCard)
                {
                    bonusDetails.BaseBonus = BonusValue;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.SequenceBonus;
                    bonusDetails.BonusDescriptions.Add($"Different Color Sequence with Trump: Base({BonusValue}) + Trump({GameMode.TrumpBonusValues.SequenceBonus})");

                    // Check for CardInMiddleBonus
                    if (orderedHand.IndexOf(trumpCard) == 1)
                    {
                        bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.CardInMiddleBonus;
                        bonusDetails.BonusDescriptions.Add($"Trump Card in Middle: +{GameMode.TrumpBonusValues.CardInMiddleBonus}");
                    }

                    // Check for RankAdjacentBonus
                    if (IsRankAdjacent(trumpCard.Rank, orderedHand[0].Rank) || IsRankAdjacent(trumpCard.Rank, orderedHand[2].Rank))
                    {
                        bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                        bonusDetails.BonusDescriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                    }
                }
                else
                {
                    bonusDetails.BonusDescriptions.Add($"Natural Different Color Sequence: {BonusValue}");
                }
                return true;
            }
            else if (hasTrumpCard)
            {
                var nonTrumpCards = combination.Where(c => c != trumpCard).ToList();
                bool canFormSequence = CanFormSequenceWithWild(nonTrumpCards.Select(c => c.GetRankValue()).ToList());
                bool differentColorsNonTrump = nonTrumpCards.Select(card => card.GetColorString()).Distinct().Count() == 2;

                if (canFormSequence && differentColorsNonTrump)
                {
                    bonusDetails.BaseBonus = 0;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.SequenceBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump-Assisted Different Color Sequence: {GameMode.TrumpBonusValues.SequenceBonus}");

                    // Check for RankAdjacentBonus in trump-assisted scenario
                    if (IsRankAdjacent(trumpCard.Rank, nonTrumpCards[0].Rank) || IsRankAdjacent(trumpCard.Rank, nonTrumpCards[1].Rank))
                    {
                        bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                        bonusDetails.BonusDescriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
