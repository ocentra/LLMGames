namespace ThreeCardBrag.LLMService
{
    [System.Serializable]
    public class LLMConfig
    {
        public string Endpoint;
        public string ApiKey;
        public string ApiUrl;
        public string Model;
        public int MaxTokens;
        public double Temperature;
        public bool Stream;
    }
}