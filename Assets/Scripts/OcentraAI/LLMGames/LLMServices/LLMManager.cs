using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Manager.Authentication;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager.LLMServices
{
    public class AIModelManager : SingletonManagerBase<AIModelManager>
    {
        public LLMProvider CurrentProvider = LLMProvider.AzureOpenAI;
        private ILLMService CurrentLLMService { get; set; }


        public override async UniTask InitializeAsync()
        {
            if (Application.isPlaying)
            {
                try
                {
                    UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>> completionSource = new UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>>();

                    await EventBus.Instance.PublishAsync(new WaitForInitializationEvent(completionSource, GetType(), typeof(IUnityServicesManager), 10));
                    IOperationResult<IMonoBehaviourBase> operationResult = await completionSource.Task;

                    if (operationResult.IsSuccess )
                    {
                        SetLLMProvider(CurrentProvider);
                    }
                   
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
            SetLLMProvider(provider.ToString()).Forget();
        }

        public async UniTask SetLLMProvider(string provider)
        {

            UniTaskCompletionSource<IConfigManager> completionSource = new UniTaskCompletionSource<IConfigManager>();
            await EventBus.Instance.PublishAsync(new RequestConfigManagerEvent<UnityServicesManager>(completionSource));
            IConfigManager llmConfig = await completionSource.Task;

            if (llmConfig != null)
            {
                if (llmConfig.TryGetConfigForProvider(provider, out ILLMConfig config))
                {
                    CurrentLLMService = LLMServiceFactory.CreateLLMService(config as LLMConfig);
                    // Debug.Log($"LLM Service initialized for provider {provider}");
                }
                else
                {
                    Debug.LogError($"Configuration for provider {provider} not found or is invalid!");
                }
            }


        }




        public async UniTask<string> GetLLMResponse(GameMode gameMode,ulong playerID)
        {
            (string systemMessage, string userPrompt) = AIHelper.Instance.GetAIInstructions(gameMode,playerID);
            if (CurrentLLMService == null)
            {
                Debug.LogError("LLM Service is not initialized!");
                return null;
            }

            return await CurrentLLMService.GetResponseAsync(systemMessage, userPrompt);
        }
    }
}