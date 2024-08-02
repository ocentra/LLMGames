using OcentraAI.LLMGames.Scriptable;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(FiveOfAKind), menuName = "Rules/FiveOfAKind")]
    public class FiveOfAKind : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(FiveOfAKind)}";
        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 80;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            if (!GameMode.UseTrump) return false;

            var rankCounts = GetRankCounts(hand);
            var trumpCard = GetTrumpCard();

            if (hand.Count >= 5)
            {
                var fourOfAKindRank = FindNOfAKind(rankCounts, 4);

                if (fourOfAKindRank.HasValue && !IsRankTrump(fourOfAKindRank.Value, trumpCard) && HasTrumpCard(hand))
                {
                    bonusDetails = CalculateBonus(hand);
                    return true;
                }
            }

            return false;
        }

        private bool IsRankTrump(Rank rank, Card trumpCard) => rank == trumpCard.Rank;

        private BonusDetails CalculateBonus(List<Card> hand)
        {
            int baseBonus = BonusValue ;
            int additionalBonus = 0;
            var descriptions = new List<string> { $"Five of a Kind:" };

            if (HasTrumpCard(hand))
            {
                additionalBonus += GameMode.TrumpBonusValues.FiveOfKindBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FiveOfKindBonus}");
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, additionalBonus);
        }

        public override void Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump) return;

            RuleName = "Five of a Kind Rule";
            Description = "Four cards of the same rank plus a trump card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 5; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                var playerExample = CreateExampleString(cardCount, true);
                var llmExample = CreateExampleString(cardCount, false);

                playerExamples.Add(playerExample);
                llmExamples.Add(llmExample);
            }

            CreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer)
        {
            List<string> exampleCards = new List<string>();
            string trumpCardSymbol = Card.GetRankSymbol(Suit.Hearts, Rank.Six, isPlayer);

            switch (cardCount)
            {
                case 5:
                    exampleCards.AddRange(GetExampleCards(isPlayer, 5, 4, trumpCardSymbol));
                    break;
                case 6:
                    exampleCards.AddRange(GetExampleCards(isPlayer, 6, 4, trumpCardSymbol));
                    break;
                case 7:
                    exampleCards.AddRange(GetExampleCards(isPlayer, 7, 4, trumpCardSymbol));
                    break;
                case 8:
                    exampleCards.AddRange(GetExampleCards(isPlayer, 8, 4, trumpCardSymbol));
                    break;
                case 9:
                    exampleCards.AddRange(GetExampleCards(isPlayer, 9, 4, trumpCardSymbol));
                    break;
                default:
                    break;
            }

            return string.Join(" ", exampleCards);
        }

        private List<string> GetExampleCards(bool isPlayer, int cardCount, int fourOfAKindCount, string trumpCardSymbol)
        {
            List<string> exampleCards = new List<string>();
            string fourOfAKindCardSymbol = isPlayer ? Card.GetRankSymbol(Suit.Spades, Rank.Five) : "5♠";

            for (int i = 0; i < fourOfAKindCount; i++)
            {
                exampleCards.Add(fourOfAKindCardSymbol);
            }

            exampleCards.Add(trumpCardSymbol);

            for (int i = fourOfAKindCount + 1; i < cardCount; i++)
            {
                string fillerCardSymbol = isPlayer ? Card.GetRankSymbol(Suit.Hearts, Rank.Q) : "Q♥";
                exampleCards.Add(fillerCardSymbol);
            }

            return exampleCards;
        }
    }
}
