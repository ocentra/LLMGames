using System;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public class LLMConfig
    {
        public string ApiKey;
        public string ApiKey2;
        public string ApiUrl;
        public string Endpoint;
        public int MaxTokens = 150000;
        public string Model;
        public string ProviderName;
        public bool Stream = false;
        public double Temperature = 0.5;
    }
}