using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ThreeCardBrag.LLMService
{
    [CreateAssetMenu(fileName = "LLMConfiguration", menuName = "ThreeCardBrag/LLMConfiguration")]
    [GlobalConfig("Assets/Resources/")]
    public class LLMConfiguration : GlobalConfig<LLMConfiguration>
    {
        [ShowInInspector]
        public Dictionary<LLMProvider, LLMConfig> LLMProviders = new Dictionary<LLMProvider, LLMConfig>();

        [ShowInInspector]
        public int MaxTokens = 150;

        [Button]
        private void PopulateDefault()
        {
            foreach (LLMProvider provider in System.Enum.GetValues(typeof(LLMProvider)))
            {
                if (!LLMProviders.ContainsKey(provider))
                {
                    LLMProviders.Add(provider, new LLMConfig());
                }
            }
        }

        public LLMConfig GetConfig(LLMProvider provider)
        {
            return LLMProviders.GetValueOrDefault(provider);
        }
    }
}