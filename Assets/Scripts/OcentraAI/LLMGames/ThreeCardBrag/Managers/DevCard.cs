using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using static OcentraAI.LLMGames.Utilities.CardUtility;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [System.Serializable]
    public class DevCard
    {
        [ValueDropdown("GetAvailableSuits")]
        public Suit Suit = Suit.None;

        [ValueDropdown("GetAvailableRanks")]
        public Rank Rank = Rank.None;

        [ShowInInspector, ReadOnly]
        public string CardText => GetCardText();


        public DevCard()
        {
            
        }

        public DevCard(Card card)
        {
            Suit = card.Suit;
            Rank = card.Rank;
        }

        public string GetCardText(bool coloured = true)
        {
            if (Suit == Suit.None || Rank == Rank.None)
                return "Not Set";

            return GetRankSymbol(Suit, Rank, coloured);
        }

        private static List<Suit> GetAvailableSuits()
        {
            return System.Enum.GetValues(typeof(Suit)).Cast<Suit>().ToList();
        }

        private List<Rank> GetAvailableRanks()
        {
            return System.Enum.GetValues(typeof(Rank)).Cast<Rank>().ToList();
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
                devCards.Add(new DevCard { Suit = suit, Rank = rank });
            }

            return devCards.ToArray();
        }


        [Button("Clear")]
        public void Clear()
        {
            Suit = Suit.None;
            Rank = Rank.None;
        }
    }
}