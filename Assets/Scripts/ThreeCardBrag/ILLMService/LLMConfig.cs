using Sirenix.OdinInspector;

namespace ThreeCardBrag.LLMService
{
    [System.Serializable]
    public class LLMConfig
    {
        public string Endpoint;
        public string ApiKey;
        public string ApiUrl;
        public string Model;
        public int MaxTokens = 150000;
        public double Temperature =0.5;
        public bool Stream =false;
    }

    [System.Serializable]
    public class LLMProviderConfig
    {
        [ShowInInspector]
        public readonly LLMProvider LLMProvider;
        public LLMConfig LLMConfig ;

        public LLMProviderConfig(LLMProvider llmProvider)
        {
            LLMProvider = llmProvider;
            LLMConfig = new LLMConfig();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            LLMProviderConfig other = (LLMProviderConfig)obj;
            return LLMProvider == other.LLMProvider;
        }

        public override int GetHashCode()
        {
            return LLMProvider.GetHashCode();
        }
    }
}

