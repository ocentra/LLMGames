using OcentraAI.LLMGames.Scriptable;
using UnityEditor;
using UnityEngine;
using static OcentraAI.LLMGames.Utilities.CardUtility;

namespace OcentraAI.LLMGames.GameModes.Extensions
{
    public static class CardExtensions
    {
        public static void Print(this Card card)
        {
            Debug.Log($"Card: {card.RankSymbol}, Suit: {card.Suit}, Rank: {card.Rank}");
        }

        public static bool IsFaceCard(this Card card)
        {
            return card.Rank is Rank.J or Rank.Q or Rank.K;
        }

        public static bool IsRedCard(this Card card)
        {
            return card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds;
        }

        public static bool IsBlackCard(this Card card)
        {
            return card.Suit == Suit.Spades || card.Suit == Suit.Clubs;
        }

        public static bool Matches(this Card card, Rank rank, Suit suit)
        {
            return card.Rank == rank && card.Suit == suit;
        }

        public static string GetCardColorString(this Card card)
        {
            return GetColorString(card.Suit);
        }
        public static void SaveChanges(this Card card)
        {
            EditorUtility.SetDirty(card);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}