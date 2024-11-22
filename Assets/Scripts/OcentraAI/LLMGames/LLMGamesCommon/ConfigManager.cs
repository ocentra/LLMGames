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
    public class ConfigManager
    {
        [ShowInInspector]
        public Dictionary<string, LLMConfig> DefaultLLMConfigs { get; set; } = new Dictionary<string, LLMConfig>();

        [ShowInInspector]
        private Dictionary<string, LLMConfig> UserLLMConfigs { get; set; } = new Dictionary<string, LLMConfig>();


        public async UniTask FetchConfig()
        {
            UserAttributes userAttr = new UserAttributes {UserId = AuthenticationService.Instance.PlayerId};

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

                        if (Enum.TryParse(typeof(LLMProvider), providerName, true, out object enumValue))
                        {
                            DefaultLLMConfigs[enumValue.ToString()] = config;
                            // Debug.Log($"Configuration Loaded for {enumValue}: {providerConfigJson}");
                        }
                        else
                        {
                            Debug.LogWarning($"Provider {providerName} is not a valid LLMProvider enum value.");
                        }
                    }

                    // Debug.Log("LLM configurations loaded successfully.");
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


        public bool TryGetConfigForProvider(string provider, out LLMConfig config)
        {
            if (UserLLMConfigs.TryGetValue(provider, out LLMConfig foundUserConfig))
            {
                config = foundUserConfig;
                return true;
            }

            if (DefaultLLMConfigs.TryGetValue(provider, out LLMConfig foundDefaultConfig))
            {
                config = foundDefaultConfig;
                return true;
            }

            config = null;
            Debug.LogError($"Configuration for provider {provider} not found.");
            return false;
        }

        public void UpdateApiKey(string provider, string newApiKey)
        {
            if (UserLLMConfigs.TryGetValue(provider, out LLMConfig userLLMConfig))
            {
                userLLMConfig.ApiKey = newApiKey;
                UserLLMConfigs[provider] = userLLMConfig;
                Debug.Log($"API key updated for {provider}");
            }
        }

        public async UniTask<(bool, LLMConfig)> TryAddOrUpdateConfig(string provider, string endpoint, string apiKey, string apiUrl, string model,
            int maxTokens, double temperature, bool stream)
        {
            LLMConfig newConfig = new LLMConfig
            {
                Endpoint = endpoint,
                ApiKey = apiKey,
                ApiUrl = apiUrl,
                Model = model,
                MaxTokens = maxTokens,
                Temperature = temperature,
                Stream = stream
            };

            try
            {
                if (ValidateConfig(newConfig))
                {
                    bool isAddedOrUpdated = false;
                    UpdateApiKey(provider, apiKey);

                    if (UserLLMConfigs.ContainsKey(provider))
                    {
                        UserLLMConfigs[provider] = newConfig;
                        isAddedOrUpdated = true;
                    }
                    else
                    {
                        if (UserLLMConfigs.TryAdd(provider, newConfig))
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
                Debug.LogError($"Error adding or updating config for provider '{provider}': {ex.Message}\nStack Trace: {ex.StackTrace}");
                return (false, null);
            }

            return (false, null);
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
                Debug.Log("Waiting for user to log in...");
                await UniTask.Delay(100);
            }

            try
            {
                Dictionary<string, Item> data = await CloudSaveService.Instance.Data.Player
                    .LoadAsync(new HashSet<string> {nameof(UserLLMConfigs)}).AsUniTask();
                if (data.TryGetValue(nameof(UserLLMConfigs), out Item keyValue))
                {
                    Dictionary<string, LLMConfig> configs = JsonUtility.FromJson<Dictionary<string, LLMConfig>>(keyValue.Value.GetAsString());
                    UserLLMConfigs = configs ?? new Dictionary<string, LLMConfig>();
                    // Debug.Log("All configurations retrieved from the cloud.");
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