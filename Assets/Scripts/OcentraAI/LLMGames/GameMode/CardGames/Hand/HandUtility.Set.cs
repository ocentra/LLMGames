using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    ///     Contains set operations on the Hand such as Concat, Except, Intersect, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        ///     Concatenates two hands into one.
        /// </summary>
        public static Hand Concat(this Hand hand1, Hand hand2)
        {
            Card[] newCards = new Card[hand1.GetCards().Length + hand2.GetCards().Length];
            Array.Copy(hand1.GetCards(), newCards, hand1.GetCards().Length);
            Array.Copy(hand2.GetCards(), 0, newCards, hand1.GetCards().Length, hand2.GetCards().Length);
            return new Hand(newCards);
        }

        /// <summary>
        ///     Returns the set difference between two hands.
        /// </summary>
        public static Hand Except(this Hand hand, Hand hand2)
        {
            HashSet<Card> set = new HashSet<Card>(hand2.GetCards());
            List<Card> result = new List<Card>(hand.GetCards().Length);
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (!set.Contains(hand.GetCards()[i]))
                {
                    result.Add(hand.GetCards()[i]);
                }
            }

            return new Hand(result);
        }

        /// <summary>
        ///     Returns the intersection of two hands.
        /// </summary>
        public static Hand Intersect(this Hand hand1, Hand hand2)
        {
            HashSet<Card> set = new HashSet<Card>(hand2.GetCards());
            List<Card> result = new List<Card>(Math.Min(hand1.GetCards().Length, hand2.GetCards().Length));
            for (int i = 0; i < hand1.GetCards().Length; i++)
            {
                if (set.Contains(hand1.GetCards()[i]))
                {
                    result.Add(hand1.GetCards()[i]);
                }
            }

            return new Hand(result);
        }
    }
}