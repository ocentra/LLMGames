using OcentraAI.LLMGames.Utilities;
using System;
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

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard))
            {
                return false;
            }

            if (hand.IsRoyalSequence(GameMode))
            {
                bonusDetail = CalculateBonus(hand);
                return true;
            }

            return false;
        }

        private BonusDetail CalculateBonus(Hand hand)
        {
            int baseBonus = BonusValue * hand.Sum();
            List<string> descriptions = new List<string> {"Royal Flush"};
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize is < 3 or > 9)
            {
                Debug.LogError($"Invalid hand size for Royal Flush. Received: {handSize}");
                return Array.Empty<string>();
            }

            string[] hand;
            int attempts = 0;
            const int maxAttempts = 100;
            bool isValidRoyalFlush = false;

            do
            {
                Suit flushSuit = CardUtility.GetRandomSuit();
                List<Rank> royalRanks =
                    HandUtility.GetRoyalSequenceAsRank(GameMode.NumberOfCards).Take(handSize).ToList();

                hand = royalRanks.Select(rank => CardUtility.GetRankSymbol(flushSuit, rank, coloured)).ToArray();

                Hand handToValidate = HandUtility.ConvertFromSymbols(hand);

                // Validate if the generated hand forms a valid Royal Flush
                isValidRoyalFlush = handToValidate.IsRoyalSequence(GameMode);

                attempts++;

                if (attempts >= maxAttempts)
                {
                    Debug.LogError($"Failed to generate a valid Royal Flush hand after {maxAttempts} attempts.");
                    return Array.Empty<string>();
                }
            } while (!isValidRoyalFlush);

            return hand;
        }


        private string CreateExampleString(int cardCount, bool isPlayer)
        {
            string[] example = CreateExampleHand(cardCount, null, isPlayer);
            return string.Join(", ", example);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description =
                $"A sequence of the highest {gameMode.NumberOfCards} cards (starting from Ace) all in the same suit.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                playerExamples.Add(CreateExampleString(cardCount, true));
                llmExamples.Add(CreateExampleString(cardCount, false));
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, false);
        }
    }
}