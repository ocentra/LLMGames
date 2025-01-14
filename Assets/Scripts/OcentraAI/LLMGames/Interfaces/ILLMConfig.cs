namespace OcentraAI.LLMGames
{
    public interface ILLMConfig
    {
        public string ApiKey { get; set; }
        public string ApiKey2 { get; set; }
        public string ApiUrl { get; set; }
        public string Endpoint { get; set; }
        public int MaxTokens { get; set; }
        public string Model { get; set; }
        public ILLMProvider Provider { get; set; }
        public bool Stream { get; set; }
        public double Temperature { get; set; }
        public string ProviderName { get; set; }
        bool TrySetProvider(string providerName);
    }
}