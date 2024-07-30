using System;
using System.Collections.Generic;
using System.Linq;
using OcentraAI.LLMGames.Scriptable;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class NOfAKindRule : BaseBonusRule
    {
        public NOfAKindRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(NOfAKindRule),
                "N of a Kind with Trump Wild Card. Covers Pairs, Three of a Kind, Four of a Kind, and Five of a Kind.",
                bonusValue,
                priority, gameMode)
        { }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            var trumpCard = GetTrumpCard();
            bool hasTrumpCard = HasTrumpCard(hand);
            var rankCounts = GetRankCounts(hand);

            List<BonusDetails> allPossibleBonuses = new List<BonusDetails>
            {
                // Evaluate all possible combinations
                EvaluateThreeOfAKind(hand, hasTrumpCard, trumpCard, rankCounts),
                EvaluateFourOfAKind(hand, hasTrumpCard, trumpCard, rankCounts),
                EvaluateFiveOfAKind(hand, hasTrumpCard, trumpCard, rankCounts),
                EvaluateFullHouse(hand, hasTrumpCard, trumpCard, rankCounts),
              
                EvaluateTwoPair(hand, hasTrumpCard, trumpCard, rankCounts),
                EvaluatePair(hand, hasTrumpCard, trumpCard, rankCounts)
            };

            // Remove null entries (combinations that weren't possible)
            allPossibleBonuses.RemoveAll(b => b == null);

            if (allPossibleBonuses.Count == 0)
            {
                bonusDetails = null;
                return false;
            }

            // Choose the combination with the highest total bonus
            bonusDetails = allPossibleBonuses.OrderByDescending(b => b.TotalBonus).First();
            return true;
        }

        private BonusDetails EvaluateFiveOfAKind(List<Card> hand, bool hasTrumpCard, Card trumpCard, Dictionary<Rank, int> rankCounts)
        {
            if (rankCounts.ContainsValue(5) || (rankCounts.ContainsValue(4) && hasTrumpCard))
            {
                var rank = rankCounts.FirstOrDefault(x => x.Value == (hasTrumpCard ? 4 : 5)).Key;
                var bonusDetails = new BonusDetails
                {
                    RuleName = "Five of a Kind",
                    BaseBonus = BonusValue * 5,
                    Priority = Priority + 5,
                    BonusDescriptions = new List<string> { $"Five of a Kind: {rank}" }
                };

                if (hasTrumpCard)
                {
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.FiveOfKindBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FiveOfKindBonus}");
                }

                return bonusDetails;
            }
            return null;
        }

        private BonusDetails EvaluateFourOfAKind(List<Card> hand, bool hasTrumpCard, Card trumpCard, Dictionary<Rank, int> rankCounts)
        {
            if (rankCounts.ContainsValue(4) || (rankCounts.ContainsValue(3) && hasTrumpCard))
            {
                var rank = rankCounts.FirstOrDefault(x => x.Value == (hasTrumpCard ? 3 : 4)).Key;
                var bonusDetails = new BonusDetails
                {
                    RuleName = "Four of a Kind",
                    BaseBonus = BonusValue * 4,
                    Priority = Priority + 4,
                    BonusDescriptions = new List<string> { $"Four of a Kind: {rank}" }
                };

                if (hasTrumpCard && rankCounts.ContainsValue(3))
                {
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.FourOfKindBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FourOfKindBonus}");
                }

                return bonusDetails;
            }
            return null;
        }

        private BonusDetails EvaluateFullHouse(List<Card> hand, bool hasTrumpCard, Card trumpCard, Dictionary<Rank, int> rankCounts)
        {
            if ((rankCounts.ContainsValue(3) && rankCounts.ContainsValue(2)) ||
                (rankCounts.Values.Count(v => v == 2) == 2 && hasTrumpCard))
            {
                var threeOfAKindRank = rankCounts.FirstOrDefault(x => x.Value == 3).Key;
                var pairRank = rankCounts.FirstOrDefault(x => x.Value == 2 && x.Key != threeOfAKindRank).Key;

                var bonusDetails = new BonusDetails
                {
                    RuleName = "Full House",
                    BaseBonus = BonusValue * 3 + BonusValue * 2,
                    Priority = Priority + 3,
                    BonusDescriptions = new List<string> { $"Full House: {threeOfAKindRank} over {pairRank}" }
                };

                if (hasTrumpCard && rankCounts.Values.Count(v => v == 2) == 2)
                {
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.ThreeOfKindBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Card Bonus: +{bonusDetails.AdditionalBonus}");
                }

                return bonusDetails;
            }
            return null;
        }

        private BonusDetails EvaluateThreeOfAKind(List<Card> hand, bool hasTrumpCard, Card trumpCard, Dictionary<Rank, int> rankCounts)
        {
            if (rankCounts.ContainsValue(3) || (rankCounts.ContainsValue(2) && hasTrumpCard))
            {
                var rank = rankCounts.FirstOrDefault(x => x.Value == (hasTrumpCard ? 2 : 3)).Key;
                var bonusDetails = new BonusDetails
                {
                    RuleName = "Three of a Kind",
                    BaseBonus = BonusValue * 3,
                    Priority = Priority + 2,
                    BonusDescriptions = new List<string> { $"Three of a Kind: {rank}" }
                };

                if (hasTrumpCard && rankCounts.ContainsValue(2))
                {
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.ThreeOfKindBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.ThreeOfKindBonus}");
                }

                return bonusDetails;
            }
            return null;
        }

        private BonusDetails EvaluateTwoPair(List<Card> hand, bool hasTrumpCard, Card trumpCard, Dictionary<Rank, int> rankCounts)
        {
            var pairs = rankCounts.Where(x => x.Value == 2).ToList();
            if (pairs.Count == 2 || (pairs.Count == 1 && hasTrumpCard))
            {
                var firstPairRank = pairs[0].Key;
                var secondPairRank = pairs.Count == 2 ? pairs[1].Key : trumpCard.Rank;
                var bonusDetails = new BonusDetails
                {
                    RuleName = "Two Pair",
                    BaseBonus = BonusValue * 2 * 2,
                    Priority = Priority + 1,
                    BonusDescriptions = new List<string> { $"Two Pair: {firstPairRank} and {secondPairRank}" }
                };

                if (hasTrumpCard && pairs.Count == 1)
                {
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.PairBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Card Bonus: +{bonusDetails.AdditionalBonus}");
                }

                return bonusDetails;
            }
            return null;
        }

        private BonusDetails EvaluatePair(List<Card> hand, bool hasTrumpCard, Card trumpCard, Dictionary<Rank, int> rankCounts)
        {
            if (rankCounts.ContainsValue(2) || hasTrumpCard)
            {
                var rank = rankCounts.ContainsValue(2) ? rankCounts.First(x => x.Value == 2).Key : hand.Where(c => c != trumpCard).Max(c => c.Rank);
                var bonusDetails = new BonusDetails
                {
                    RuleName = "Pair",
                    BaseBonus = BonusValue * 2,
                    Priority = Priority,
                    BonusDescriptions = new List<string> { $"Pair of {rank}" }
                };

                if (hasTrumpCard && !rankCounts.ContainsValue(2))
                {
                    bonusDetails.AdditionalBonus = GameMode.TrumpBonusValues.PairBonus;
                    bonusDetails.BonusDescriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                }

                return bonusDetails;
            }
            return null;
        }
    }
}