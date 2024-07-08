using System;
using UnityEngine;

namespace ThreeCardBrag.LLMService
{
    public static class LLMServiceFactory
    {
        public static ILLMService CreateLLMService(LLMConfig config)
        {
            
            switch (LLMManager.Instance.CurrentProvider)
            {
                case LLMProvider.AzureOpenAI:
                    return new AzureOpenAIService(config);
                case LLMProvider.OpenAI:
                    return new OpenAIService(config);
                case LLMProvider.Claude:
                    return new ClaudeService(config);
                case LLMProvider.LocalLLM:
                    return new LocalLLMService(config);
                default:
                    Debug.LogError("Invalid LLM provider");
                    return new AzureOpenAIService(config);
                
                
            }
        }
    }
}