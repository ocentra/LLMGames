using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Manager.Authentication;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager.LLMServices
{
    public class LLMManager : ManagerBase<LLMManager>
    {
        public LLMProvider CurrentProvider = LLMProvider.AzureOpenAI;
        private ILLMService CurrentLLMService { get; set; }


        protected override async UniTask InitializeAsync()
        {
            if (Application.isPlaying)
            {
                try
                {
                    await UnityServicesManager.GetInstance().WaitForInitializationAsync();

                    SetLLMProvider(CurrentProvider);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize UnityServicesManager: {ex.Message}");
                }
            }

            await base.InitializeAsync();
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
                // Debug.Log($"LLM Service initialized for provider {provider}");
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
                Debug.Log("LLM Service initialized with direct config");
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

        public async UniTask<string> GetLLMResponse()
        {
            var (systemMessage, userPrompt) = AIHelper.Instance.GetAIInstructions();
            if (CurrentLLMService == null)
            {
                Debug.LogError("LLM Service is not initialized!");
                return null;
            }

            return await CurrentLLMService.GetResponseAsync(systemMessage, userPrompt);
        }
    }
}