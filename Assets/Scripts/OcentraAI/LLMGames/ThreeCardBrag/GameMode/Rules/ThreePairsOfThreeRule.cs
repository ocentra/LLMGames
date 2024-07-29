using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class ThreePairsOfThreeRule : BaseBonusRule
    {
        public ThreePairsOfThreeRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(ThreePairsOfThreeRule),
                "Three pairs of three. Example: AAA, JJJ, KKK",
                bonusValue,
                priority, gameMode)
        { }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = new BonusDetails { RuleName = RuleName, BaseBonus = BonusValue, Priority = Priority };

            var rankCounts = GetRankCounts(hand);
            int pairsOfThree = rankCounts.Values.Count(v => v == 3);

            if (pairsOfThree == 3)
            {
                bonusDetails.BaseBonus = BonusValue;
                bonusDetails.BonusDescriptions.Add($"Three Pairs of Three: {BonusValue}");
                return true;
            }
            return false;
        }
    }
}