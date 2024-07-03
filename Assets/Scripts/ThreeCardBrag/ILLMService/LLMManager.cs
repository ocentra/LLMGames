using System.Threading.Tasks;
using UnityEngine;

namespace ThreeCardBrag.LLMService
{
    public class LLMManager : MonoBehaviour
    {
        public static LLMManager Instance { get; private set; }

        public LLMProvider CurrentProvider = LLMProvider.AzureOpenAI;
        private ILLMService CurrentLLMService { get;  set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);  
            }
            else
            {
                Destroy(gameObject);
            }

            SetLLMProvider(CurrentProvider);  
        }

        public void SetLLMProvider(LLMProvider provider)
        {
            CurrentProvider = provider;
            LLMConfig config = LLMConfiguration.Instance.GetConfig(provider);
            if (config != null && ValidateConfig(config))
            {
                CurrentLLMService = LLMServiceFactory.CreateLLMService(LLMConfiguration.Instance, provider);
                Debug.Log($"LLM Service initialized for provider {provider}");
            }
            else
            {
                Debug.LogError($"Configuration for provider {provider} not found!");
            }
        }

        private bool ValidateConfig(LLMConfig config)
        {
            if (string.IsNullOrEmpty(config.Endpoint) || string.IsNullOrEmpty(config.ApiKey) ||
                string.IsNullOrEmpty(config.ApiUrl) || string.IsNullOrEmpty(config.Model))
            {
                Debug.LogError("LLMConfig contains null or empty fields!");
                return false;
            }
            return true;
        }

        public async Task<string> GetLLMResponse()
        {
            var (systemMessage, userPrompt) = GameManager.Instance.AIHelper.GetAIInstructions();
            if (CurrentLLMService == null)
            {
                Debug.LogError("LLM Service is not initialized!");
                return null;
            }

            return await CurrentLLMService.GetResponseAsync(systemMessage, userPrompt);
        }
    }
}