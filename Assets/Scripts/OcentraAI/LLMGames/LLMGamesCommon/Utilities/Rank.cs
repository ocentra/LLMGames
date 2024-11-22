using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    [Serializable]
    public class Rank : IComparable<Rank>
    {
        // Static readonly fields for predefined ranks
        public static Rank None = new Rank(0, "None", "none");
        public static Rank Two = new Rank(2, "2", "2");
        public static Rank Three = new Rank(3, "3", "3");
        public static Rank Four = new Rank(4, "4", "4");
        public static Rank Five = new Rank(5, "5", "5");
        public static Rank Six = new Rank(6, "6", "6");
        public static Rank Seven = new Rank(7, "7", "7");
        public static Rank Eight = new Rank(8, "8", "8");
        public static Rank Nine = new Rank(9, "9", "9");
        public static Rank Ten = new Rank(10, "10", "10");
        public static Rank J = new Rank(11, "J", "jack");
        public static Rank Q = new Rank(12, "Q", "queen");
        public static Rank K = new Rank(13, "K", "king");
        public static Rank A = new Rank(14, "A", "ace");

        // Static list to store all ranks
        private static List<Rank> standardRanks;
        [SerializeField][HideInInspector] private string alias;

        [SerializeField][HideInInspector] private string name;

        // Backing fields for serialized properties
        [SerializeField][HideInInspector] private int value;

        // Constructor
        public Rank(int value, string name, string alias)
        {
            this.value = value;
            this.name = name;
            this.alias = alias;
        }

        // Public read-only properties to expose the backing fields
        [ShowInInspector] public int Value => value;

        [ShowInInspector] public string Name => name;

        [ShowInInspector] public string Alias => alias;

        // Property to access the initialized standard ranks
        private static List<Rank> StandardRanks
        {
            get
            {
                if (standardRanks is not { Count: 13 })
                {
                    standardRanks = InitializeAllRanks();
                }

                return standardRanks;
            }
        }

        public int CompareTo(Rank other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            return value.CompareTo(other.value);
        }

        private static List<Rank> InitializeAllRanks()
        {
            List<Rank> ranks = new List<Rank>();
            FieldInfo[] fields =
                typeof(Rank).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(Rank))
                {
                    Rank rank = (Rank)field.GetValue(null);
                    if (rank.Value != 0)
                    {
                        ranks.Add(rank);
                    }
                }
            }

            ranks.Sort(CompareCardRanks);
            return ranks;
        }

        private static int CompareCardRanks(Rank x, Rank y)
        {
            return x.value.CompareTo(y.value);
        }

        public static List<Rank> GetStandardRanks()
        {
            List<Rank> standardRanks = new List<Rank>();

            foreach (Rank rank in StandardRanks)
            {
                if (rank != None && rank.value >= Two.value && rank.value <= A.value)
                {
                    standardRanks.Add(rank);
                }
            }

            return standardRanks;
        }


        public static Rank CreateCustomRank(int value, string name, string alias)
        {
            foreach (Rank rank in StandardRanks)
            {
                if (rank.value == value || rank.name == name)
                {
                    return rank;
                }
            }

            Rank newRank = new Rank(value, name, alias);

            StandardRanks.Add(newRank);

            StandardRanks.Sort(CompareCardRanks);

            return newRank;
        }


        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rank other)
            {
                return value == other.value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static bool operator ==(Rank left, Rank right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Rank left, Rank right)
        {
            return !(left == right);
        }

        public static bool operator <(Rank left, Rank right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }

            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Rank left, Rank right)
        {
            if (ReferenceEquals(left, null))
            {
                return true;
            }

            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Rank left, Rank right)
        {
            return !(left <= right);
        }

        public static bool operator >=(Rank left, Rank right)
        {
            return !(left < right);
        }

        public static Rank FromValue(int value)
        {
            foreach (Rank rank in StandardRanks)
            {
                if (rank.value == value)
                {
                    return rank;
                }
            }

            return None; // Return None if no matching rank is found
        }


        public static Rank Random()
        {
            return StandardRanks[UnityEngine.Random.Range(0, StandardRanks.Count)];
        }

        public static Rank RandomBetween(Rank min, Rank max)
        {
            List<Rank> validRanks = new List<Rank>();

            foreach (Rank rank in StandardRanks)
            {
                if (rank.value >= min.value && rank.value <= max.value)
                {
                    validRanks.Add(rank);
                }
            }

            if (validRanks.Count == 0)
            {
                Debug.LogWarning($"No valid ranks found between {min.Name} and {max.Name}. Returning None.");
                return None;
            }

            int randomIndex = UnityEngine.Random.Range(0, validRanks.Count);
            return validRanks[randomIndex];
        }

        public static Rank RandomBetweenStandard()
        {
            List<Rank> standardRanks = GetStandardRanks();
            if (standardRanks.Count == 0)
            {
                Debug.LogWarning("No standard ranks available. Returning None.");
                return None;
            }

            int randomIndex = UnityEngine.Random.Range(0, standardRanks.Count);
            return standardRanks[randomIndex];
        }

        public static List<Rank> GetRanksInDescendingOrder()
        {
            List<Rank> ranks = GetStandardRanks();
            ranks.Sort((a, b) => b.CompareTo(a));
            return ranks;
        }

        public static Rank[] GetTopNRanks(int numberOfCards)
        {
            List<Rank> ranksInDescendingOrder = GetRanksInDescendingOrder();

            if (numberOfCards > ranksInDescendingOrder.Count)
            {
                numberOfCards = ranksInDescendingOrder.Count;
            }

            Rank[] result = new Rank[numberOfCards];
            ranksInDescendingOrder.CopyTo(0, result, 0, numberOfCards);

            return result;
        }
    }
}