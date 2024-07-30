using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class FullHouseRule : BaseBonusRule
    {
        public FullHouseRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(FullHouseRule),
                "Full House. Example: AAA, KK",
                bonusValue,
                priority, gameMode)
        { }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = new BonusDetails { RuleName = RuleName, BaseBonus = BonusValue, Priority = Priority };

            var rankCounts = GetRankCounts(hand);

            if (rankCounts.ContainsValue(3) && rankCounts.ContainsValue(2))
            {
                bonusDetails.BaseBonus = BonusValue;
                bonusDetails.BonusDescriptions.Add($"Full House: {BonusValue}");
                return true;
            }
            return false;
        }
    }
}