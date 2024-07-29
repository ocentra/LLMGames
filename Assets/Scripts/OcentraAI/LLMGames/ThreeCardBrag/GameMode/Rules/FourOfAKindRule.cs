using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class FourOfAKindRule : BaseBonusRule
    {
        public FourOfAKindRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(FourOfAKindRule),
                "Four of a Kind. Example: AAAA",
                bonusValue,
                priority, gameMode)
        { }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = new BonusDetails { RuleName = RuleName, BaseBonus = BonusValue, Priority = Priority };

            var rankCounts = GetRankCounts(hand);

            if (rankCounts.ContainsValue(4))
            {
                bonusDetails.BaseBonus = BonusValue;
                bonusDetails.BonusDescriptions.Add($"Four of a Kind: {BonusValue}");
                return true;
            }
            return false;
        }
    }
}