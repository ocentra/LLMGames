using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using UnityEngine;
using static OcentraAI.LLMGames.Utilities.CardUtility;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [Serializable]
    public class DevCard
    {
        [SerializeField] public Rank Rank = Rank.None;
        [SerializeField] public Suit Suit = Suit.None;

        public DevCard()
        {
        }

        public DevCard(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public DevCard(Card card)
        {
            Suit = card.Suit;
            Rank = card.Rank;
        }

        [HideInInspector] public string ID => $"{Rank.Alias}_{Suit.Symbol}";

        [HideInInspector] public string CardText => GetCardText();

        public string GetCardText(bool coloured = true)
        {
            if (Suit == Suit.None || Rank == Rank.None)
            {
                return "Not Set";
            }

            return GetRankSymbol(Suit, Rank, coloured);
        }


        public static DevCard[] ConvertToDevCardFromSymbols(string[] cardSymbols)
        {
            List<DevCard> devCards = new List<DevCard>();

            foreach (string symbol in cardSymbols)
            {
                Rank rank = Rank.None;
                Suit suit = Suit.None;

                if (symbol.Length == 3) // For "10♠" type symbols
                {
                    rank = Rank.Ten;
                    suit = GetSuitFromChar(symbol[2]);
                }
                else if (symbol.Length == 2) // For "2♠", "J♠" type symbols
                {
                    rank = GetRankFromChar(symbol[0]);
                    suit = GetSuitFromChar(symbol[1]);
                }

                // Create a DevCard instance and add it to the list
                devCards.Add(new DevCard {Suit = suit, Rank = rank});
            }

            return devCards.ToArray();
        }

        public void Clear()
        {
            Suit = Suit.None;
            Rank = Rank.None;
        }
    }
}