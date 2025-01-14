using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OcentraAI.LLMGames.LLMServices;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager.Authentication
{
    [Serializable]
    public class ConfigManager: IConfigManager
    {
        [ShowInInspector,DictionaryDrawerSettings]
        public Dictionary<ILLMProvider, LLMConfig> DefaultLLMConfigs { get; set; } = new Dictionary<ILLMProvider, LLMConfig>();

        [ShowInInspector, DictionaryDrawerSettings]
        private Dictionary<ILLMProvider, LLMConfig> UserLLMConfigs { get; set; } = new Dictionary<ILLMProvider, LLMConfig>();


        public async UniTask FetchConfig(string playerId)
        {
            UserAttributes userAttr = new UserAttributes {UserId = playerId};

            AppAttributes appAttr = new AppAttributes
            {
                AppVersion = Application.version, Platform = Application.platform.ToString()
            };
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;


            await RemoteConfigService.Instance.FetchConfigsAsync(userAttr, appAttr).AsUniTask();

            await TryGetAllConfigsFromCloud();
        }


        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {


            string configJson = RemoteConfigService.Instance.appConfig.GetJson(nameof(LLMConfig));

            if (!string.IsNullOrEmpty(configJson))
            {
                try
                {
                    Dictionary<string, Dictionary<string, object>> llmConfigDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(configJson);

                    foreach (KeyValuePair<string, Dictionary<string, object>> providerConfig in llmConfigDictionary)
                    {
                        string providerConfigJson = JsonConvert.SerializeObject(providerConfig.Value);
                        LLMConfig config = JsonConvert.DeserializeObject<LLMConfig>(providerConfigJson);

                        string providerName = config.ProviderName.ToLower();

                        if (config.TrySetProvider(providerName))
                        {
                            LLMProvider configProvider = config.Provider as LLMProvider;

                            if (configProvider != null)
                            {
                                DefaultLLMConfigs[configProvider] = config;
                               


                            }
                        }

                    }

                }
                catch (JsonException jsonEx)
                {
                    Debug.LogError($"Failed to deserialize LLMConfig JSON: {jsonEx.Message}");
                }
            }
            else
            {
                Debug.LogWarning("LLMConfig configuration is missing or empty.");
            }
        }


        public bool TryGetConfigForProvider(ILLMProvider provider, out ILLMConfig config)
        {
            if (UserLLMConfigs.TryGetValue(provider, out LLMConfig foundUserConfig))
            {
                
                if (ValidateConfig(foundUserConfig))
                {
                    config = foundUserConfig;
                    return true;
                }


            }

            if (DefaultLLMConfigs.TryGetValue(provider, out LLMConfig foundDefaultConfig))
            {
                if (ValidateConfig(foundDefaultConfig))
                {
                    config = foundDefaultConfig;
                    return true;
                }
            }

            config = null;
            Debug.LogError($"Configuration for provider {provider} not found.");
            return false;
        }

        public void UpdateApiKey(ILLMProvider provider, string newApiKey)
        {
            if (UserLLMConfigs.TryGetValue(provider, out LLMConfig userLLMConfig))
            {
                userLLMConfig.ApiKey = newApiKey;
                UserLLMConfigs[provider] = userLLMConfig;
                Debug.Log($"API key updated for {provider}");
            }
        }

        public async UniTask<(bool, ILLMConfig)> TryAddOrUpdateConfig(ILLMConfig newConfig)
        {

            try
            {
                if (ValidateConfig(newConfig))
                {
                    bool isAddedOrUpdated = false;
                    UpdateApiKey(newConfig.Provider, newConfig.ApiKey);

                    if (UserLLMConfigs.ContainsKey(newConfig.Provider))
                    {
                        UserLLMConfigs[newConfig.Provider] = newConfig as LLMConfig;
                        isAddedOrUpdated = true;
                    }
                    else
                    {
                        if (UserLLMConfigs.TryAdd(newConfig.Provider, newConfig as LLMConfig))
                        {
                            isAddedOrUpdated = true;
                        }
                    }

                    if (isAddedOrUpdated)
                    {
                        await SaveAllConfigsToCloud();
                        return (true, newConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error adding or updating config for provider '{newConfig.Provider.Name}': {ex.Message}\nStack Trace: {ex.StackTrace}");
                return (false, null);
            }

            return (false, null);
        }



        public bool ValidateConfig(ILLMConfig config)
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

        public async UniTask SaveAllConfigsToCloud()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(UserLLMConfigs);
                Dictionary<string, object> data = new Dictionary<string, object> {{nameof(UserLLMConfigs), jsonData}};
                await CloudSaveService.Instance.Data.Player.SaveAsync(data).AsUniTask();
                Debug.Log("All configurations saved to the cloud.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving all configurations: {ex.Message}");
            }
        }

        public async UniTask<bool> TryGetAllConfigsFromCloud()
        {
            // Debug.Log("Attempting to retrieve all configurations from the cloud...");

            while (!AuthenticationService.Instance.IsSignedIn)
            {
                await UniTask.Delay(100);
            }

            try
            {
                Dictionary<string, Item> data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> {nameof(UserLLMConfigs)}).AsUniTask();
              
                if (data.TryGetValue(nameof(UserLLMConfigs), out Item keyValue))
                {
                    string asString = keyValue.Value.GetAsString();
                    Dictionary<string, LLMConfig> configs = JsonUtility.FromJson<Dictionary<string, LLMConfig>>(asString);


                    UserLLMConfigs ??= new Dictionary<ILLMProvider, LLMConfig>();

                    foreach (KeyValuePair<string, LLMConfig> pair in configs)
                    {
                        ILLMProvider llmProvider = LLMProvider.FromName(pair.Key);
                        UserLLMConfigs.TryAdd(llmProvider, pair.Value);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error retrieving all configurations: {ex.Message}");
            }

            return false;
        }

        [Serializable]
        public struct UserAttributes
        {
            public string UserId;
        }

        [Serializable]
        public struct AppAttributes
        {
            public string AppVersion;
            public string Platform;
        }
    }
}