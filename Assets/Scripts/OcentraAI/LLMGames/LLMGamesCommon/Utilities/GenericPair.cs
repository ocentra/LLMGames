using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Commons
{
    [Serializable]
    public class GenericPair<TKey, TValue> : IEquatable<GenericPair<TKey, TValue>>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public readonly Guid ID = Guid.NewGuid();

        public GenericPair() { }

        public GenericPair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public bool Equals(GenericPair<TKey, TValue> other)
        {
            if (other == null) return false;

            return EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
                   EqualityComparer<TValue>.Default.Equals(Value, other.Value) &&
                   ID == other.ID; // Compare ID to ensure uniqueness.
        }

        public override bool Equals(object obj)
        {
            return obj is GenericPair<TKey, TValue> pair && Equals(pair);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"Key: {Key}, Value: {Value}, ID: {ID}";
        }
    }
}