using OcentraAI.LLMGames.Scriptable;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(RoyalFlush), menuName = "Rules/RoyalFlush")]
    public class RoyalFlush : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(RoyalFlush)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            List<int> ranks = hand.Select(card => (int)card.Rank).ToList();
            List<int> sequence = GetSequence(GameMode.NumberOfCards);

            if (hand.All(card => card.Suit == hand[0].Suit) && IsSequence(ranks, sequence))
            {
                bonusDetails = CalculateBonus(hand);
                return true;
            }

            bonusDetails = null;
            return false;
        }

        private BonusDetails CalculateBonus(List<Card> hand)
        {
            int baseBonus = BonusValue;
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Royal Flush" };

            if (GameMode.UseTrump)
            {
                Card trumpCard = GetTrumpCard();
                bool hasTrumpCard = HasTrumpCard(hand);

                if (hasTrumpCard && hand.Any(card => card.Equals(trumpCard)))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RoyalFlushBonus;
                    descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.RoyalFlushBonus}");
                }
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }



        public override void Initialize(GameMode gameMode)
        {

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            string playerTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six);
            string llmTrumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six,false);


            List<int> sequence = GetSequence(gameMode.NumberOfCards);
            Rank fromRank = (Rank)sequence.First();
            Rank toRank = (Rank)sequence.Last();
            Description = $"A sequence of cards from {fromRank} to {toRank} in the same suit";

            string playerExample = string.Join(", ", sequence.Select(rank => Card.GetRankSymbol(Suit.Spades, (Rank)rank)));
            string llmExample = string.Join(", ", sequence.Select(rank => Card.GetRankSymbol(Suit.Spades, (Rank)rank,false)));

            playerExamples.Add(playerExample);
            llmExamples.Add(llmExample);

            if (gameMode.UseTrump)
            {
                IEnumerable<string> trumpSequence = sequence.Take(gameMode.NumberOfCards - 1).Concat(new[] { (int)Rank.Six }).Select(rank => Card.GetRankSymbol(Suit.Hearts, (Rank)rank));
                string trumpPlayerExample = string.Join(", ", trumpSequence);
                playerTrumpExamples.Add($"{trumpPlayerExample} (Trump: {playerTrumpCardSymbol})");

                IEnumerable<string> sequenceLLM = sequence.Take(gameMode.NumberOfCards - 1).Concat(new[] { (int)Rank.Six }).Select(rank => Card.GetRankSymbol(Suit.Hearts, (Rank)rank,false));
                string llmTrumpExample = string.Join(", ", sequenceLLM);
                llmTrumpExamples.Add($"{llmTrumpExample} (Trump: {llmTrumpCardSymbol})");
            }

            CreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}
