using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager.Authentication
{
    public class UnityServicesManager : ManagerBase<UnityServicesManager>
    {
        public IAnalyticsService AnalyticsService => Unity.Services.Analytics.AnalyticsService.Instance;
        public ICloudSaveService CloudSaveService => Unity.Services.CloudSave.CloudSaveService.Instance;
        [ShowInInspector] public ConfigManager ConfigManager { get; private set; }


        protected override async UniTask Initialize()
        {
            if (Application.isPlaying)
            {
                try
                {
                    InitializationOptions options = new InitializationOptions();
                    options.SetOption("com.unity.services.core.environment-name", "production");
                    await UnityServices.InitializeAsync(options).AsUniTask();
                    AnalyticsService.StartDataCollection();
                    ConfigManager = new ConfigManager();
                    await ConfigManager.FetchConfig();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }


            await base.Initialize();
        }


        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            EventRegistrar.Subscribe<AuthenticationSignOutEvent>(OnAuthenticationSignOutEvent);
            EventRegistrar.Subscribe<RequestPlayerDataFromCloudEvent>(OnRequestPlayerDataFromCloudEvent);
            EventRegistrar.Subscribe<SavePlayerDataToCloudEvent>(OnSavePlayerDataToCloudEvent);
            EventRegistrar.Subscribe<WaitForInitializationEvent<UnityServicesManager>>(OnWaitForInitialization);

        }

        private async UniTask OnWaitForInitialization(WaitForInitializationEvent<UnityServicesManager> arg)
        {
            await WaitForInitializationAsync();
            arg.CompletionSource.TrySetResult(true);
        }


        private async UniTask OnSavePlayerDataToCloudEvent(SavePlayerDataToCloudEvent arg)
        {
            await SavePlayerDataToCloud(arg.AuthPlayerData);
            await UniTask.Yield();
        }

        private async UniTask OnRequestPlayerDataFromCloudEvent(RequestPlayerDataFromCloudEvent arg)
        {
            (bool success, IAuthPlayerData playerData) = await TryGetPlayerDataFromCloud(arg.PlayerId);

            arg.PlayerDataSource.TrySetResult((success, playerData));
        }


        public async UniTask SavePlayerDataToCloud(string key, IAuthPlayerData authPlayerData)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(authPlayerData);
                Dictionary<string, object> data = new Dictionary<string, object> { { key, jsonData } };
                await CloudSaveService.Data.Player.SaveAsync(data).AsUniTask();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving player data: {ex.Message}");
            }
        }

        public async UniTask SavePlayerDataToCloud(IAuthPlayerData authPlayerData)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(authPlayerData);
                Dictionary<string, object> data = new Dictionary<string, object> { { authPlayerData.PlayerID, jsonData } };
                await CloudSaveService.Data.Player.SaveAsync(data).AsUniTask();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving player data: {ex.Message}");
            }
        }

        public async UniTask<(bool success, IAuthPlayerData playerData)> TryGetPlayerDataFromCloud(string key)
        {
            try
            {
                Dictionary<string, Item> data = await CloudSaveService.Data.Player.LoadAsync(new HashSet<string> { AuthenticationService.Instance.PlayerId }).AsUniTask();
                if (data.TryGetValue(key, out Item keyValue))
                {
                    AuthPlayerData authPlayerData = JsonUtility.FromJson<AuthPlayerData>(keyValue.Value.GetAsString());
                    return (true, authPlayerData);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading player data: {ex.Message}", this);
                return (false, null);

            }

            return (false, null);
        }


        public async UniTask<(bool, string)> TryGetPlayerName(string key)
        {
            try
            {
                (bool success, IAuthPlayerData playerData) = await TryGetPlayerDataFromCloud(key);
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

        public async UniTask<(bool, string)> TryGetPlayerEmail(string key)
        {
            try
            {
                (bool success, IAuthPlayerData playerData) = await TryGetPlayerDataFromCloud(key);
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


        private async UniTask OnAuthenticationSignOutEvent(AuthenticationSignOutEvent arg)
        {
            await SignOut(arg.AuthPlayerData);
            await UniTask.Yield();
        }

        public async UniTask SignOut(IAuthPlayerData authPlayerData)
        {
            if (authPlayerData == null) return;

            try
            {
                await SavePlayerDataToCloud(authPlayerData);
                Log("Player data saved successfully.", this);
            }
            catch (Exception ex)
            {
                LogError($"Error occurred while saving player data: {ex.Message}", this);
            }

            AuthenticationService.Instance.SignOut();

        }


    }
}