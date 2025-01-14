using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Manager.Authentication;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager.LLMServices
{
    [CreateAssetMenu(fileName = nameof(AIModelManager), menuName = "LLMGames/AIModelManager")]
    [GlobalConfig("Assets/Resources/")]
    public class AIModelManager : ScriptableSingletonBase<AIModelManager>
    {
        [ShowInInspector] public LLMProvider DefaultProvider = LLMProvider.AzureOpenAI;
        private ILLMService CurrentLLMService { get; set; }

        protected Dictionary<LLMProvider, ILLMService> Providers = new Dictionary<LLMProvider, ILLMService>();

        [ShowInInspector, ReadOnly]
        private Dictionary<string, ScriptableObject> AllProviders { get; set; } = new Dictionary<string, ScriptableObject>();

        [ShowInInspector, ReadOnly] IConfigManager ConfigManager { get; set; }

        protected override void OnEnable()
        {
            InitializeAsync().Forget();
            base.OnEnable();
        }

        public override async UniTask InitializeAsync()
        {
            ScriptableObject[] services = Resources.LoadAll<ScriptableObject>("");

            foreach (ScriptableObject service in services)
            {
                if (service is ILLMService { Provider: not null } llmService)
                {
                    Providers.TryAdd(llmService.Provider as LLMProvider, llmService);
                    AllProviders.TryAdd(llmService.Provider.Name, service);
                }
            }
            await base.InitializeAsync();

            await UniTask.Yield();
        }

        public async UniTask UpdateProvider(IConfigManager configManager)
        {
            ConfigManager = configManager;
            foreach (LLMProvider provider in Providers.Keys)
            {
                await SetLLMProvider(provider);
            }

            if (Providers.TryGetValue(DefaultProvider, out ILLMService llmService))
            {

                CurrentLLMService = llmService;

            }



            await UniTask.Yield();
        }

        public async UniTask SetLLMProvider(LLMProvider provider)
        {
            if (ConfigManager == null)
            {
                UniTaskCompletionSource<IConfigManager> completionSource = new UniTaskCompletionSource<IConfigManager>();
                await EventBus.Instance.PublishAsync(new RequestConfigManagerEvent<UnityServicesManager>(completionSource));
                ConfigManager = await completionSource.Task;
            }


            if (ConfigManager != null)
            {
                if (ConfigManager.TryGetConfigForProvider(provider, out ILLMConfig config))
                {

                    if (Providers.TryGetValue(provider, out ILLMService llmService))
                    {
                        llmService.InitializeAsync(config);

                    }

                }
                else
                {
                    Debug.LogError($"Configuration for provider {provider} not found or is invalid!");
                }
            }


        }


        public async UniTask<string> GetLLMResponse(GameMode gameMode, ulong playerID)
        {
            (string systemMessage, string userPrompt) = AIHelper.Instance.GetAIInstructions(gameMode, playerID);
            if (CurrentLLMService == null)
            {
                Debug.LogError("LLM Service is not initialized!");
                return null;
            }

            return await CurrentLLMService.GetResponseAsync(systemMessage, userPrompt);
        }



    }
}