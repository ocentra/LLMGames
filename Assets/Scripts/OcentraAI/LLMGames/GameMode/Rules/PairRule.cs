using System;
using System.Collections.Generic;
using System.Linq;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [Serializable]
    public class PairRule : BaseBonusRule
    {
        public PairRule(int bonusValue, int priority, GameMode gameMode) :
            base(nameof(PairRule),
                "Pair of cards with the same rank, optionally considering Trump Wild Card.",
                bonusValue,
                priority, gameMode)
        {
            InitializeExamples();
        }

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            var rankCounts = GetRankCounts(hand);
            var pair = FindHighestPair(rankCounts);

            if (pair.HasValue)
            {
                bonusDetails = CalculateBonus(hand, pair.Value);
                return true;
            }

            bonusDetails = null;
            return false;
        }

        private Rank? FindHighestPair(Dictionary<Rank, int> rankCounts)
        {
            return rankCounts.Where(kv => kv.Value >= 2)
                             .OrderByDescending(kv => kv.Key)
                             .Select(kv => (Rank?)kv.Key)
                             .FirstOrDefault();
        }

        private BonusDetails CalculateBonus(List<Card> hand, Rank pairRank)
        {
            int baseBonus = BonusValue;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Pair of {Card.GetRankSymbol(Suit.Spades, pairRank)}" };

            if (GameMode.UseTrump)
            {
                var trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && pairRank == trumpCard.Rank)
                {
                    additionalBonus += GameMode.TrumpBonusValues.PairBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.PairBonus}");
                }

                if (hasTrumpCard && IsRankAdjacent(trumpCard.Rank, pairRank))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent Bonus: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            return new BonusDetails
            {
                RuleName = RuleName,
                BaseBonus = baseBonus,
                AdditionalBonus = additionalBonus,
                BonusDescriptions = descriptions,
                Priority = Priority
            };
        }

        public override void InitializeExamples()
        {
            Examples = new GameRulesContainer
            {
                Player = $@"Pair Rule Examples:

Valid in all games:
- {Card.GetRankSymbol(Suit.Spades, Rank.A)}, {Card.GetRankSymbol(Suit.Hearts, Rank.A)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Seven)} (Pair of Aces)
- {Card.GetRankSymbol(Suit.Diamonds, Rank.Five)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Five)}, {Card.GetRankSymbol(Suit.Spades, Rank.Q)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Three)} (Pair of Fives)
- {Card.GetRankSymbol(Suit.Spades, Rank.J)}, {Card.GetRankSymbol(Suit.Hearts, Rank.J)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.Ten)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Two)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Seven)} (Pair of Jacks)

Additional examples for games with Trump Card (e.g., {Card.GetRankSymbol(Suit.Hearts, Rank.Six)} as Trump):
- {Card.GetRankSymbol(Suit.Spades, Rank.Six)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Six)}, {Card.GetRankSymbol(Suit.Clubs, Rank.Nine)} (Pair of Sixes using Trump, extra bonus)
- {Card.GetRankSymbol(Suit.Spades, Rank.Seven)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.Seven)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Six)} (Pair of Sevens with adjacent Trump, additional bonus)

Not Valid:
- {Card.GetRankSymbol(Suit.Spades, Rank.K)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Q)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.J)} (No pair)
- {Card.GetRankSymbol(Suit.Clubs, Rank.Three)}, {Card.GetRankSymbol(Suit.Hearts, Rank.Four)}, {Card.GetRankSymbol(Suit.Diamonds, Rank.Five)}, {Card.GetRankSymbol(Suit.Spades, Rank.Six)} (No pair)

Scoring:
- Base bonus for a pair
- In games with Trump:
  - Additional bonus if the pair includes the Trump card
  - Adjacent Trump bonus if the Trump card is adjacent in rank to the pair

Note: In hands with multiple pairs, only the highest pair is considered for this rule.",

                LLM = @"PairRule: Evaluate hands for the highest pair. Valid for any hand size. Base scoring for pair. In games with Trump: consider Trump card to form pairs or provide adjacent rank bonus. Scoring may include Trump card bonus if used in pair, and Trump adjacent bonus. Example without Trump: A?, A?, 7? (valid pair of Aces). Example with 6? as Trump: 7?, 7?, 6? (pair of Sevens with adjacent Trump bonus)."
            };
        }
    }
}