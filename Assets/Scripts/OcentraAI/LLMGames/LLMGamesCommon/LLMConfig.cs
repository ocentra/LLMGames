using Sirenix.OdinInspector;
using System;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public class LLMConfig : ILLMConfig
    {
        public string ApiKey { get; set; }
        public string ApiKey2 { get; set; }
        public string ApiUrl { get; set; }
        public string Endpoint { get; set; }
        public int MaxTokens { get; set; } = 150000;
        public string Model { get; set; }
        public ILLMProvider Provider { get; set; }
        public bool Stream { get; set; } = false;
        public double Temperature { get; set; } = 0.5;
        public string ProviderName { get; set; }

        public bool TrySetProvider(string providerName)
        {
            Provider = LLMProvider.None;
            foreach (ILLMProvider provider in LLMProvider.GetAllProvidersStatic())
            {
                if (string.Equals(provider.Name, providerName, StringComparison.OrdinalIgnoreCase))
                {
                    Provider = provider;
                    return true;

                }
            }

            return false;
        }
    }

}