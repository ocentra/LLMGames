using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    [Serializable]
    public class Suit : IComparable<Suit>
    {
        // Backing fields for serialized properties
        [SerializeField, HideInInspector] private int value;
        [SerializeField, HideInInspector] private string name;
        [SerializeField, HideInInspector] private string symbol;

        // Public read-only properties to expose the backing fields
        [ShowInInspector]
        public int Value => value;

        [ShowInInspector]
        public string Name => name;

        [ShowInInspector]
        public string Symbol => symbol;

        // Static readonly fields for predefined suits
        public static readonly Suit None = new Suit(0, "None", "N");
        public static readonly Suit Heart = new Suit(1, "Heart", "♥");
        public static readonly Suit Diamond = new Suit(2, "Diamond", "♦");
        public static readonly Suit Club = new Suit(3, "Club", "♣");
        public static readonly Suit Spade = new Suit(4, "Spade", "♠");

        // Constructor
        public Suit(int value, string name, string symbol)
        {
            this.value = value;
            this.name = name;
            this.symbol = symbol;
        }

        // Lazy initialization for all suits
        private static readonly Lazy<List<Suit>> LazyAllSuits = new Lazy<List<Suit>>(InitializeAllSuits);

        private static List<Suit> StandardSuits => LazyAllSuits.Value;

        private static List<Suit> InitializeAllSuits()
        {
            List<Suit> suits = new List<Suit>();
            FieldInfo[] fields = typeof(Suit).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(Suit))
                {
                    Suit suit = (Suit)field.GetValue(null);
                    if (suit != None)
                    {
                        suits.Add(suit);
                    }
                }
            }

            suits.Sort(CompareSuits);
            return suits;
        }

        private static int CompareSuits(Suit x, Suit y)
        {
            return x.value.CompareTo(y.value);
        }



        public static Suit CreateCustomSuit(int value, string name, string symbol)
        {
            foreach (Suit suit in StandardSuits)
            {
                if (suit.value == value || suit.name == name)
                {
                    throw new ArgumentException("A suit with this value or name already exists.");
                }
            }
            return new Suit(value, name, symbol);
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj is Suit other)
            {
                return value == other.value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public int CompareTo(Suit other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return value.CompareTo(other.value);
        }

        public static bool operator ==(Suit left, Suit right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(Suit left, Suit right)
        {
            return !(left == right);
        }

        public static bool operator <(Suit left, Suit right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Suit left, Suit right)
        {
            if (ReferenceEquals(left, null))
            {
                return true;
            }
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Suit left, Suit right)
        {
            return !(left <= right);
        }

        public static bool operator >=(Suit left, Suit right)
        {
            return !(left < right);
        }

        public static Suit FromValue(int value)
        {
            foreach (Suit suit in StandardSuits)
            {
                if (suit.value == value)
                {
                    return suit;
                }
            }
            return None;
        }


        public static Suit Random()
        {
            return StandardSuits[UnityEngine.Random.Range(1, StandardSuits.Count)];
        }

        public static List<Suit> GetStandardSuits()
        {
            List<Suit> standardSuits = new List<Suit>();

            // Manually add the standard suits
            if (Heart != None) standardSuits.Add(Heart);
            if (Diamond != None) standardSuits.Add(Diamond);
            if (Club != None) standardSuits.Add(Club);
            if (Spade != None) standardSuits.Add(Spade);

            return standardSuits;
        }



        public static Suit RandomBetweenStandard()
        {
            List<Suit> standardSuits = GetStandardSuits();
            int randomIndex = UnityEngine.Random.Range(0, standardSuits.Count);
            return standardSuits[randomIndex];
        }

    }
}
