using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(TrumpOfAKind), menuName = "GameMode/Rules/TrumpOfAKind")]
    public class TrumpOfAKind : BaseBonusRule
    {
        public override string RuleName { get; protected set; } = $"{nameof(TrumpOfAKind)}";

        public override int MinNumberOfCard { get; protected set; } = 3;

        public override int BonusValue { get; protected set; } = 30;
        public override int Priority { get; protected set; } = 85;

        public override bool Evaluate(List<Card> hand, out BonusDetails bonusDetails)
        {
            bonusDetails = null;

            if (GameMode == null || (GameMode != null && GameMode.NumberOfCards > MinNumberOfCard))
                return false;

            if (!GameMode.UseTrump || hand.Count > 4)
                return false;

            var trumpCard = GetTrumpCard();
            var trumpCount = hand.Count(card => card.Equals(trumpCard));

            if (trumpCount > 0)
            {
                bonusDetails = CalculateBonus(trumpCount);
                return true;
            }

            return false;
        }

        private BonusDetails CalculateBonus(int trumpCount)
        {
            int baseBonus = BonusValue * trumpCount;
            var descriptions = new List<string> { $"Trump of a Kind: {trumpCount} Trump Cards" };

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions);
        }


        public override bool Initialize(GameMode gameMode)
        {
            if (!gameMode.UseTrump) return false;

            Description = "Multiple Trump cards in the hand.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                var playerExample = CreateExampleString(cardCount, true);
                var llmExample = CreateExampleString(cardCount, false);

                playerExamples.Add(playerExample);
                llmExamples.Add(llmExample);
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, null, null, gameMode.UseTrump);
        }

        private string CreateExampleString(int cardCount, bool isPlayer)
        {
            string[] cardSymbols = cardCount switch
            {
                3 => new[] { "6♥", "6♥", "6♥" },
                4 => new[] { "6♥", "6♥", "6♥", "6♥" },
                5 => new[] { "6♥", "6♥", "6♥", "6♥", "A♠" },
                6 => new[] { "6♥", "6♥", "6♥", "6♥", "A♠", "K♦" },
                7 => new[] { "6♥", "6♥", "6♥", "6♥", "A♠", "K♦", "Q♣" },
                8 => new[] { "6♥", "6♥", "6♥", "6♥", "A♠", "K♦", "Q♣", "J♥" },
                9 => new[] { "6♥", "6♥", "6♥", "6♥", "A♠", "K♦", "Q♣", "J♥", "10♠" },
                _ => Array.Empty<string>()
            };

            List<string> exampleCards = isPlayer ? ConvertCardSymbols(cardSymbols).ToList() : cardSymbols.ToList();

            return string.Join(" ", exampleCards);
        }
    }
}
