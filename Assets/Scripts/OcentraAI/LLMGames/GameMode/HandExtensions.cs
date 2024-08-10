using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OcentraAI.LLMGames.Scriptable;
using static OcentraAI.LLMGames.Utilities.CardUtility;
using UnityEngine.XR;

namespace OcentraAI.LLMGames.GameModes
{
    public static class HandExtensions
    {
        public static bool IsTrumpInMiddle(this Hand hand, Card trumpCard)
        {
            List<Card> orderedHand = hand.Cards.OrderBy(card => card.GetRankValue()).ToList();
            
            int handSize = hand.Count();
            if (handSize % 2 == 1)
            {
                // Odd number of cards, one middle position
                int middleIndex = handSize / 2;
                return orderedHand[middleIndex].Equals(trumpCard);
            }
            else
            {
                // Even number of cards, two middle positions
                int firstMiddleIndex = (handSize / 2) - 1;
                int secondMiddleIndex = handSize / 2;
                return orderedHand[firstMiddleIndex].Equals(trumpCard) || orderedHand[secondMiddleIndex].Equals(trumpCard);
            }
        }

        public static bool IsRankAdjacentToTrump(this Hand hand, Card trumpCard)
        {
            List<Card> orderedHand = hand.Cards.OrderBy(card => card.GetRankValue()).ToList();

            foreach (Card card in orderedHand)
            {
                if (hand.IsRankAdjacent(card.Rank, trumpCard.Rank))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Add(this Hand hand,Card drawCard)
        {
            List<Card> cards = hand.Cards.ToList();
            cards.Add(drawCard);
            hand.SetCards(cards.ToArray());
        }

        public static string GetFormattedHand(this Hand hand)
        {
            return string.Join(" ", hand.Cards.Select(card => card.RankSymbol));
        }

        public static Card FindHighestCard(this Hand hand)
        {
            Card highestCard = null;
            foreach (Card card in hand.Cards)
            {
                if (card.Rank >= Rank.J && (highestCard == null || card.Rank > highestCard.Rank))
                {
                    highestCard = card;
                }
            }
            return highestCard;
        }
        public static int Sum(this Hand hand)
        {
            return hand.Cards.Sum(c => c.GetRankValue());
        }
        public static bool Any(this Hand hand, string id)
        {
            return hand.Cards.Any(c=> c.Id == id );
        }

        public static int Max(this Hand hand)
        {
            return hand.Cards.Max(c => c.GetRankValue());
        }

        public static int Min(this Hand hand)
        {
            return hand.Cards.Min(c => c.GetRankValue());
        }

        public static int Count(this Hand hand)
        {
            return hand.Cards?.Length ?? 0;
        }

        public static void Ascending(this Hand hand)
        {
            if (hand?.Cards != null)
            {
                Array.Sort(hand.Cards, (card1, card2) => card1.GetRankValue().CompareTo(card2.GetRankValue()));
            }

        }

        public static void Descending(this Hand hand)
        {
            if (hand?.Cards != null)
            {
                Array.Sort(hand.Cards, (card1, card2) => card2.GetRankValue().CompareTo(card1.GetRankValue()));
            }

        }

        public static bool Contains(this Hand hand, Card card)
        {
            return card != null && hand.Cards.Any(c => c.Equals(card));
        }

        public static bool IsFlush(this Hand hand)
        {
            return hand.Cards.Length > 0 && hand.Cards.All(card => card.Suit == hand.Cards[0].Suit);
        }

        public static bool IsSequence(this Hand hand)
        {
            if (hand.Cards.Length < 2) return false;

            List<int> ranks = hand.Cards.Select(card => card.GetRankValue()).OrderBy(rank => rank).ToList();
            return IsAscendingSequence(ranks) || IsWraparoundSequence(ranks);
        }

        private static bool IsAscendingSequence(IReadOnlyCollection<int> sortedRanks)
        {
            return sortedRanks.Zip(sortedRanks.Skip(1), (a, b) => b == a + 1).All(x => x);
        }

        private static bool IsWraparoundSequence(IReadOnlyList<int> sortedRanks)
        {
            return (sortedRanks[0] == 2 && sortedRanks[1] == 3 && sortedRanks[2] == 14) || // A-2-3
                   (sortedRanks[0] == 2 && sortedRanks[1] == 13 && sortedRanks[2] == 14) || // K-A-2
                   (sortedRanks[0] == 12 && sortedRanks[1] == 13 && sortedRanks[2] == 14);  // Q-K-A
        }

        public static Dictionary<Rank, int> GetRankCounts(this Hand hand)
        {
            return hand.Cards.GroupBy(card => card.Rank).ToDictionary(g => g.Key, g => g.Count());
        }

        public static bool IsNOfAKind(this Hand hand, Rank rank, int numberOfCards)
        {
            return hand.GetRankCounts().TryGetValue(rank, out int count) && count == numberOfCards;
        }

        public static bool IsNOfAKind(this Hand hand, int numberOfCards)
        {
            return hand.GetRankCounts().Values.Any(count => count >= numberOfCards);
        }

        public static bool IsNOfAKindOfTrump(this Hand hand, Rank trumpRank, int n)
        {
            return hand.GetRankCounts().TryGetValue(trumpRank, out int count) && count == n;
        }

        public static bool HasTrumpCard(this Hand hand, Card trumpCard)
        {
            return hand.Cards.Any(card => card.Equals(trumpCard));
        }

        public static bool IsRankAdjacent(this Hand hand, Rank rank1, Rank rank2)
        {
            return Math.Abs((int)rank1 - (int)rank2) == 1 ||
                   (rank1 == Rank.A && rank2 == Rank.Two) ||
                   (rank1 == Rank.Two && rank2 == Rank.A);
        }

        public static string GetHandAsSymbols(this Hand hand, bool coloured = true)
        {
            return string.Join(", ", hand.Cards.Select(card => GetRankSymbol(card.Suit, card.Rank, coloured)));
        }

        public static Hand ConvertFromSymbols(string[] cardSymbols)
        {
            return new Hand(ConvertToCardFromSymbols(cardSymbols));
        }


        public static void Print(this Hand hand)
        {
            if (hand?.Cards == null || hand.Cards.Length == 0)
            {
                Debug.Log("Hand is empty or null.");
                return;
            }

            string handAsString = string.Join(", ", hand.Cards.Select(card => GetRankSymbol(card.Suit, card.Rank, true)));
            Debug.Log($"Hand: {handAsString}");
        }

        public static Card GetCard(this Hand hand, int index)
        {
            if (hand?.Cards == null)
            {
                return null;
            }

            if (index < 0 || index >= hand.Cards.Length)
            {
                Debug.LogError("Attempted to access a card at an invalid index.");
                return null;
            }

            return hand.Cards[index];
        }

        public static Card FindWorstCard(this Hand hand)
        {
            return  hand.Cards.Min();
        }

        public static void ReplaceCard(this Hand hand, int index, Card newCard)
        {
            if (hand?.Cards == null)
            {
                Debug.LogError("Hand or Cards array is null.");
                return;
            }

            if (index < 0 || index >= hand.Cards.Length)
            {
                Debug.LogError("Attempted to replace a card at an invalid index.");
                return;
            }

            if (newCard == null)
            {
                Debug.LogError("Attempted to replace a card with a null card.");
                return;
            }

            hand.Cards[index] = newCard;
        }
    }
}
