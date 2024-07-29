using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class FlushRule : BaseBonusRule
    {
        public FlushRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(FlushRule),
                "Cards of the same suit, not necessarily in sequence",
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
            bool isFlush = combination.All(card => card.Suit == combination[0].Suit);

            if (isFlush)
            {
                if (hasTrumpCard)
                {
                    bonusDetails.BaseBonus = BonusValue;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.TrumpCardBonus;
                    bonusDetails.BonusDescriptions.Add($"Flush with Trump: Base({BonusValue}) + Trump({GameMode.TrumpBonusValues.TrumpCardBonus})");
                }
                else
                {
                    bonusDetails.BaseBonus = BonusValue;
                    bonusDetails.AdditionalBonus = 0;
                    bonusDetails.BonusDescriptions.Add($"Natural Flush: {BonusValue}");
                }
                return true;
            }
            else if (hasTrumpCard)
            {
                var nonTrumpCards = combination.Where(c => c != trumpCard).ToList();
                bool sameSuitNonTrump = nonTrumpCards.All(card => card.Suit == nonTrumpCards[0].Suit);

                if (sameSuitNonTrump)
                {
                    bonusDetails.BaseBonus = 0;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.TrumpCardBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump-Assisted Flush: {GameMode.TrumpBonusValues.TrumpCardBonus}");
                    return true;
                }
            }

            return false;
        }
    }
}
