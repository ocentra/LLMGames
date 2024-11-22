using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    ///     Contains basic operations on the Hand such as Add, Contains, Count, GetCard, ReplaceCard, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        ///     Adds a card to the hand.
        /// </summary>
        public static void Add(this Hand hand, Card drawCard)
        {
            Card[] newCards = new Card[hand.GetCards().Length + 1];
            Array.Copy(hand.GetCards(), newCards, hand.GetCards().Length);
            newCards[hand.GetCards().Length] = drawCard;
            hand.SetCards(newCards);
        }

        /// <summary>
        ///     Checks if the hand contains a specific card.
        /// </summary>
        public static bool Contains(this Hand hand, Card card)
        {
            if (card == null)
            {
                return false;
            }

            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (hand.GetCards()[i].Equals(card))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if the hand contains any card from a list of cards.
        /// </summary>
        public static bool Contains(this Hand hand, List<Card> cards)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                for (int j = 0; j < cards.Count; j++)
                {
                    if (hand.GetCards()[i].Equals(cards[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Counts the number of cards in the hand.
        /// </summary>
        public static int Count(this Hand hand)
        {
            return Count(hand, _ => true);
        }

        /// <summary>
        ///     Counts the number of cards in the hand that match a specific predicate.
        /// </summary>
        public static int Count(this Hand hand, Func<Card, bool> predicate)
        {
            int count = 0;
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (predicate(hand.GetCards()[i]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        ///     Gets a card from the hand by its index.
        /// </summary>
        public static Card GetCard(this Hand hand, int index)
        {
            if (hand?.GetCards() == null)
            {
                return null;
            }

            if (index < 0 || index >= hand.GetCards().Length)
            {
                Debug.LogError("Attempted to access a card at an invalid index.");
                return null;
            }

            return hand.GetCards()[index];
        }

        /// <summary>
        ///     Replaces a card in the hand at a specific index with a new card.
        /// </summary>
        public static void ReplaceCard(this Hand hand, int index, Card newCard)
        {
            if (hand?.GetCards() == null)
            {
                Debug.LogError("Hand or Cards array is null.");
                return;
            }

            if (index < 0 || index >= hand.GetCards().Length)
            {
                Debug.LogError("Attempted to replace a card at an invalid index.");
                return;
            }

            if (newCard == null)
            {
                Debug.LogError("Attempted to replace a card with a null card.");
                return;
            }

            hand.GetCards()[index] = newCard;
        }
    }
}