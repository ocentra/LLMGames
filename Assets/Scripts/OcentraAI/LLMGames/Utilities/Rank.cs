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
        // Backing fields for serialized properties
        [SerializeField,HideInInspector] private int value;
        [SerializeField, HideInInspector] private string name;
        [SerializeField, HideInInspector] private string alias;

        // Public read-only properties to expose the backing fields
        [ShowInInspector]
        public int Value => value;

        [ShowInInspector]
        public string Name => name;

        [ShowInInspector]
        public string Alias => alias;

        // Static readonly fields for predefined ranks
        public static readonly Rank None = new Rank(0, "None", "none");
        public static readonly Rank Two = new Rank(2, "2", "2");
        public static readonly Rank Three = new Rank(3, "3", "3");
        public static readonly Rank Four = new Rank(4, "4", "4");
        public static readonly Rank Five = new Rank(5, "5", "5");
        public static readonly Rank Six = new Rank(6, "6", "6");
        public static readonly Rank Seven = new Rank(7, "7", "7");
        public static readonly Rank Eight = new Rank(8, "8", "8");
        public static readonly Rank Nine = new Rank(9, "9", "9");
        public static readonly Rank Ten = new Rank(10, "10", "10");
        public static readonly Rank J = new Rank(11, "J", "jack");
        public static readonly Rank Q = new Rank(12, "Q", "queen");
        public static readonly Rank K = new Rank(13, "K", "king");
        public static readonly Rank A = new Rank(14, "A", "ace");

        // Constructor
        public Rank(int value, string name, string alias)
        {
            this.value = value;
            this.name = name;
            this.alias = alias;

        }

        // Static list to store all ranks
        private static List<Rank> standardRanks;

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

        private static List<Rank> InitializeAllRanks()
        {
            List<Rank> ranks = new List<Rank>();
            FieldInfo[] fields = typeof(Rank).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

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

        public int CompareTo(Rank other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return value.CompareTo(other.value);
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

    }
}





