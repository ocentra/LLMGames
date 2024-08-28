using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using UnityEditor;
using UnityEngine;
using static OcentraAI.LLMGames.Utilities.CardUtility;

namespace OcentraAI.LLMGames.GameModes.Extensions
{
    public static class CardExtensions
    {
        public static void Print(this Card card)
        {
            Debug.Log($"Card: {GetRankSymbol(card.Suit, card.Rank)}, Suit: {card.Suit}, Rank: {card.Rank}");
        }

        public static bool IsFaceCard(this Card card)
        {
            return card.Rank == Rank.J || card.Rank == Rank.Q || card.Rank == Rank.K;
        }

        public static bool IsRedCard(this Card card)
        {
            return card.Suit == Suit.Heart || card.Suit == Suit.Diamond;
        }

        public static bool IsBlackCard(this Card card)
        {
            return card.Suit == Suit.Spade || card.Suit == Suit.Club;
        }

        public static bool Matches(this Card card, Rank rank, Suit suit)
        {
            return card.Rank == rank && card.Suit == suit;
        }

        public static string GetCardColorString(this Card card)
        {
            return GetColorString(card.Suit);
        }
    }
}