using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    [Serializable]
    public class LLMProvider : ILLMProvider
    {
        // Default fallback
        public static readonly LLMProvider None = new LLMProvider(0, nameof(None));

        // Providers
        public static readonly LLMProvider AzureOpenAI = new LLMProvider(1, nameof(AzureOpenAI));
        public static readonly LLMProvider OpenAI = new LLMProvider(2, nameof(OpenAI));
        public static readonly LLMProvider Claude = new LLMProvider(3, nameof(Claude));
        public static readonly LLMProvider LocalLLM = new LLMProvider(4, nameof(LocalLLM));



        [SerializeField] private int providerId;

        public int ProviderId
        {
            get => providerId;
            private set => providerId = value;
        }

        [ShowInInspector] public string Name { get; private set; }

        private LLMProvider(int providerId, string name)
        {
            this.providerId = providerId;
            Name = name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref providerId);
        }

        public static ILLMProvider FromId(int id)
        {
            foreach (ILLMProvider provider in GetAllProvidersStatic())
            {
                if (provider.ProviderId == id)
                {
                    return provider;
                }
            }
            return None;
        }

        public static ILLMProvider FromName(string providerName)
        {
            foreach (ILLMProvider provider in GetAllProvidersStatic())
            {
                if (provider.Name == providerName)
                {
                    return provider;
                }
            }
            return None;
        }

        public override bool Equals(object obj)
        {
            if (obj is ILLMProvider other)
            {
                return providerId == other.ProviderId;
            }
            return false;
        }

        public static bool operator ==(LLMProvider left, LLMProvider right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(LLMProvider left, LLMProvider right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return ProviderId.GetHashCode();
        }

        public static List<ILLMProvider> GetAllProvidersStatic()
        {
            List<ILLMProvider> providers = new List<ILLMProvider>();
            FieldInfo[] fields = typeof(LLMProvider).GetFields(BindingFlags.Public |
                                                               BindingFlags.Static |
                                                               BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(LLMProvider))
                {
                    LLMProvider provider = (LLMProvider)field.GetValue(null);
                    providers.Add(provider);
                }
            }

            return providers;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
