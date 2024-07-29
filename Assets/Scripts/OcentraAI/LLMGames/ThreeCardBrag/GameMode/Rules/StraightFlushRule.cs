using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class StraightFlushRule : BaseBonusRule
    {
        public StraightFlushRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(StraightFlushRule),
                "Straight Flush with Trump Wild Card. Example: 9, 10, J of Spades",
                bonusValue,
                priority, gameMode)
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

            bool isStraight = IsSequence(orderedHand.Select(c => c.GetRankValue()).ToList());
            bool isFlush = combination.All(card => card.Suit == combination[0].Suit);

            if (isStraight && isFlush)
            {
                if (hasTrumpCard)
                {
                    bonusDetails.BaseBonus = BonusValue;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.StraightFlushBonus;
                    bonusDetails.BonusDescriptions.Add($"Straight Flush with Trump: Base({BonusValue}) + Trump({GameMode.TrumpBonusValues.StraightFlushBonus})");

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
                    bonusDetails.BaseBonus = BonusValue;
                    bonusDetails.BonusDescriptions.Add($"Natural Straight Flush: {BonusValue}");
                }
                return true;
            }
            else if (hasTrumpCard)
            {
                var nonTrumpCards = combination.Where(c => c != trumpCard).ToList();
                bool canFormStraightFlush = (isFlush && CanFormSequenceWithWild(nonTrumpCards.Select(c => c.GetRankValue()).ToList())) ||
                                            (isStraight && nonTrumpCards.All(c => c.Suit == nonTrumpCards[0].Suit));

                if (canFormStraightFlush)
                {
                    bonusDetails.BaseBonus = 0;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.StraightFlushBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump-Assisted Straight Flush: {GameMode.TrumpBonusValues.StraightFlushBonus}");

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
