using Newtonsoft.Json;
using OcentraAI.LLMGames.LLMServices;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace OcentraAI.LLMGames.Authentication
{
    [Serializable]
    public class ConfigManager
    {
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

        [ShowInInspector]
        public Dictionary<string, LLMConfig> DefaultLLMConfigs { get; set; } = new Dictionary<string, LLMConfig>();

        [ShowInInspector]
        private Dictionary<string, LLMConfig> UserLLMConfigs { get; set; } = new Dictionary<string, LLMConfig>();


        public async Task FetchConfig()
        {
            UserAttributes userAttr = new UserAttributes
            {
                UserId = AuthenticationService.Instance.PlayerId,
            };

            AppAttributes appAttr = new AppAttributes
            {
                AppVersion = Application.version,
                Platform = Application.platform.ToString()
            };

            
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
            await RemoteConfigService.Instance.FetchConfigsAsync(userAttr, appAttr);

            await TryGetAllConfigsFromCloud();
        }


        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {

            //switch (configResponse.requestOrigin)
            //{
            //    case ConfigOrigin.Default:
            //        Debug.Log("No settings loaded this session; using default values.");
            //        break;
            //    case ConfigOrigin.Cached:
            //        Debug.Log("No settings loaded this session; using cached values from a previous session.");
            //        break;
            //    case ConfigOrigin.Remote:
            //        Debug.Log("New settings loaded this session; update values accordingly.");
            //        break;
            //}

            // Log all available keys and values
            //var allKeys = RemoteConfigService.Instance.appConfig.GetKeys();
            //foreach (var key in allKeys)
            //{
            //    var value = RemoteConfigService.Instance.appConfig.GetJson(key);
            //    Debug.Log($"Key: {key}, Value: {value}");
            //}


            string configJson = RemoteConfigService.Instance.appConfig.GetJson(nameof(LLMConfig));

            if (!string.IsNullOrEmpty(configJson))
            {
                try
                {
                    var llmConfigDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(configJson);

                    foreach (var providerConfig in llmConfigDictionary)
                    {
                        var providerConfigJson = JsonConvert.SerializeObject(providerConfig.Value);
                        LLMConfig config = JsonConvert.DeserializeObject<LLMConfig>(providerConfigJson);

                        string providerName = config.ProviderName.ToLower();

                        if (Enum.TryParse(typeof(LLMProvider), providerName, true, out var enumValue))
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

        public async Task AddNewConfig(string provider, string endpoint, string apiKey, string apiUrl, string model, int maxTokens, double temperature, bool stream)
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

            if (!LLMManager.Instance.ValidateConfig(newConfig))
            {
                return;
            }

            if (!UserLLMConfigs.TryAdd(provider, newConfig))
            {
                UpdateApiKey(provider, apiKey);
            }


            await SaveAllConfigsToCloud();
            LLMManager.Instance.SetLLMProvider(newConfig);
        }
        public async Task SaveAllConfigsToCloud()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(UserLLMConfigs);
                Dictionary<string, object> data = new Dictionary<string, object> { { nameof(UserLLMConfigs), jsonData } };
                await UnityServicesManager.Instance.CloudSaveService.Data.Player.SaveAsync(data);
                Debug.Log("All configurations saved to the cloud.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving all configurations: {ex.Message}");
            }
        }

        public async Task<bool> TryGetAllConfigsFromCloud()
        {
           // Debug.Log("Attempting to retrieve all configurations from the cloud...");

            while (!AuthenticationManager.Instance.IsLoggedIn)
            {
               // Debug.Log("Waiting for user to log in...");
                await Task.Delay(100);
            }

            try
            {
                Dictionary<string, Unity.Services.CloudSave.Models.Item> data = await UnityServicesManager.Instance.CloudSaveService.Data.Player.LoadAsync(new HashSet<string> { nameof(UserLLMConfigs) });
                if (data.TryGetValue(nameof(UserLLMConfigs), out Unity.Services.CloudSave.Models.Item keyValue))
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
    }
}
