using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    /// Contains comparison operations on the Hand such as verifying hand, checking flushes, sequences, etc.
    /// </summary>
    public static partial class HandUtility
    {
        /// <summary>
        /// Verifies if the hand is valid for a specific game mode.
        /// </summary>
        public static bool VerifyHand(this Hand hand, GameMode gameMode, int minNumberOfCard)
        {
            if (hand != null && gameMode != null && gameMode.NumberOfCards >= minNumberOfCard && hand.Count() == gameMode.NumberOfCards)
            {
                for (int i = 0; i < hand.GetCards().Length; i++)
                {
                    if (hand.GetCards()[i].Rank == Rank.None || hand.GetCards()[i].Suit == Suit.None)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the hand is a flush (all cards have the same suit).
        /// </summary>
        public static bool IsFlush(this Hand hand)
        {
            if (hand.GetCards().Length == 0) return false;
            Suit firstSuit = hand.GetCards()[0].Suit;
            for (int i = 1; i < hand.GetCards().Length; i++)
            {
                if (hand.GetCards()[i].Suit != firstSuit) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if all cards in the hand are of the same color but different suits.
        /// </summary>
        public static bool IsSameColorAndDifferentSuits(this Hand hand)
        {
            if (hand == null || hand.Count() == 0) return false;

            Color firstCardColor = CardUtility.GetColorValue(hand.GetCards()[0].Suit);

            for (int i = 1; i < hand.GetCards().Length; i++)
            {
                if (CardUtility.GetColorValue(hand.GetCards()[i].Suit) != firstCardColor)
                {
                    return false;
                }
            }

            return !hand.IsSameSuits();
        }


        /// <summary>
        /// Determines if all cards in the hand have the same suit.
        /// </summary>
        public static bool IsSameSuits(this Hand hand)
        {
            if (hand.GetCards().Length == 0) return true;
            Suit firstSuit = hand.GetCards()[0].Suit;
            for (int i = 1; i < hand.GetCards().Length; i++)
            {
                if (hand.GetCards()[i].Suit != firstSuit) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the hand forms a sequence.
        /// </summary>
        public static bool IsSequence(this Hand hand)
        {
            if (hand.GetCards().Length < 2) return false;
            int[] ranks = new int[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                ranks[i] = hand.GetCards()[i].Rank.Value;
            }

            Array.Sort(ranks);
            return IsAscendingSequence(ranks) || IsWraparoundSequence(ranks);
        }

        private static bool IsAscendingSequence(IReadOnlyList<int> sortedRanks)
        {
            for (int i = 1; i < sortedRanks.Count; i++)
            {
                if (sortedRanks[i] != sortedRanks[i - 1] + 1) return false;
            }

            return true;
        }

        private static bool IsWraparoundSequence(IReadOnlyList<int> sortedRanks)
        {
            bool isFirstConditionMet = sortedRanks[0] == 2 && sortedRanks[1] == 3 && sortedRanks[2] == 14; // A-2-3
            bool isSecondConditionMet = sortedRanks[0] == 2 && sortedRanks[1] == 13 && sortedRanks[2] == 14; // K-A-2
            bool isThirdConditionMet = sortedRanks[0] == 12 && sortedRanks[1] == 13 && sortedRanks[2] == 14; // Q-K-A

            return isFirstConditionMet || isSecondConditionMet || isThirdConditionMet;
        }
    }
}
