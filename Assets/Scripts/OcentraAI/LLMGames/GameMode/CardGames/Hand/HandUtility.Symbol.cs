using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    ///     Contains operations related to card symbols and formatting, such as converting to symbols, formatting hands, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        ///     Generates a list of card symbols representing a Straight Flush for the specified number of cards.
        /// </summary>
        public static List<string> GetStraightFlushAsSymbol(int numberOfCards, bool coloured = true)
        {
            List<Rank> sequence = GetHighestSequence(numberOfCards);
            Suit[] availableSuits = CardUtility.GetAvailableSuits();
            Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits.ToList());
            List<string> cardSymbols = new List<string>();

            foreach (Rank rank in sequence)
            {
                string cardSymbol = CardUtility.GetRankSymbol(suit, rank, coloured);
                cardSymbols.Add(cardSymbol);
            }

            return cardSymbols;
        }

        /// <summary>
        ///     Converts a list of ranks into a list of card symbols.
        /// </summary>
        public static List<string> ToCardSymbols(this List<Rank> ranks, bool coloured = true)
        {
            List<string> cardSymbols = new List<string>();
            List<Suit> availableSuits = CardUtility.GetAvailableSuits().ToList();

            foreach (Rank rank in ranks)
            {
                Suit randomSuit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                cardSymbols.Add(CardUtility.GetRankSymbol(randomSuit, rank, coloured));
            }

            return cardSymbols;
        }

        /// <summary>
        ///     Converts a sequence of rank values into a list of card symbols.
        /// </summary>
        public static List<string> ToCardSymbols(this List<int> sequence, List<Suit> availableSuits,
            bool coloured = true)
        {
            List<string> cardSymbols = new List<string>();
            foreach (int rankValue in sequence)
            {
                Rank rank = Rank.GetStandardRanks().FirstOrDefault(r => r.Value == rankValue);
                if (rank == null)
                {
                    Debug.LogWarning($"No CardRank found for value {rankValue}");
                    continue;
                }

                Suit randomSuit = CardUtility.GetAndRemoveRandomSuit(availableSuits);
                cardSymbols.Add(CardUtility.GetRankSymbol(randomSuit, rank, coloured));
            }

            return cardSymbols;
        }

        /// <summary>
        ///     Returns a formatted string representing the hand.
        /// </summary>
        public static string GetFormattedHand(this Hand hand)
        {
            string[] symbols = new string[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                symbols[i] = hand.GetCards()[i].RankSymbol;
            }

            return string.Join(" ", symbols);
        }

        /// <summary>
        ///     Converts the hand into a string of symbols.
        /// </summary>
        public static string GetHandAsSymbols(this Hand hand, bool coloured = true)
        {
            string[] symbols = new string[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                symbols[i] = CardUtility.GetRankSymbol(hand.GetCards()[i].Suit, hand.GetCards()[i].Rank, coloured);
            }

            return string.Join(", ", symbols);
        }

        /// <summary>
        ///     Converts a list of card symbol strings into a hand object and returns the string of symbols.
        /// </summary>
        public static string GetHandAsSymbols(List<string> cards, bool coloured = true)
        {
            Card[] convertedCards = CardUtility.ConvertToCardFromSymbols(cards.ToArray());
            return GetHandAsSymbols(new Hand(convertedCards), coloured);
        }

        /// <summary>
        ///     Converts an array of cards into a string of symbols.
        /// </summary>
        public static string GetHandAsSymbols(Card[] cards, bool coloured)
        {
            if (cards == null)
            {
                Debug.LogWarning("GetHandAsSymbols: cards array is null");
                return string.Empty;
            }

            string[] symbols = new string[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    symbols[i] = CardUtility.GetRankSymbol(cards[i].Suit, cards[i].Rank, coloured);
                }
            }

            return string.Join(", ", symbols);
        }

        /// <summary>
        ///     Converts an array of card symbols into a hand object.
        /// </summary>
        public static Hand ConvertFromSymbols(string[] cardSymbols)
        {
            return new Hand(CardUtility.ConvertToCardFromSymbols(cardSymbols));
        }
    }
}