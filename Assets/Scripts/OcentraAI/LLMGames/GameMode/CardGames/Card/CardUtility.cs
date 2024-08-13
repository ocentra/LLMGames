using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using static OcentraAI.LLMGames.Utility;

namespace OcentraAI.LLMGames.Utilities
{
    public static class CardUtility
    {
        public static string GetColorString(Suit suit)
        {
            return suit is Suit.Hearts or Suit.Diamonds ? "Red" : "Black";
        }

        public static Color GetColorValue(Suit suit)
        {
            return suit is Suit.Hearts or Suit.Diamonds ? Color.red : Color.black;
        }

        public static Rank GetRankFromChar(char rankChar)
        {
            return rankChar switch
            {
                '2' => Rank.Two,
                '3' => Rank.Three,
                '4' => Rank.Four,
                '5' => Rank.Five,
                '6' => Rank.Six,
                '7' => Rank.Seven,
                '8' => Rank.Eight,
                '9' => Rank.Nine,
                'J' => Rank.J,
                'Q' => Rank.Q,
                'K' => Rank.K,
                'A' => Rank.A,
                _ => Rank.None
            };
        }

        public static Suit GetSuitFromChar(char suitChar)
        {
            return suitChar switch
            {
                '♠' => Suit.Spades,
                '♥' => Suit.Hearts,
                '♦' => Suit.Diamonds,
                '♣' => Suit.Clubs,
                _ => Suit.None
            };
        }

        public static Card[] ConvertToCardFromSymbols(string[] cardSymbols)
        {
            List<Card> cards = new List<Card>();

            foreach (string symbol in cardSymbols)
            {
                Card card = GetCardFromSymbol(symbol);

                cards.Add(card);
            }

            return cards.ToArray();
        }

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

        public static Rank[] GetAvailableRanks(Rank[] excludeRanks = null)
        {
            IEnumerable<Rank> availableRanks = Enum.GetValues(typeof(Rank))
                .Cast<Rank>()
                .Where(r => r != Rank.None);

            if (excludeRanks is { Length: > 0 })
            {
                availableRanks = availableRanks.Except(excludeRanks);
            }

            return availableRanks.ToArray();
        }

        public static Suit[] GetAvailableSuits()
        {
            return Enum.GetValues(typeof(Suit))
                .Cast<Suit>()
                .Where(s => s != Suit.None)
                .ToArray();
        }

        public static List<Rank> SelectRanks(int count, bool allowSequence, bool sameColor = false, bool sameSuit = false, Suit? fixedSuit = null)
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
                    randomRank = availableRanks[UnityEngine.Random.Range(0, availableRanks.Length)];
                } while (!IsValidRankSelection(randomRank, selectedRanks, allowSequence, sameColor, sameSuit, selectedSuit, selectedColor));

                selectedRanks.Add(randomRank);
            }

            return selectedRanks;
        }

        private static bool IsValidRankSelection(Rank newRank, List<Rank> selectedRanks, bool allowSequence, bool sameColor, bool sameSuit, Suit selectedSuit, Color selectedColor)
        {
            if (!allowSequence && selectedRanks.Any(r => Math.Abs((int)r - (int)newRank) == 1))
            {
                return false;
            }

            if (sameColor || sameSuit)
            {
                Suit newSuit = GetValidSuit(newRank, sameColor, sameSuit, selectedSuit, selectedColor);
                return newSuit != Suit.None;
            }

            return true;
        }

        private static Suit GetValidSuit(Rank rank, bool sameColor, bool sameSuit, Suit selectedSuit, Color selectedColor)
        {
            Suit[] validSuits = GetAvailableSuits();

            if (sameSuit)
            {
                return selectedSuit;
            }
            else if (sameColor)
            {
                return validSuits.FirstOrDefault(s => GetColor(s) == selectedColor);
            }

            return validSuits[UnityEngine.Random.Range(0, validSuits.Length)];
        }

        private static Color GetColor(Suit suit)
        {
            return suit is Suit.Hearts or Suit.Diamonds ? Color.red : Color.black;
        }

        public static Suit GetRandomSuit()
        {
            Suit[] availableSuits = GetAvailableSuits();
            return availableSuits[UnityEngine.Random.Range(0, availableSuits.Length)];
        }

        public static string GetRankSymbol(Suit suit, Rank rank, bool coloured = true)
        {
            if (suit == Suit.None || rank == Rank.None)
            {
                return "None";
            }

            string symbol = suit switch
            {
                Suit.Hearts => coloured ? ColouredMessage("♥", Color.red) : "♥",
                Suit.Diamonds => coloured ? ColouredMessage("♦", Color.red) : "♦",
                Suit.Clubs => coloured ? ColouredMessage("♣", Color.black) : "♣",
                Suit.Spades => coloured ? ColouredMessage("♠", Color.black) : "♠",
                _ => ""
            };

            string formattedRank = rank switch
            {
                Rank.A => "A",
                Rank.K => "K",
                Rank.Q => "Q",
                Rank.J => "J",
                _ => ((int)rank).ToString()
            };

            return coloured ? $"{ColouredMessage($"{formattedRank}", GetColorValue(suit))}{symbol}" : $"{formattedRank}{symbol}";
        }

        public static Rank GetRankFromSymbol(string symbol)
        {
            if (symbol.Length == 3) // For "10♠" type symbols
            {
                return Rank.Ten;
            }
            else if (symbol.Length == 2) // For "2♠", "J♠" type symbols
            {
                return GetRankFromChar(symbol[0]);
            }

            return Rank.None;
        }


        public static Rank GetRank(Card trumpCard, bool useTrump, Rank primaryRank, Rank alternateRank)
        {
            return useTrump && trumpCard != null && trumpCard.Rank == primaryRank ? alternateRank : primaryRank;
        }

        public static Suit GetAndRemoveRandomSuit(List<Suit> availableSuits)
        {
            int index = UnityEngine.Random.Range(0, availableSuits.Count);
            Suit selectedSuit = availableSuits[index];
            availableSuits.RemoveAt(index);
            return selectedSuit;
        }

        public static Card GetCard(Suit suit, Rank rank)
        {
            Card card = Deck.Instance.GetCard(suit, rank);
            return card;
        }
    }
}
