using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    ///     Contains LINQ-like operations on the Hand such as Where, Select, OrderBy, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        ///     Copies the cards from the source hand to the result hand, filtering by a predicate.
        /// </summary>
        public static Hand WhereTo(this Hand sourceHand, Hand resultHand, Func<Card, bool> predicate)
        {
            List<Card> newCards = new List<Card>(sourceHand.GetCards().Length);
            for (int i = 0; i < sourceHand.GetCards().Length; i++)
            {
                if (predicate(sourceHand.GetCards()[i]))
                {
                    newCards.Add(sourceHand.GetCards()[i]);
                }
            }

            resultHand.SetCards(newCards.ToArray());

            return resultHand;
        }

        /// <summary>
        ///     Concatenates two hands and stores the result in the result hand.
        /// </summary>
        public static Hand ConcatTo(this Hand hand1, Hand hand2, Hand resultHand)
        {
            Card[] newCards = new Card[hand1.GetCards().Length + hand2.GetCards().Length];
            Array.Copy(hand1.GetCards(), newCards, hand1.GetCards().Length);
            Array.Copy(hand2.GetCards(), 0, newCards, hand1.GetCards().Length, hand2.GetCards().Length);
            resultHand.SetCards(newCards);

            return resultHand;
        }

        /// <summary>
        ///     Removes the cards in hand2 from hand1 and stores the result in the result hand.
        /// </summary>
        public static Hand ExceptTo(this Hand hand1, Hand hand2, Hand resultHand)
        {
            HashSet<Card> set = new HashSet<Card>(hand2.GetCards());
            List<Card> newCards = new List<Card>(hand1.GetCards().Length);
            for (int i = 0; i < hand1.GetCards().Length; i++)
            {
                if (!set.Contains(hand1.GetCards()[i]))
                {
                    newCards.Add(hand1.GetCards()[i]);
                }
            }

            resultHand.SetCards(newCards.ToArray());

            return resultHand;
        }

        /// <summary>
        ///     Finds the intersection of two hands and stores the result in the result hand.
        /// </summary>
        public static Hand IntersectTo(this Hand hand1, Hand hand2, Hand resultHand)
        {
            HashSet<Card> set = new HashSet<Card>(hand2.GetCards());
            List<Card> newCards = new List<Card>(Math.Min(hand1.GetCards().Length, hand2.GetCards().Length));
            for (int i = 0; i < hand1.GetCards().Length; i++)
            {
                if (set.Contains(hand1.GetCards()[i]))
                {
                    newCards.Add(hand1.GetCards()[i]);
                }
            }

            resultHand.SetCards(newCards.ToArray());

            return resultHand;
        }


        /// <summary>
        ///     Filters the cards in the hand based on a predicate.
        /// </summary>
        public static Hand Where(this Hand hand, Func<Card, bool> predicate)
        {
            List<Card> result = new List<Card>(hand.GetCards().Length);
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (predicate(hand.GetCards()[i]))
                {
                    result.Add(hand.GetCards()[i]);
                }
            }

            return new Hand(result);
        }

        /// <summary>
        ///     Orders the cards in the hand by rank value in ascending order.
        /// </summary>
        public static Hand OrderBy(this Hand hand)
        {
            return OrderBy(hand, card => card.Rank.Value);
        }

        /// <summary>
        ///     Orders the cards in the hand based on a key selector function.
        /// </summary>
        public static Hand OrderBy(this Hand hand, Func<Card, IComparable> keySelector)
        {
            Card[] sortedCards = (Card[])hand.GetCards().Clone();
            Array.Sort(sortedCards, (a, b) => keySelector(a).CompareTo(keySelector(b)));
            return new Hand(sortedCards);
        }

        /// <summary>
        ///     Orders the cards in the hand by rank value in descending order.
        /// </summary>
        public static Hand OrderByDescending(this Hand hand)
        {
            return OrderByDescending(hand, card => card.Rank.Value);
        }

        /// <summary>
        ///     Orders the cards in the hand based on a key selector function in descending order.
        /// </summary>
        public static Hand OrderByDescending(this Hand hand, Func<Card, IComparable> keySelector)
        {
            Card[] sortedCards = (Card[])hand.GetCards().Clone();
            Array.Sort(sortedCards, (a, b) => keySelector(b).CompareTo(keySelector(a)));
            return new Hand(sortedCards);
        }

        /// <summary>
        ///     Selects a subset of cards from the hand.
        /// </summary>
        public static Hand Take(this Hand hand, int count)
        {
            int newCount = Math.Min(count, hand.GetCards().Length);
            Card[] newCards = new Card[newCount];
            Array.Copy(hand.GetCards(), newCards, newCount);
            return new Hand(newCards);
        }

        /// <summary>
        ///     Skips a specified number of cards in the hand and returns the rest.
        /// </summary>
        public static Hand Skip(this Hand hand, int count)
        {
            if (count >= hand.GetCards().Length)
            {
                return new Hand(Array.Empty<Card>());
            }

            int newCount = hand.GetCards().Length - count;
            Card[] newCards = new Card[newCount];
            Array.Copy(hand.GetCards(), count, newCards, 0, newCount);
            return new Hand(newCards);
        }

        /// <summary>
        ///     Returns distinct cards from the hand.
        /// </summary>
        public static Hand Distinct(this Hand hand)
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

            return new Hand(distinctCards);
        }

        /// <summary>
        ///     Selects cards from the hand based on a selector function.
        /// </summary>
        public static Hand Select(this Hand hand, Func<Card, Card> selector)
        {
            Card[] newCards = new Card[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                newCards[i] = selector(hand.GetCards()[i]);
            }

            return new Hand(newCards);
        }

        /// <summary>
        ///     Selects cards from the hand and applies a function with an index.
        /// </summary>
        public static IEnumerable<TResult> Select<TResult>(this Hand hand, Func<Card, int, TResult> selector)
        {
            Card[] cards = hand.GetCards();
            for (int i = 0; i < cards.Length; i++)
            {
                yield return selector(cards[i], i);
            }
        }

        /// <summary>
        ///     Selects cards from the hand and applies a function.
        /// </summary>
        public static IEnumerable<TResult> Select<TResult>(this Hand hand, Func<Card, TResult> selector)
        {
            foreach (Card card in hand.GetCards())
            {
                yield return selector(card);
            }
        }
    }
}