using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    /// Contains aggregation operations on the Hand such as Sum, Max, Min, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        /// Sums the rank values of all cards in the hand.
        /// </summary>
        public static int Sum(this Hand hand)
        {
            return Sum(hand, card => card.Rank.Value);
        }

        /// <summary>
        /// Sums the result of applying a selector function to each card in the hand.
        /// </summary>
        public static int Sum(this Hand hand, Func<Card, int> selector)
        {
            int sum = 0;
            foreach (Card card in hand.GetCards())
            {
                sum += selector(card);
            }
            return sum;
        }

        /// <summary>
        /// Finds the card with the maximum rank value in the hand.
        /// </summary>
        public static int Max(this Hand hand)
        {
            Card maxCard = Max(hand, card => card.Rank.Value);
            return maxCard?.Rank.Value ?? int.MinValue;
        }

        /// <summary>
        /// Finds the card with the maximum value based on a selector function.
        /// </summary>
        public static Card Max(this Hand hand, Func<Card, IComparable> selector)
        {
            if (hand.GetCards().Length == 0) return null;

            Card maxCard = hand.GetCards()[0];
            IComparable maxValue = selector(maxCard);
            for (int i = 1; i < hand.GetCards().Length; i++)
            {
                IComparable currentValue = selector(hand.GetCards()[i]);
                if (currentValue.CompareTo(maxValue) > 0)
                {
                    maxCard = hand.GetCards()[i];
                    maxValue = currentValue;
                }
            }
            return maxCard;
        }

        /// <summary>
        /// Finds the card with the minimum rank value in the hand.
        /// </summary>
        public static int Min(this Hand hand)
        {
            Card minCard = Min(hand, card => card.Rank.Value);
            int rankValue = minCard?.Rank.Value ?? int.MaxValue;
            return rankValue;
        }

        /// <summary>
        /// Finds the card with the minimum value based on a selector function.
        /// </summary>
        public static Card Min(this Hand hand, Func<Card, IComparable> selector)
        {
            if (hand.GetCards().Length == 0) return null;

            Card minCard = hand.GetCards()[0];
            IComparable minValue = selector(minCard);
            for (int i = 1; i < hand.GetCards().Length; i++)
            {
                IComparable currentValue = selector(hand.GetCards()[i]);
                if (currentValue.CompareTo(minValue) < 0)
                {
                    minCard = hand.GetCards()[i];
                    minValue = currentValue;
                }
            }
            return minCard;
        }
    }
}
