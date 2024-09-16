using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System.Collections.Generic;
using System;
using UnityEngine;
using static OcentraAI.LLMGames.Utility;

namespace OcentraAI.LLMGames.Utilities
{
    /// <summary>
    /// Provides utility methods for card-related operations.
    /// </summary>
    public static class CardUtility
    {
        /// <summary>
        /// Gets the color string representation of a suit.
        /// </summary>
        /// <param name="suit">The suit to get the color for.</param>
        /// <returns>A string representing the color of the suit.</returns>
        public static string GetColorString(Suit suit)
        {
            string color;
            if (suit == Suit.Heart || suit == Suit.Diamond)
            {
                color = "Red";
            }
            else
            {
                color = "Black";
            }
            return color;
        }

        /// <summary>
        /// Gets the Unity Color value for a suit.
        /// </summary>
        /// <param name="suit">The suit to get the color for.</param>
        /// <returns>A Unity Color representing the color of the suit.</returns>
        public static Color GetColorValue(Suit suit)
        {
            Color color;
            if (suit == Suit.Heart || suit == Suit.Diamond)
            {
                color = Color.red;
            }
            else
            {
                color = Color.black;
            }
            return color;
        }

        /// <summary>
        /// Converts a character to its corresponding CardRank.
        /// </summary>
        /// <param name="rankChar">The character representing a card rank.</param>
        /// <returns>The corresponding CardRank.</returns>
        public static Rank GetRankFromChar(char rankChar)
        {
            Rank rank;
            switch (rankChar)
            {
                case '2': rank = Rank.Two; break;
                case '3': rank = Rank.Three; break;
                case '4': rank = Rank.Four; break;
                case '5': rank = Rank.Five; break;
                case '6': rank = Rank.Six; break;
                case '7': rank = Rank.Seven; break;
                case '8': rank = Rank.Eight; break;
                case '9': rank = Rank.Nine; break;
                case 'T': rank = Rank.Ten; break;
                case 'J': rank = Rank.J; break;
                case 'Q': rank = Rank.Q; break;
                case 'K': rank = Rank.K; break;
                case 'A': rank = Rank.A; break;
                default: rank = Rank.None; break;
            }
            return rank;
        }

        /// <summary>
        /// Converts a character to its corresponding Suit.
        /// </summary>
        /// <param name="suitChar">The character representing a suit.</param>
        /// <returns>The corresponding Suit.</returns>
        public static Suit GetSuitFromChar(char suitChar)
        {
            Suit suit;
            switch (suitChar)
            {
                case '♠': suit = Suit.Spade; break;
                case '♥': suit = Suit.Heart; break;
                case '♦': suit = Suit.Diamond; break;
                case '♣': suit = Suit.Club; break;
                default: suit = Suit.None; break;
            }
            return suit;
        }

        /// <summary>
        /// Converts an array of card symbols to an array of Card objects.
        /// </summary>
        /// <param name="cardSymbols">An array of card symbols.</param>
        /// <returns>An array of Card objects.</returns>
        public static Card[] ConvertToCardFromSymbols(string[] cardSymbols)
        {
            List<Card> cards = new List<Card>();

            foreach (string symbol in cardSymbols)
            {
                Card card = GetCardFromSymbol(symbol);

                if (card != null)
                {
                    cards.Add(card);
                }
            }

            return cards.ToArray();
        }

        /// <summary>
        /// Creates a Card object from a card symbol.
        /// </summary>
        /// <param name="symbol">The card symbol.</param>
        /// <returns>A Card object, or null if the symbol is invalid.</returns>
        public static Card GetCardFromSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return null;
            }

            Rank rank = Rank.None;
            Suit suit = Suit.None;

            if (symbol.Length == 3)
            {
                rank = Rank.Ten;
                suit = GetSuitFromChar(symbol[2]);
            }
            else if (symbol.Length == 2)
            {
                rank = GetRankFromChar(symbol[0]);
                suit = GetSuitFromChar(symbol[1]);
            }

            Card card = Deck.Instance.GetCard(suit, rank);
            return card;
        }

        /// <summary>
        /// Gets an array of available CardRanks, optionally excluding specified ranks.
        /// </summary>
        /// <param name="excludeRanks">An array of CardRanks to exclude.</param>
        /// <returns>An array of available CardRanks.</returns>
        public static Rank[] GetAvailableRanks(Rank[] excludeRanks = null)
        {
            List<Rank> availableRanks = new List<Rank>(Rank.GetStandardRanks());

            if (excludeRanks != null && excludeRanks.Length > 0)
            {
                foreach (Rank rank in excludeRanks)
                {
                    availableRanks.Remove(rank);
                }
            }

            return availableRanks.ToArray();
        }

        /// <summary>
        /// Gets an array of all available Suits.
        /// </summary>
        /// <returns>An array of available Suits.</returns>
        public static Suit[] GetAvailableSuits()
        {
            List<Suit> suits = new List<Suit>();
            foreach (Suit suit in Suit.GetStandardSuits())
            {
                if (suit != Suit.None)
                {
                    suits.Add(suit);
                }
            }
            return suits.ToArray();
        }

        /// <summary>
        /// Selects a list of CardRanks based on specified criteria.
        /// </summary>
        /// <param name="count">The number of ranks to select.</param>
        /// <param name="allowSequence">Whether to allow sequential ranks.</param>
        /// <param name="sameColor">Whether all ranks should be of the same color.</param>
        /// <param name="sameSuit">Whether all ranks should be of the same suit.</param>
        /// <param name="fixedSuit">A specific suit to use, if any.</param>
        /// <returns>A list of selected CardRanks.</returns>
        public static List<Rank> SelectRanks(int count, bool allowSequence, bool sameColor = false, bool sameSuit = false, Suit fixedSuit = null)
        {
            List<Rank> selectedRanks = new List<Rank>();
            Rank[] availableRanks = GetAvailableRanks();
            Suit[] suits = GetAvailableSuits();

            Suit selectedSuit = fixedSuit ?? GetRandomSuit();
            Color selectedColor = GetColor(selectedSuit);

            for (int i = 0; i < count; i++)
            {
                Rank randomRank;
                do
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableRanks.Length);
                    randomRank = availableRanks[randomIndex];
                } while (!IsValidRankSelection(randomRank, selectedRanks, allowSequence, sameColor, sameSuit, selectedSuit, selectedColor));

                selectedRanks.Add(randomRank);
            }

            return selectedRanks;
        }

        /// <summary>
        /// Checks if a rank selection is valid based on specified criteria.
        /// </summary>
        private static bool IsValidRankSelection(Rank newRank, List<Rank> selectedRanks, bool allowSequence, bool sameColor, bool sameSuit, Suit selectedSuit, Color selectedColor)
        {
            // Handle sequence logic
            if (allowSequence)
            {
                if (selectedRanks.Count > 0)
                {
                    Rank lastSelectedRank = selectedRanks[selectedRanks.Count - 1];
                    if (Math.Abs(lastSelectedRank.Value - newRank.Value) != 1)
                    {
                        return false; 
                    }
                }
            }
            else
            {
                foreach (Rank rank in selectedRanks)
                {
                    if (Math.Abs(rank.Value - newRank.Value) == 1)
                    {
                        return false; 
                    }
                }
            }

            // Handle same color or same suit logic
            if (sameColor || sameSuit)
            {
                Suit newSuit = GetValidSuit(newRank, sameColor, sameSuit, selectedSuit, selectedColor);
                return newSuit != Suit.None; 
            }
            else
            {
                return true; 
            }
        }


        /// <summary>
        /// Gets a valid suit based on specified criteria.
        /// </summary>
        private static Suit GetValidSuit(Rank rank, bool sameColor, bool sameSuit, Suit selectedSuit, Color selectedColor)
        {
            Suit[] validSuits = GetAvailableSuits();

            if (sameSuit)
            {
                return selectedSuit;
            }
            else if (sameColor)
            {
                foreach (Suit suit in validSuits)
                {
                    if (GetColor(suit) == selectedColor)
                    {
                        return suit;
                    }
                }
            }

            int randomIndex = UnityEngine.Random.Range(0, validSuits.Length);
            return validSuits[randomIndex];
        }

        /// <summary>
        /// Gets the color of a suit.
        /// </summary>
        private static Color GetColor(Suit suit)
        {
            Color color;
            if (suit == Suit.Heart || suit == Suit.Diamond)
            {
                color = Color.red;
            }
            else
            {
                color = Color.black;
            }
            return color;
        }

        /// <summary>
        /// Gets a random suit.
        /// </summary>
        /// <returns>A randomly selected Suit.</returns>
        public static Suit GetRandomSuit()
        {
            Suit[] availableSuits = GetAvailableSuits();
            int randomIndex = UnityEngine.Random.Range(0, availableSuits.Length);
            return availableSuits[randomIndex];
        }

        /// <summary>
        /// Gets a string representation of a card's rank and suit.
        /// </summary>
        /// <param name="suit">The card's suit.</param>
        /// <param name="rank">The card's rank.</param>
        /// <param name="coloured">Whether to apply color to the output.</param>
        /// <returns>A string representation of the card.</returns>
        public static string GetRankSymbol(Suit suit, Rank rank, bool coloured = true)
        {
            if (suit == Suit.None || rank == null || rank == Rank.None)
            {
                return "None";
            }

            string symbol;
            if (suit == Suit.Heart)
            {
                symbol = coloured ? ColouredMessage("♥", Color.red) : "♥";
            }
            else if (suit == Suit.Diamond)
            {
                symbol = coloured ? ColouredMessage("♦", Color.red) : "♦";
            }
            else if (suit == Suit.Club)
            {
                symbol = coloured ? ColouredMessage("♣", Color.black) : "♣";
            }
            else if (suit == Suit.Spade)
            {
                symbol = coloured ? ColouredMessage("♠", Color.black) : "♠";
            }
            else
            {
                symbol = "";
            }

            string result;
            if (coloured)
            {
                result = $"{ColouredMessage($"{rank.Name}", GetColorValue(suit))}{symbol}";
            }
            else
            {
                result = $"{rank.Name}{symbol}";
            }

            return result;
        }

        /// <summary>
        /// Gets the CardRank from a card symbol.
        /// </summary>
        /// <param name="symbol">The card symbol.</param>
        /// <returns>The corresponding CardRank.</returns>
        public static Rank GetRankFromSymbol(string symbol)
        {
            Rank rank;
            if (symbol.Length == 3) // For "10♠" type symbols
            {
                rank = Rank.Ten;
            }
            else if (symbol.Length == 2) // For "2♠", "J♠" type symbols
            {
                rank = GetRankFromChar(symbol[0]);
            }
            else
            {
                rank = Rank.None;
            }
            return rank;
        }

        /// <summary>
        /// Determines the rank to use based on the trump card and specified ranks.
        /// </summary>
        /// <param name="trumpCard">The trump card.</param>
        /// <param name="useTrump">Whether to use the trump card.</param>
        /// <param name="primaryRank">The primary rank to consider.</param>
        /// <param name="alternateRank">The alternate rank to use if the primary rank matches the trump card.</param>
        /// <returns>The determined CardRank.</returns>
        public static Rank GetRank(Card trumpCard, bool useTrump, Rank primaryRank, Rank alternateRank)
        {
            Rank result;
            if (useTrump && trumpCard != null && trumpCard.Rank == primaryRank)
            {
                result = alternateRank;
            }
            else
            {
                result = primaryRank;
            }
            return result;
        }

        /// <summary>
        /// Selects and removes a random suit from a list of available suits.
        /// </summary>
        /// <param name="availableSuits">The list of available suits.</param>
        /// <returns>The randomly selected and removed Suit.</returns>
        public static Suit GetAndRemoveRandomSuit(List<Suit> availableSuits)
        {
            int index = UnityEngine.Random.Range(0, availableSuits.Count);
            Suit selectedSuit = availableSuits[index];
            availableSuits.RemoveAt(index);
            return selectedSuit;
        }

        /// <summary>
        /// Gets a Card object for a given suit and rank.
        /// </summary>
        /// <param name="suit">The suit of the card.</param>
        /// <param name="rank">The rank of the card.</param>
        /// <returns>A Card object.</returns>
        public static Card GetCard(Suit suit, Rank rank)
        {
            Card card = Deck.Instance.GetCard(suit, rank);
            return card;
        }
    }
}