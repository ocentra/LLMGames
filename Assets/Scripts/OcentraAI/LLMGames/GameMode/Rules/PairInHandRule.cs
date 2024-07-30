using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class PairInHandRule : BaseBonusRule
    {
        public PairInHandRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(PairInHandRule),
                "Pair in hand with bonuses for trump card and same color",
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
            var rankCounts = GetRankCounts(combination);

            if (rankCounts.ContainsValue(2))
            {
                var pairRank = rankCounts.First(kv => kv.Value == 2).Key;
                var pairCards = combination.Where(c => c.Rank == pairRank).ToList();

                bonusDetails.BaseBonus = BonusValue;
                bonusDetails.BonusDescriptions.Add($"Pair of {pairRank}");

                if (pairCards.All(c => c.GetColorString() == pairCards[0].GetColorString()))
                {
                    bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.SameColorBonus;
                    bonusDetails.BonusDescriptions.Add($"Same Color Pair: +{GameMode.TrumpBonusValues.SameColorBonus}");
                }

                if (hasTrumpCard)
                {
                    if (pairRank == trumpCard.Rank)
                    {
                        bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.TrumpCardBonus;
                        bonusDetails.BonusDescriptions.Add($"Trump Card Pair: +{GameMode.TrumpBonusValues.TrumpCardBonus}");
                    }
                    else
                    {
                        // Check for RankAdjacentBonus when trump is the third card
                        var thirdCard = combination.First(c => c.Rank != pairRank);
                        if (IsRankAdjacent(trumpCard.Rank, pairRank))
                        {
                            bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                            bonusDetails.BonusDescriptions.Add($"Trump Rank Adjacent to Pair: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                        }
                        else if (IsRankAdjacent(trumpCard.Rank, thirdCard.Rank))
                        {
                            bonusDetails.AdditionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                            bonusDetails.BonusDescriptions.Add($"Trump Rank Adjacent to Third Card: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                        }
                    }
                }

                return true;
            }
            else if (hasTrumpCard)
            {
                // Trump-assisted pair
                var highCard = combination.Where(c => c != trumpCard).OrderByDescending(c => c.GetRankValue()).First();
                bonusDetails.BaseBonus = 0;
                bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.TrumpCardBonus;
                bonusDetails.BonusDescriptions.Add($"Trump-Assisted Pair with {highCard.Rank}: {GameMode.TrumpBonusValues.TrumpCardBonus}");

                if (IsRankAdjacent(trumpCard.Rank, highCard.Rank))
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
