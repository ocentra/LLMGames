using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    /// <summary>
    /// Contains operations for combinations such as N-of-a-kind, Full House, Pair, etc.
    /// </summary>
    public static partial class HandUtility
    {

        /// <summary>
        /// Determines if the hand has N-of-a-Kind for a specific rank.
        /// </summary>
        public static bool IsNOfAKind(this Hand hand, Rank rank, int numberOfCards)
        {
            int count = 0;
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                if (hand.GetCards()[i].Rank == rank)
                {
                    count++;
                }
            }
            return count == numberOfCards;
        }

        /// <summary>
        /// Determines if the hand has N-of-a-Kind for any rank.
        /// </summary>
        public static bool IsNOfAKind(this Hand hand, int numberOfCards)
        {
            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();

            foreach (var count in rankCounts.Values)
            {
                if (count == numberOfCards)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the highest sequence of ranks in the hand.
        /// </summary>
        public static List<Rank> GetHighestSequence(int numberOfCards)
        {
            List<Rank> baseSequence = new List<Rank>
            {
                Rank.Two, Rank.Three, Rank.Four, Rank.Five, Rank.Six,
                Rank.Seven, Rank.Eight, Rank.Nine, Rank.Ten,
                Rank.J, Rank.Q, Rank.K, Rank.A
            };

            List<Rank> result = new List<Rank>();

            int maxRanks = Math.Min(numberOfCards, baseSequence.Count);
            int startIndex = baseSequence.Count - maxRanks;

            for (int i = startIndex; i < baseSequence.Count; i++)
            {
                result.Add(baseSequence[i]);
            }

            return result;
        }

        /// <summary>
        /// Generates a Straight Flush sequence for the specified number of cards.
        /// </summary>
        public static List<Rank> GetStraightFlush(int numberOfCards)
        {
            UnityRandom random = new UnityRandom();
            List<Rank> straightFlush;
            List<Rank> royalSequence = GetRoyalSequenceAsRank(numberOfCards);
            List<Rank> orderedRanks = Rank.GetStandardRanks().OrderBy(r => r.Value).ToList();

            do
            {
                int startIndex = random.Range(0, orderedRanks.Count - numberOfCards + 1);
                straightFlush = new List<Rank>();

                for (int i = 0; i < numberOfCards; i++)
                {
                    int rankIndex = (startIndex + i) % orderedRanks.Count;
                    straightFlush.Add(orderedRanks[rankIndex]);
                }
            } while (IsRoyalSequence(straightFlush, royalSequence));

            return straightFlush;
        }

        /// <summary>
        /// Checks if a sequence is a Royal Sequence.
        /// </summary>
        private static bool IsRoyalSequence(IReadOnlyList<Rank> sequence, IReadOnlyList<Rank> royalSequence)
        {
            if (sequence.Count != royalSequence.Count)
            {
                return false;
            }

            for (int i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] != royalSequence[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsThreeOfAKind(this Hand hand, Card trumpCard, bool useTrump)
        {
            if (hand == null || hand.Count() != 3)
            {
                return false;
            }

            Rank threeOfAKindRank = hand.GetThreeOfAKindRank(trumpCard, useTrump);
            return threeOfAKindRank != Rank.None;
        }
        public static bool IsMultiplePairs(this Hand hand, Card trumpCard, bool useTrump, out List<Rank> pairRanks)
        {
            pairRanks = new List<Rank>();
            if (hand == null || hand.Count() < 4) return false;

            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();
            int trumpCount = useTrump && trumpCard != null ? hand.Count(c => c.Suit == trumpCard.Suit && c.Rank == trumpCard.Rank) : 0;

            pairRanks = rankCounts
                .Where(kv => kv.Value >= 2 || (kv.Value == 1 && trumpCount > 0 && kv.Key == trumpCard.Rank))
                .Select(kv => kv.Key)
                .ToList();

            return pairRanks.Count >= 2;
        }

        public static bool IsMultipleTriplets(this Hand hand, Card trumpCard, GameMode gameMode)
        {
            if (hand == null || hand.Count() < 6) return false;

            List<Rank> triplets = hand.GetTripletRanks(trumpCard, gameMode.UseTrump);
            return triplets.Count >= 2 && !triplets.Contains(Rank.A) && !triplets.Contains(Rank.K);
        }

        public static List<Rank> GetTripletRanks(this Hand hand, Card trumpCard, bool useTrump)
        {
            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();
            int trumpCount = useTrump && trumpCard != null ? hand.Count(c => c.Suit == trumpCard.Suit && c.Rank == trumpCard.Rank) : 0;

            return rankCounts
                .Where(kv => kv.Value >= 3 || (kv.Value == 2 && trumpCount > 0 && kv.Key != trumpCard.Rank))
                .Select(kv => kv.Key)
                .ToList();
        }

        /// <summary>
        /// Gets a Royal Sequence as a list of ranks.
        /// </summary>
        public static List<Rank> GetRoyalSequenceAsRank(int numberOfCards)
        {
            List<Rank> royalRanks = new List<Rank>
            {
                Rank.A, Rank.K, Rank.Q, Rank.J, Rank.Ten,
                Rank.Nine, Rank.Eight, Rank.Seven, Rank.Six
            };

            List<Rank> result = new List<Rank>();

            int maxRanks = Math.Min(numberOfCards, royalRanks.Count);

            for (int i = 0; i < maxRanks; i++)
            {
                result.Add(royalRanks[i]);
            }

            return result;
        }

        /// <summary>
        /// Checks if the hand is a Royal Sequence.
        /// </summary>
        public static bool IsRoyalSequence(this Hand hand, GameMode gameMode)
        {
            if (!hand.IsSameSuits()) return false;

            int[] ranks = new int[hand.GetCards().Length];
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                ranks[i] = hand.GetCards()[i].Rank.Value;
            }

            Array.Sort(ranks);
            int[] royalSequence = GetRoyalSequence(gameMode);

            for (int i = 0; i < ranks.Length; i++)
            {
                if (ranks[i] != royalSequence[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the ranks of a Royal Sequence for the specified game mode.
        /// </summary>
        public static int[] GetRoyalSequence(GameMode gameMode)
        {
            int[] royalRanks = new int[]
            {
                Rank.A.Value, Rank.K.Value, Rank.Q.Value, Rank.J.Value, Rank.Ten.Value,
                Rank.Nine.Value, Rank.Eight.Value, Rank.Seven.Value, Rank.Six.Value
            };

            int[] result = new int[gameMode.NumberOfCards];
            Array.Copy(royalRanks, result, gameMode.NumberOfCards);
            return result;
        }


        /// <summary>
        /// Gets the rank counts of cards in the hand, optionally excluding certain ranks.
        /// </summary>
        public static Dictionary<Rank, int> GetRankCounts(this Hand hand, Rank[] ranksToExclude = null)
        {
            Dictionary<Rank, int> rankCounts = new Dictionary<Rank, int>();
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                Card card = hand.GetCards()[i];
                if (card != null && (ranksToExclude == null || !ArrayContains(ranksToExclude, card.Rank)))
                {
                    rankCounts.TryAdd(card.Rank, 0);
                    rankCounts[card.Rank]++;
                }
            }
            return rankCounts;
        }

        /// <summary>
        /// Determines if the hand is a Full House or N-of-a-Kind including trump cards.
        /// </summary>
        public static bool IsFullHouseOrTrumpOfKind(this Hand hand, Card trumpCard, GameMode gameMode)
        {
            if (hand?.GetCards() == null || gameMode == null || trumpCard == null || trumpCard.Rank == Rank.None)
            {
                return false;
            }

            if (hand.Count() == gameMode.NumberOfCards)
            {
                if (IsNOfAKindOfTrump(hand, trumpCard, gameMode.NumberOfCards))
                {
                    return true;
                }
            }

            return IsFullHouse(hand, trumpCard, gameMode);
        }


        /// <summary>
        /// Determines if the hand is a Full House, considering the presence of a trump card.
        /// </summary>
        public static bool IsFullHouse(this Hand hand, Card trumpCard, GameMode gameMode)
        {
            if (hand == null || hand.Count() < gameMode.NumberOfCards)
            {
                return false;
            }

            Rank primaryRank = CardUtility.GetRank(trumpCard, gameMode.UseTrump, Rank.A, Rank.K);
            Rank secondaryRank = CardUtility.GetRank(trumpCard, gameMode.UseTrump, Rank.K, Rank.Q);

            int primaryCount = hand.Count(c => c != null && c.Rank == primaryRank);
            int secondaryCount = hand.Count(c => c != null && c.Rank == secondaryRank);
            int trumpCount = trumpCard != null ? hand.Count(c => c != null && c.Suit == trumpCard.Suit && c.Rank == trumpCard.Rank) : 0;

            // Adjust counts if the trump card is present and is not of the primary or secondary rank
            if (trumpCount > 0 && trumpCard != null && trumpCard.Rank != primaryRank && trumpCard.Rank != secondaryRank)
            {
                if (primaryCount >= secondaryCount)
                {
                    primaryCount += trumpCount;
                }
                else
                {
                    secondaryCount += trumpCount;
                }
            }

            int requiredPrimaryCount = (hand.Count() == 3) ? 3 : 4;

            if (primaryCount < requiredPrimaryCount)
            {
                return false;
            }

            int remainingSlots = hand.Count() - primaryCount;
            int requiredSecondaryCount = Math.Max(remainingSlots, 0);

            bool isValid = secondaryCount >= requiredSecondaryCount;

            // Only warn if we expected a Full House but didn't get one
            if (!isValid)
            {
                Debug.LogWarning($"Not enough secondary rank cards for Full House. Primary Rank: {primaryRank}, Required: {requiredPrimaryCount}, Actual: {primaryCount}. Secondary Rank: {secondaryRank}, Required: {requiredSecondaryCount}, Actual: {secondaryCount}. Hand: {string.Join(", ", hand.Select(c => c.ToString()))}");
            }

            return isValid;
        }


        /// <summary>
        /// Determines if the hand has N-of-a-Kind including trump cards.
        /// </summary>
        public static bool IsNOfAKindOfTrump(this Hand hand, Card trumpCard, int numberOfCards)
        {
            if (hand?.GetCards() == null || trumpCard == null || trumpCard.Rank == Rank.None)
            {
                return false;
            }

            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();

            bool hasTrumpCount = rankCounts.TryGetValue(trumpCard.Rank, out int trumpCount);
            return hasTrumpCount && trumpCount == numberOfCards;
        }

        /// <summary>
        /// Determines if the hand has N-of-a-Kind including trump cards by rank.
        /// </summary>
        public static bool IsNOfAKindOfTrump(this Hand hand, Rank trumpRank, int numberOfCards)
        {
            if (hand?.GetCards() == null || trumpRank == Rank.None)
            {
                return false;
            }

            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();

            bool hasTrumpCount = rankCounts.TryGetValue(trumpRank, out int trumpCount);
            return hasTrumpCount && trumpCount == numberOfCards;
        }

        /// <summary>
        /// Finds the rank with the highest N-of-a-Kind in the hand.
        /// </summary>
        public static Rank TryGetHighestNOfKindRank(this Hand hand, int numberOfCards)
        {
            Dictionary<Rank, int> rankCounts = hand.GetRankCounts();

            Rank highestRank = null;

            foreach (KeyValuePair<Rank, int> kvp in rankCounts)
            {
                if (kvp.Value >= numberOfCards && (highestRank == null || kvp.Key.Value > highestRank.Value))
                {
                    highestRank = kvp.Key;
                }
            }

            return highestRank;
        }

        /// <summary>
        /// Generates a hand with N-of-a-Kind cards.
        /// </summary>
        public static Hand GetNOfAKindHand(int numberOfCards, Rank rank)
        {
            Suit[] availableSuits = CardUtility.GetAvailableSuits();
            List<Card> cards = new List<Card>();
            for (int i = 0; i < numberOfCards; i++)
            {
                Suit suit = CardUtility.GetAndRemoveRandomSuit(availableSuits.ToList());
                cards.Add(CardUtility.GetCard(suit, rank));
            }
            return new Hand(cards.ToArray());
        }

        /// <summary>
        /// Determines if the hand contains a pair.
        /// </summary>
        public static bool IsPair(this Hand hand, Card trumpCard, bool useTrump, out List<Rank> pairRanks)
        {
            pairRanks = new List<Rank>();

            if (useTrump && trumpCard != null && hand.Contains(trumpCard))
            {
                return false;
            }

            if (hand?.GetCards() == null || hand.Count() is < 3 or > 9)
            {
                return false;
            }

            if (hand.IsSequence())
            {
                return false;
            }

            Dictionary<Rank, int> rankCounts = new Dictionary<Rank, int>();
            for (int i = 0; i < hand.GetCards().Length; i++)
            {
                Rank rank = hand.GetCards()[i].Rank;
                rankCounts.TryAdd(rank, 0);
                rankCounts[rank]++;
            }

            foreach (KeyValuePair<Rank, int> kvp in rankCounts)
            {
                if (kvp.Value == 2)
                {
                    pairRanks.Add(kvp.Key);
                }
                else if (kvp.Value > 2)
                {
                    return false;
                }
            }

            return pairRanks.Count > 0;
        }

        /// <summary>
        /// Finds the rank of the Three-of-a-Kind in the hand.
        /// </summary>
        public static Rank GetThreeOfAKindRank(this Hand hand, Card trumpCard, bool useTrump)
        {
            Rank[] ranksToExclude = useTrump && trumpCard != null ? new[] { Rank.A, trumpCard.Rank } : new[] { Rank.A };
            Dictionary<Rank, int> rankCounts = hand.GetRankCounts(ranksToExclude);
            int trumpCount = 0;
            if (useTrump && trumpCard != null)
            {
                for (int i = 0; i < hand.GetCards().Length; i++)
                {
                    if (hand.GetCards()[i] == trumpCard)
                    {
                        trumpCount++;
                    }
                }
            }

            foreach (KeyValuePair<Rank, int> kvp in rankCounts)
            {
                if (kvp.Value == 3)
                {
                    return kvp.Key;
                }
                else if (kvp.Value == 2 && trumpCount == 1)
                {
                    return kvp.Key;
                }
            }

            return Rank.None;
        }

        /// <summary>
        /// Determines if the hand is a Four-of-a-Kind.
        /// </summary>
        public static bool IsFourOfAKind(this Hand hand, Card trumpCard, bool useTrump)
        {
            if (hand?.GetCards() == null || hand.Count() != 4)
            {
                return false;
            }

            Rank fourOfAKindRank = hand.GetFourOfAKindRank(trumpCard, useTrump);
            return fourOfAKindRank != Rank.None;
        }

        /// <summary>
        /// Finds the rank of the Four-of-a-Kind in the hand.
        /// </summary>
        public static Rank GetFourOfAKindRank(this Hand hand, Card trumpCard, bool useTrump)
        {
            Rank[] ranksToExclude = useTrump && trumpCard != null ? new[] { Rank.A, trumpCard.Rank } : new[] { Rank.A };
            Dictionary<Rank, int> rankCounts = hand.GetRankCounts(ranksToExclude);
            int trumpCount = 0;
            if (useTrump && trumpCard != null)
            {
                for (int i = 0; i < hand.GetCards().Length; i++)
                {
                    if (hand.GetCards()[i] == trumpCard)
                    {
                        trumpCount++;
                    }
                }
            }

            foreach (KeyValuePair<Rank, int> kvp in rankCounts)
            {
                if (kvp.Value == 4)
                {
                    return kvp.Key;
                }
                else if (kvp.Value == 3 && trumpCount == 1)
                {
                    return kvp.Key;
                }
            }

            return Rank.None;
        }
    }
}
