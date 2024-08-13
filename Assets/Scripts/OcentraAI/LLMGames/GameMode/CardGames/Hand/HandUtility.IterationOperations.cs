using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    /// Contains methods for iterating over the Hand such as ForEach, Any, All, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        /// Executes an action for each card in the hand.
        /// </summary>
        public static void ForEach(this Hand hand, Action<Card> action)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                action(hand.GetCards()[i]);
            }
        }

        /// <summary>
        /// Determines if any card in the hand matches a specific predicate.
        /// </summary>
        public static bool Any(this Hand hand, Func<Card, bool> predicate)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (predicate(hand.GetCards()[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if any card in the hand has a specific ID.
        /// </summary>
        public static bool Any(this Hand hand, string id)
        {
            return Any(hand, card => card.Id == id);
        }

        /// <summary>
        /// Determines if all cards in the hand match a specific predicate.
        /// </summary>
        public static bool All(this Hand hand, Func<Card, bool> predicate)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (!predicate(hand.GetCards()[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds the first card in the hand that matches a specific predicate.
        /// </summary>
        public static Card Find(this Hand hand, Func<Card, bool> predicate)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (predicate(hand.GetCards()[i]))
                {
                    return hand.GetCards()[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the index of the first card in the hand that matches a specific predicate.
        /// </summary>
        public static int FindIndex(this Hand hand, Func<Card, bool> predicate)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (predicate(hand.GetCards()[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds the first card in the hand or returns the default value (null) if the hand is empty.
        /// </summary>
        public static Card FirstOrDefault(this Hand hand)
        {
            if (hand == null || hand.GetCards().Length == 0)
            {
                return null;
            }

            return hand.GetCards()[0];
        }
    }
}
