using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class ThreeOfAKindRule : BaseBonusRule
    {
        public ThreeOfAKindRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(ThreeOfAKindRule),
                "Three of a Kind with Trump Wild Card. Example: AAA or KKK",
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
            var rankCounts = GetRankCounts(combination);
            bool hasTrumpCard = HasTrumpCard(combination);

            if (rankCounts.ContainsValue(3))
            {
                // Natural three of a kind
                var rank = rankCounts.First(kv => kv.Value == 3).Key;
                if (rank == trumpCard.Rank)
                {
                    // Three of a kind of trump cards
                    bonusDetails.BaseBonus = BonusValue;
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.ThreeOfKindBonus;
                    bonusDetails.BonusDescriptions.Add($"Three of a Kind of Trump Cards: Base({BonusValue}) + Trump({GameMode.TrumpBonusValues.ThreeOfKindBonus})");
                }
                else
                {
                    // Natural three of a kind (non-trump)
                    bonusDetails.BonusDescriptions.Add($"Natural Three of a Kind: {BonusValue}");
                }
                return true;
            }
            else if (hasTrumpCard && rankCounts.ContainsValue(2))
            {
                // Three of a kind with trump assistance
                var pairRank = rankCounts.First(kv => kv.Value == 2).Key;
                bonusDetails.BaseBonus = 0;
                bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.ThreeOfKindBonus;
                bonusDetails.BonusDescriptions.Add($"Trump-Assisted Three of a Kind: {GameMode.TrumpBonusValues.ThreeOfKindBonus}");

                // Check for RankAdjacentBonus
                if (IsRankAdjacent(trumpCard.Rank, pairRank))
                {
                    bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }

                return true;
            }

            return false;
        }
    }
}
