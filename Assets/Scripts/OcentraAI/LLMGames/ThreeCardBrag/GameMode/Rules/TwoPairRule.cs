using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class TwoPairRule : BaseBonusRule
    {
        public TwoPairRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(TwoPairRule),
                "Two Pair. Example: AA, KK",
                bonusValue,
                priority, gameMode)
        { }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = new BonusDetails { RuleName = RuleName, BaseBonus = BonusValue, Priority = Priority };

            var rankCounts = GetRankCounts(hand);
            int pairs = rankCounts.Values.Count(v => v == 2);

            if (pairs >= 2)
            {
                bonusDetails.BaseBonus = BonusValue;
                bonusDetails.BonusDescriptions.Add($"Two Pair: {BonusValue}");
                return true;
            }
            return false;
        }
    }
    

}