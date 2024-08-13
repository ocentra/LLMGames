using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    /// Contains operations for analyzing hands, such as finding the highest/worst cards, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        /// Finds the highest card in the hand that is greater than or equal to a specified minimum rank.
        /// </summary>
        public static Card FindHighestCard(this Hand hand, Rank minimumRank = Rank.J)
        {
            Card highestCard = null;
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                Card card = hand.GetCards()[i];
                if (card.Rank >= minimumRank && (highestCard == null || card.Rank > highestCard.Rank))
                {
                    highestCard = card;
                }
            }
            return highestCard;
        }

        /// <summary>
        /// Finds the worst card in the hand.
        /// </summary>
        public static Card FindWorstCard(this Hand hand)
        {
            if (hand.GetCards().Length == 0) return null;
            Card worstCard = hand.GetCards()[0];
            for (int i = 1; i < hand.GetCards().Length; i++)
            {
                if (hand.GetCards()[i].GetRankValue() < worstCard.GetRankValue())
                {
                    worstCard = hand.GetCards()[i];
                }
            }

            return worstCard;
        }

        /// <summary>
        /// Cleans up the hand by destroying all card objects.
        /// </summary>
        public static void CleanupHand(this Hand hand)
        {
            foreach (Card card in hand.GetCards())
            {
                UnityEngine.Object.DestroyImmediate(card);
            }
        }

        /// <summary>
        /// Prints the hand to the console.
        /// </summary>
        public static void Print(this Hand hand)
        {
            if (hand?.GetCards() == null || hand.GetCards().Length == 0)
            {
                Debug.Log("Hand is empty or null.");
                return;
            }

            string handAsString = GetHandAsSymbols(hand, true);
            Debug.Log($"Hand: {handAsString}");
        }
    }
}
