using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    ///     Contains operations that modify the Hand in-place, such as WhereInPlace, OrderByInPlace, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        ///     Filters the hand in place based on a predicate.
        /// </summary>
        public static Hand WhereInPlace(this Hand hand, Func<Card, bool> predicate)
        {
            List<Card> newCards = new List<Card>(hand.GetCards().Length);
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (predicate(hand.GetCards()[i]))
                {
                    newCards.Add(hand.GetCards()[i]);
                }
            }

            hand.SetCards(newCards.ToArray());

            return hand;
        }

        /// <summary>
        ///     Orders the hand in place based on a key selector function.
        /// </summary>
        public static Hand OrderByInPlace(this Hand hand, Func<Card, IComparable> keySelector)
        {
            Array.Sort(hand.GetCards(), (a, b) => keySelector(a).CompareTo(keySelector(b)));
            return hand;
        }

        /// <summary>
        ///     Orders the hand in place in descending order based on a key selector function.
        /// </summary>
        public static Hand OrderByDescendingInPlace(this Hand hand, Func<Card, IComparable> keySelector)
        {
            Array.Sort(hand.GetCards(), (a, b) => keySelector(b).CompareTo(keySelector(a)));
            return hand;
        }

        /// <summary>
        ///     Takes the first specified number of cards from the hand in place.
        /// </summary>
        public static Hand TakeInPlace(this Hand hand, int count)
        {
            if (count < hand.GetCards().Length)
            {
                Card[] newCards = new Card[count];
                Array.Copy(hand.GetCards(), newCards, count);
                hand.SetCards(newCards);
            }

            return hand;
        }

        /// <summary>
        ///     Skips a specified number of cards in place and returns the rest.
        /// </summary>
        public static Hand SkipInPlace(this Hand hand, int count)
        {
            if (count >= hand.GetCards().Length)
            {
                hand.SetCards(Array.Empty<Card>());
            }
            else
            {
                int newLength = hand.GetCards().Length - count;
                Card[] newCards = new Card[newLength];
                Array.Copy(hand.GetCards(), count, newCards, 0, newLength);
                hand.SetCards(newCards);
            }

            return hand;
        }

        /// <summary>
        ///     Removes duplicate cards from the hand in place.
        /// </summary>
        public static Hand DistinctInPlace(this Hand hand)
        {
            List<Card> distinctCards = new List<Card>();
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                bool isDuplicate = false;
                for (int j = 0; j < distinctCards.Count; j++)
                {
                    if (distinctCards[j].Equals(hand.GetCards()[i]))
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    distinctCards.Add(hand.GetCards()[i]);
                }
            }

            hand.SetCards(distinctCards.ToArray());

            return hand;
        }

        /// <summary>
        ///     Modifies the hand in place by selecting cards based on a selector function.
        /// </summary>
        public static Hand SelectInPlace(this Hand hand, Func<Card, Card> selector)
        {
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                hand.GetCards()[i] = selector(hand.GetCards()[i]);
            }

            return hand;
        }
    }
}