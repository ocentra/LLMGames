using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    public static partial class HandUtility
    {
        /// <summary>
        ///     Checks if the array contains a specified item.
        /// </summary>
        public static bool ArrayContains<T>(T[] array, T item)
        {
            foreach (T t in array)
            {
                if (EqualityComparer<T>.Default.Equals(t, item))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        ///     Selects ranks from the deck that are not high-ranking and do not match the trump rank.
        /// </summary>
        public static List<Rank> SelectNonHighRanks(int handSize, Rank trumpRank)
        {
            IRandom random = new UnityRandom();
            List<Rank> availableRanks = new List<Rank>();
            foreach (Rank rank in Rank.GetStandardRanks())
            {
                if (rank != Rank.J && rank != Rank.Q && rank != Rank.K && rank != Rank.A &&
                    trumpRank != Rank.None && rank != trumpRank)
                {
                    availableRanks.Add(rank);
                }
            }

            List<Rank> selectedRanks = new List<Rank>();
            for (int i = 0; i < handSize && availableRanks.Count > 0; i++)
            {
                int randomIndex = random.Range(0, availableRanks.Count);
                selectedRanks.Add(availableRanks[randomIndex]);
                availableRanks.RemoveAt(randomIndex);
            }

            return selectedRanks;
        }

        /// <summary>
        ///     Determines if the hand can form a sequence using a wild card.
        /// </summary>
        public static bool CanFormSequenceWithWild(this Hand hand)
        {
            if (hand.GetCards().Length < 3)
            {
                Debug.LogWarning("Hand has fewer than 3 cards.");
                return false;
            }

            int[] ranks = new int[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                ranks[i] = hand.GetCards()[i].Rank.Value;
            }

            Array.Sort(ranks);

            // Check for normal sequence
            bool isNormalSequence = true;
            for (int i = 1; i < ranks.Length; i++)
            {
                if (ranks[i] > ranks[i - 1] + 2)
                {
                    isNormalSequence = false;
                    break;
                }
            }

            if (isNormalSequence)
            {
                return true;
            }

            // Check for wraparound sequence
            return (ranks[0] == 2 && ranks[1] <= 4) || // x-2-3, x-2-4
                   (ranks[0] == 2 && ranks[^1] == 14) || // 2-x-A
                   (ranks[^2] == 13 && ranks[^1] == 14) || // Q-K-x, x-K-A
                   (ranks[0] == 12 && ranks[^1] == 14); // Q-x-A
        }

        /// <summary>
        ///     Gets the optimal value for a wild card to complete a sequence.
        /// </summary>
        public static int GetOptimalWildCardValue(this Hand hand)
        {
            if (hand.GetCards().Length == 0)
            {
                return 0;
            }

            int[] ranks = new int[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                ranks[i] = hand.GetCards()[i].Rank.Value;
            }

            Array.Sort(ranks);

            if (ranks[0] == 2 && ranks[1] == 3)
            {
                return 4; // A-2-3
            }

            if (ranks[0] == 2 && ranks[^1] == 13)
            {
                return 14; // K-A-2
            }

            if (ranks[0] == 12 && ranks[1] == 13)
            {
                return 14; // Q-K-A
            }

            if (ranks[1] == ranks[0] + 1)
            {
                return Math.Min(ranks[1] + 1, 14);
            }

            return ranks[0] + 1;
        }
    }
}