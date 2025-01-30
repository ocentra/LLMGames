using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class LLMSourceType : IEquatable<LLMSourceType>, ILabeledItem
    {
        public static readonly LLMSourceType None = new LLMSourceType(0, nameof(None));
        public static readonly LLMSourceType LocalEmbedded = new LLMSourceType(1, nameof(LocalEmbedded));
        public static readonly LLMSourceType LocalAPI = new LLMSourceType(2, nameof(LocalAPI));
        public static readonly LLMSourceType RemoteAPI = new LLMSourceType(3, nameof(RemoteAPI));
        public static readonly LLMSourceType CloudService = new LLMSourceType(4, nameof(CloudService));

        [ShowInInspector, ReadOnly] private int id;
        [ShowInInspector, ReadOnly] private string name;

        private LLMSourceType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int Id => id;
        public string Name => name;

        public static LLMSourceType[] GetAll()
        {
            FieldInfo[] fields = typeof(LLMSourceType).GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly
            );

            List<LLMSourceType> values = new List<LLMSourceType>();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(LLMSourceType))
                {
                    values.Add((LLMSourceType)field.GetValue(null));
                }
            }
            return values.ToArray();
        }

        public static LLMSourceType FromId(int id)
        {
            foreach (LLMSourceType source in GetAll())
            {
                if (source.Id == id)
                {
                    return source;
                }
            }
            Debug.LogWarning($"LLMSourceType with ID {id} not found. Returning None.");
            return None;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LLMSourceType);
        }

        public bool Equals(LLMSourceType other)
        {
            return other != null && id == other.id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(LLMSourceType left, LLMSourceType right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(LLMSourceType left, LLMSourceType right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return name;
        }
    }
}