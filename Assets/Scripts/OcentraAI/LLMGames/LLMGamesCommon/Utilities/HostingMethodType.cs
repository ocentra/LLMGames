using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class HostingMethodType : IEquatable<HostingMethodType>, ILabeledItem
    {
        public static readonly HostingMethodType None = new HostingMethodType(0, nameof(None));
        public static readonly HostingMethodType DedicatedServer = new HostingMethodType(1, nameof(DedicatedServer));
        public static readonly HostingMethodType PlayerHosted = new HostingMethodType(2, nameof(PlayerHosted));
        public static readonly HostingMethodType CloudManaged = new HostingMethodType(3, nameof(CloudManaged));

        [ShowInInspector, ReadOnly] private int id;
        [ShowInInspector, ReadOnly] private string name;

        private HostingMethodType(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int Id => id;
        public string Name => name;

        public static HostingMethodType[] GetAll()
        {
            FieldInfo[] fields = typeof(HostingMethodType).GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly
            );

            List<HostingMethodType> values = new List<HostingMethodType>();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(HostingMethodType))
                {
                    values.Add((HostingMethodType)field.GetValue(null));
                }
            }
            return values.ToArray();
        }

        public static HostingMethodType FromId(int id)
        {
            foreach (HostingMethodType method in GetAll())
            {
                if (method.Id == id)
                {
                    return method;
                }
            }
            Debug.LogWarning($"HostingMethodType with ID {id} not found. Returning None.");
            return None;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HostingMethodType);
        }

        public bool Equals(HostingMethodType other)
        {
            return other != null && id == other.id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(HostingMethodType left, HostingMethodType right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(HostingMethodType left, HostingMethodType right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return name;
        }
    }
}