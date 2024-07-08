using Sirenix.OdinInspector;

namespace OcentraAI.LLMGames.LLMServices
{
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