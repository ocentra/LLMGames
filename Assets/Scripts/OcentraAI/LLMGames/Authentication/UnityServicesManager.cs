using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

namespace OcentraAI.LLMGames.Authentication
{
    public class UnityServicesManager : MonoBehaviour
    {
        public IAnalyticsService AnalyticsService => Unity.Services.Analytics.AnalyticsService.Instance;
        public ICloudSaveService CloudSaveService => Unity.Services.CloudSave.CloudSaveService.Instance;
        public static UnityServicesManager Instance { get; private set; }

        [ShowInInspector]
        public ConfigManager ConfigManager { get; private set; }

        public bool IsInitialized { get; private set; } = false;

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
                await InitializeUnityServices();
            }
        }


        private async Task InitializeUnityServices()
        {
            try
            {
                InitializationOptions options = new InitializationOptions();
                options.SetOption("com.unity.services.core.environment-name", "production");
                await UnityServices.InitializeAsync(options);
                AnalyticsService.StartDataCollection();
                ConfigManager = new ConfigManager();
                await ConfigManager.FetchConfig();
                IsInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task SavePlayerDataToCloud(string key, PlayerData playerData)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(playerData);
                Dictionary<string, object> data = new Dictionary<string, object> { { key, jsonData } };
                await CloudSaveService.Data.Player.SaveAsync(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving player data: {ex.Message}");

            }

        }

        public async Task SavePlayerDataToCloud(PlayerData playerData)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(playerData);
                Dictionary<string, object> data = new Dictionary<string, object> { { playerData.PlayerID, jsonData } };
                await CloudSaveService.Data.Player.SaveAsync(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving player data: {ex.Message}");

            }

        }

        public async Task<(bool success, PlayerData playerData)> TryGetPlayerDataFromCloud(string key)
        {
            try
            {
                Dictionary<string, Unity.Services.CloudSave.Models.Item> data = await CloudSaveService.Data.Player.LoadAsync(new HashSet<string> { AuthenticationService.Instance.PlayerId });
                if (data.TryGetValue(key, out Unity.Services.CloudSave.Models.Item keyValue))
                {
                    PlayerData playerData = JsonUtility.FromJson<PlayerData>(keyValue.Value.GetAsString());
                    return (true, playerData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading player data: {ex.Message}");
            }

            return (false, null);
        }



        public async Task<(bool, string)> TryGetPlayerName(string key)
        {
            try
            {
                (bool success, PlayerData playerData) = await TryGetPlayerDataFromCloud(key);
                if (success)
                {
                    if (string.IsNullOrEmpty(playerData.PlayerName))
                    {
                        return (true, playerData.PlayerName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error Getting player Name: {e.Message}");

            }



            return (false, null);
        }

        public async Task<(bool, string)> TryGetPlayerEmail(string key)
        {
            try
            {
                (bool success, PlayerData playerData) = await TryGetPlayerDataFromCloud(key);
                if (success)
                {
                    if (string.IsNullOrEmpty(playerData.Email))
                    {
                        return (true, playerData.PlayerName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error Getting player Email: {e.Message}");
            }


            return (false, null);
        }

        public IEnumerator UpdatePlayerDataOnQuit(PlayerData playerData)
        {
            if (playerData != null)
            {


                Task saveTask = SavePlayerDataToCloud(playerData);

                while (!saveTask.IsCompleted)
                {
                    yield return null;
                }

                if (saveTask.IsFaulted)
                {
                    Debug.LogError($"Error occurred while saving player data: {saveTask.Exception} ");
                }
                else
                {
                    Debug.Log("Player data saved successfully.");
                }


                AuthenticationService.Instance.SignOut();
                Application.Quit();
            }
        }

        public void OnApplicationWantsToQuit(PlayerData playerData)
        {
            // StartCoroutine(UpdatePlayerDataOnQuit(playerData));

        }
    }
}