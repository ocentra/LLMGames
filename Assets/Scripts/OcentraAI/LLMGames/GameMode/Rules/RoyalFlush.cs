using OcentraAI.LLMGames.Scriptable;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(RoyalFlush), menuName = "GameMode/Rules/RoyalFlush")]
    public class RoyalFlush : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(RoyalFlush)}";
        public override int BonusValue { get; protected set; } = 200;
        public override int Priority { get; protected set; } = 100;

        public override bool Evaluate(List<Card> hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!VerifyNumberOfCards(hand)) return false;
            
            if (IsRoyalSequence(hand))
            {
                bonusDetail = CalculateBonus(hand);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(List<Card> hand)
        {
            int baseBonus = BonusValue * CalculateHandValue(hand);
            List<string> descriptions = new List<string> { $"Royal Flush" };
            string bonusCalculationDescriptions = $"{BonusValue} * {CalculateHandValue(hand)}";
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions);
        }

        public override bool Initialize(GameMode gameMode)
        {
            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            List<int> sequence = GetRoyalSequence();
            Rank fromRank = (Rank)sequence.First();
            Rank toRank = (Rank)sequence.Last();
            Description = $"A sequence of cards from {fromRank} to {toRank} in the same suit";

            string playerExample = string.Join(", ", sequence.Select(rank => Card.GetRankSymbol(Suit.Spades, (Rank)rank)));
            string llmExample = string.Join(", ", sequence.Select(rank => Card.GetRankSymbol(Suit.Spades, (Rank)rank, false)));

            playerExamples.Add(playerExample);
            llmExamples.Add(llmExample);

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, new List<string>(), new List<string>(), gameMode.UseTrump);
        }
    }
}
