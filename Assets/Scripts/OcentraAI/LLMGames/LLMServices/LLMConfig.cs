namespace OcentraAI.LLMGames.LLMServices
{
    [System.Serializable]
    public class LLMConfig
    {
        public string ProviderName;
        public string Endpoint;
        public string ApiKey;
        public string ApiKey2;
        public string ApiUrl;
        public string Model;
        public int MaxTokens = 150000;
        public double Temperature =0.5;
        public bool Stream =false;
    }
}

