using System;

namespace OcentraAI.LLMGames.LLMServices
{
    [Serializable]
    public class LLMConfig: ILLMConfig
    {
        public string ApiKey { get; set; }
        public string ApiKey2 { get; set; }
        public string ApiUrl { get; set; }
        public string Endpoint { get; set; }
        public int MaxTokens { get; set; } = 150000;
        public string Model { get; set; }
        public string ProviderName { get; set; }
        public bool Stream { get; set; } = false;
        public double Temperature { get; set; } = 0.5;
    }
}