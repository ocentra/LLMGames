using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace OcentraAI.LLMGames.GameModes.Rules
{
    [CreateAssetMenu(fileName = nameof(Flush), menuName = "GameMode/Rules/Flush")]
    public class Flush : BaseBonusRule
    {
        public override int MinNumberOfCard { get; protected set; } = 3;
        public override string RuleName { get; protected set; } = $"{nameof(Flush)}";
        public override int BonusValue { get; protected set; } = 110;
        public override int Priority { get; protected set; } = 88;

        public override bool Evaluate(Hand hand, out BonusDetail bonusDetail)
        {
            bonusDetail = null;
            if (!hand.VerifyHand(GameMode, MinNumberOfCard)) return false;

            Card trumpCard = GameMode.UseTrump ? GetTrumpCard() : null;

            List<Card> nonTrumpCards = hand.SelectHighRankingNonTrumpCards(trumpCard);

            if (IsFlushValid(hand, nonTrumpCards, trumpCard))
            {
                bonusDetail = CalculateBonus(hand, false);
                return true;
            }
          
            if (GameMode.UseTrump && hand.HasTrumpCard(trumpCard))
            {

                Hand nonTrumpHand = hand.Where(c => c != trumpCard);
                if (IsFlushValid(nonTrumpHand, nonTrumpCards, trumpCard))
                {
                    bonusDetail = CalculateBonus(hand, true);
                    return true;
                }
            }

            return false;
        }

        private bool IsFlushValid(Hand hand, List<Card> nonHighRanks, Card trumCard)
        {
            if (!hand.IsFlush())
            {
                return false;
            }

            if (hand.IsSequence())
            {
                return false;
            }

            if (hand.IsNOfAKind(2))
            {
                return false;
            }

            
            if (hand.Contains(nonHighRanks))
            {
                return false;
            }

            if (hand.Contains(trumCard))
            {
                return false;
            }



            return true;
        }


        private BonusDetail CalculateBonus(Hand hand, bool isTrumpAssisted)
        {
            int baseBonus = BonusValue * hand.Sum();
            int additionalBonus = 0;
            List<string> descriptions = new List<string> { $"Flush:" };
            string bonusCalculationDescriptions = $"{BonusValue} * {hand.Sum()}";

            if (isTrumpAssisted)
            {
                additionalBonus += GameMode.TrumpBonusValues.FlushBonus;
                descriptions.Add($"Trump Card Bonus: +{GameMode.TrumpBonusValues.FlushBonus}");

                Card trumpCard = GetTrumpCard();

                if (hand.IsTrumpInMiddle(trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.CardInMiddleBonus;
                    descriptions.Add($"Trump Card in Middle: +{GameMode.TrumpBonusValues.CardInMiddleBonus}");
                }

                if (hand.IsRankAdjacentToTrump(trumpCard))
                {
                    additionalBonus += GameMode.TrumpBonusValues.RankAdjacentBonus;
                    descriptions.Add($"Trump Rank Adjacent: +{GameMode.TrumpBonusValues.RankAdjacentBonus}");
                }
            }

            if (additionalBonus > 0)
            {
                bonusCalculationDescriptions += $" + {additionalBonus}";
            }

            return CreateBonusDetails(RuleName, baseBonus, Priority, descriptions, bonusCalculationDescriptions, additionalBonus);
        }

        public override string[] CreateExampleHand(int handSize, string trumpCardRank = null, bool coloured = true)
        {
            if (handSize < 3)
            {
                Debug.LogError("Hand size must be at least 3 for a Flush.");
                return Array.Empty<string>();
            }

            Rank trumpRank = !string.IsNullOrEmpty(trumpCardRank) ? CardUtility.GetRankFromSymbol(trumpCardRank) : Rank.None;
            Card trumpCard = CardUtility.GetCardFromSymbol(trumpCardRank);
            List <Rank> nonHighRanks = HandUtility.SelectNonHighRanks(handSize, trumpRank);
            List<Card> nonTrumpCards;
            List<string> handSymbols;
            Hand handCards;
            int attemptCount = 0;

            const int maxAttempts = 100; // Limit the number of attempts to avoid infinite loops

            do
            {
                handSymbols = GeneratePotentialHand(handSize, trumpCardRank, coloured, nonHighRanks);
                handCards = HandUtility.ConvertFromSymbols(handSymbols.ToArray());
                nonTrumpCards = handCards.SelectHighRankingNonTrumpCards(trumpCard);
                attemptCount++;
                if (attemptCount >= maxAttempts)
                {
                    Debug.LogWarning("Reached maximum attempts to generate a valid flush hand.");
                    break;
                }
            }
            while (!IsFlushValid(handCards, nonTrumpCards, trumpCard));

            return handSymbols.ToArray();
        }



        private List<string> GeneratePotentialHand(int handSize, string trumpCard, bool coloured, List<Rank> nonHighRanks)
        {
            List<string> hand = new List<string>();
            Suit flushSuit = CardUtility.GetRandomSuit();
            List<Rank> selectedRanks = new List<Rank>(nonHighRanks);
            selectedRanks = AvoidSequencesAndDuplicates(selectedRanks);

            for (int i = 0; i < handSize; i++)
            {
                if (!string.IsNullOrEmpty(trumpCard) && i == handSize - 1)
                {
                    hand.Add(trumpCard);
                }
                else if (i < selectedRanks.Count)
                {
                    hand.Add(CardUtility.GetRankSymbol(flushSuit, selectedRanks[i], coloured));
                }
                else
                {
                    Debug.LogError($"Not enough ranks selected for hand size {handSize}");
                    return hand; // Return partial hand to avoid infinite loop
                }
            }

            return hand;
        }



        private List<Rank> AvoidSequencesAndDuplicates(List<Rank> ranks)
        {
            ranks = ranks.OrderBy(rank => rank).ToList();
            for (int i = 0; i < ranks.Count - 1; i++)
            {
                if (HandUtility.IsRankAdjacent(ranks[i], ranks[i + 1]) || ranks[i] == ranks[i + 1])
                {
                    ranks.RemoveAt(i + 1);
                    i--; // Recheck this position
                }
            }
            return ranks;
        }

        private string CreateExampleString(int cardCount, bool isPlayer, bool useTrump = false)
        {
            string trumpCard = useTrump ? CardUtility.GetRankSymbol(Suit.Hearts, Rank.Six, isPlayer) : null;
            string[] exampleHand = CreateExampleHand(cardCount, trumpCard, isPlayer);
            return string.Join(", ", exampleHand);
        }

        public override bool Initialize(GameMode gameMode)
        {
            Description = "All cards of the same suit, without forming a sequence, pair, or n of a kind, optionally considering Trump Wild Card.";

            List<string> playerExamples = new List<string>();
            List<string> llmExamples = new List<string>();
            List<string> playerTrumpExamples = new List<string>();
            List<string> llmTrumpExamples = new List<string>();

            for (int cardCount = 3; cardCount <= gameMode.NumberOfCards; cardCount++)
            {
                playerExamples.Add(CreateExampleString(cardCount, true, false));
                llmExamples.Add(CreateExampleString(cardCount, false, false));

                if (gameMode.UseTrump)
                {
                    playerTrumpExamples.Add(CreateExampleString(cardCount, true, true));
                    llmTrumpExamples.Add(CreateExampleString(cardCount, false, true));
                }
            }

            return TryCreateExample(RuleName, Description, BonusValue, playerExamples, llmExamples, playerTrumpExamples, llmTrumpExamples, gameMode.UseTrump);
        }
    }
}