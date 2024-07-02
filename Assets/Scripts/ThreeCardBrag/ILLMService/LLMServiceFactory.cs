using System;

namespace ThreeCardBrag.LLMService
{
    public static class LLMServiceFactory
    {
        public static ILLMService CreateLLMService(LLMConfiguration config, LLMProvider provider)
        {
            LLMConfig llmConfig = config.GetConfig(provider);
            if (llmConfig == null)
            {
                throw new ArgumentException($"Configuration for provider {provider} not found.");
            }

            switch (provider)
            {
                case LLMProvider.AzureOpenAI:
                    return new AzureOpenAIService(llmConfig);
                case LLMProvider.OpenAI:
                    return new OpenAIService(llmConfig);
                case LLMProvider.Claude:
                    return new ClaudeService(llmConfig);
                case LLMProvider.LocalLLM:
                    return new LocalLLMService(llmConfig);
                default:
                    throw new ArgumentException("Invalid LLM provider");
            }
        }
    }
}