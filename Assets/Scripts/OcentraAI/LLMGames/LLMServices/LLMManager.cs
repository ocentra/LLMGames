using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using UnityEngine;

namespace OcentraAI.LLMGames.LLMServices
{
    public class LLMManager : MonoBehaviour
    {
        public static LLMManager Instance { get; private set; }

        public LLMProvider CurrentProvider = LLMProvider.AzureOpenAI;
        private ILLMService CurrentLLMService { get; set; }

        async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            await WaitForInitialization();
        }

        private async Task WaitForInitialization()
        {
            if (UnityServicesManager.Instance != null)
            {
                while (!UnityServicesManager.Instance.IsInitialized)
                {
                    await Task.Delay(100);
                }

                SetLLMProvider(CurrentProvider);
            }
            else
            {
                Debug.Log($"UnityServicesManager is missing!");
            }
        }

        public void SetLLMProvider(LLMProvider provider)
        {
            CurrentProvider = provider;
            SetLLMProvider(provider.ToString());
        }

        public void SetLLMProvider(string provider)
        {
            if (UnityServicesManager.Instance.ConfigManager.TryGetConfigForProvider(provider, out LLMConfig config)
                && ValidateConfig(config))
            {
                CurrentLLMService = LLMServiceFactory.CreateLLMService(config);
                Debug.Log($"LLM Service initialized for provider {provider}");
            }
            else
            {
                Debug.LogError($"Configuration for provider {provider} not found or is invalid!");
            }
        }

        public void SetLLMProvider(LLMConfig config)
        {
            if (ValidateConfig(config))
            {
                CurrentLLMService = LLMServiceFactory.CreateLLMService(config);
                Debug.Log($"LLM Service initialized with direct config");
            }
        }

        public bool ValidateConfig(LLMConfig config)
        {
            bool isValid = true;
            string errorMessage = "LLMConfig contains null or empty fields: ";

            if (string.IsNullOrEmpty(config.Endpoint))
            {
                errorMessage += "Endpoint ";
                isValid = false;
            }
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                errorMessage += "ApiKey ";
                isValid = false;
            }
            if (string.IsNullOrEmpty(config.ApiUrl))
            {
                errorMessage += "ApiUrl ";
                isValid = false;
            }
            if (string.IsNullOrEmpty(config.Model))
            {
                errorMessage += "Model ";
                isValid = false;
            }

            if (!isValid)
            {
                Debug.LogError(errorMessage);
            }

            return isValid;
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
