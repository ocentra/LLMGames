using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FiveOfAKind), menuName = "GameMode/Rules/FiveOfAKind")]
    public class FiveOfAKind : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 5;

        public override string RuleName { get; protected set; } = $"{nameof(FiveOfAKind)}";
        public override int BonusValue { get; protected set; } = 140;
        public override int Priority { get; protected set; } = 94;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!VerifyNumberOfCards(hand)) return false;

            if (!GameMode.UseTrump) return false;

            Card trumpCard = GetTrumpCard();

            if (hand.Count() >= 5)
            {
                Rank? fourOfAKindRank = FindNOfAKind(hand, 4);

                if (fourOfAKindRank.HasValue && !IsRankTrumpOrAce(fourOfAKindRank.Value, trumpCard) && hand.HasTrumpCard(trumpCard))
                {
                    bonusDetail = CalculateBonus(hand, fourOfAKindRank.Value);
                    return true;
                }
            }

            return false;
        }

        private bool IsRankTrumpOrAce(Rank rank, Card trumpCard)
        {
            return rank == trumpCard.Rank || rank == Rank.A;
        }

        private BonusDetail CalculateBonus(Hand hand, Rank rank)
        {
            int baseBonus = BonusValue * ((int)rank * 5);
            string bonusCalculationDescriptions = $"{BonusValue} * ( {(int)rank} * 5)";

            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Five of a Kind:" };

            if (HasTrumpCard(hand))
            {
                additionalBonus += GameMode.TrumpBonusValues.FiveOfKindBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FiveOfKindBonus}");
            }
            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions = $"{BonusValue} * ( {(int)rank} * 5) + {additionalBonus} ";
            }
            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override bool Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump || gameMode.NumberOfCards < 5) return false;

            Description = "Four cards of the same rank plus a trump card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 5; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                string playerExample = CreateExampleString(cardCount, true);
                string llmExample = CreateExampleString(cardCount, false);

                playerExamples.Add(playerExample);
                llmExamples.Add(llmExample);
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCard = null, bool coloured = true)
        {
            if (handSize < 5 || string.IsNullOrEmpty(trumpCard))
            {
                Debug.LogError("Hand size must be at least 5 and trump card must be specified for Five of a Kind.");
                return Array.Empty<string>();
            }

            List<string> hand = new List<string>();

            Rank fiveOfAKindRank = (Rank)UnityEngine.Random.Range(2, 15);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                hand.Add($"{CardUtility.GetRankSymbol(suit, fiveOfAKindRank, coloured)}");
            }

            hand.Add(trumpCard);

            for (int i = 5; i < handSize; i++)
            {
                Rank randomRank;
                do
                {
                    randomRank = (Rank)UnityEngine.Random.Range(2, 15);
                } while (randomRank == fiveOfAKindRank);

                Suit randomSuit = (Suit)UnityEngine.Random.Range(0, 4);
                hand.Add($"{CardUtility.GetRankSymbol(randomSuit, randomRank, coloured)}");
            }

            return hand.ToArray();
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            List<string[]> examples = new List<string[]>();
            string trumpCard = "6♥"; // Trump card is always used in this rule

            examples.Add(CreateExampleHand(cardCount, trumpCard, isPlayer));

            IEnumerable<string> exampleStrings = examples.Select(example =>
                string.Join(", ", example)
            );

            return string.Join(Environment.NewLine, exampleStrings);
        }
    }
}
