using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Manager.LLMServices;
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
    [CreateAssetMenu(fileName = "UnityServicesManager", menuName = "OcentraAI/UnityServicesManager", order = 1)]
    public class UnityServicesManager : ScriptableSingletonBase<UnityServicesManager>, IUnityServicesManager
    {
        public IAnalyticsService AnalyticsService => Unity.Services.Analytics.AnalyticsService.Instance;
        public ICloudSaveService CloudSaveService => Unity.Services.CloudSave.CloudSaveService.Instance;
        public IAuthenticationService AuthenticationService => Unity.Services.Authentication.AuthenticationService.Instance;

        [ShowInInspector] public IConfigManager ConfigManager { get; private set; } = new ConfigManager();


        public override async UniTask InitializeAsync()
        {
            if (Application.isPlaying)
            {
                if (!IsInitialized)
                {
                    try
                    {
                        InitializationOptions options = new InitializationOptions();
                        options.SetOption("com.unity.services.core.environment-name", "production");
                        await UnityServices.InitializeAsync(options).AsUniTask();
                        AnalyticsService.StartDataCollection();


                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.LogException($"Failed to initialize {nameof(UnityServicesManager)}: {ex.Message}", this);
                        return;
                    }

                    await base.InitializeAsync();
                }
            }

        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<AuthenticationSignOutEvent>(OnAuthenticationSignOutEvent);
            EventRegistrar.Subscribe<RequestPlayerDataFromCloudEvent>(OnRequestPlayerDataFromCloudEvent);
            EventRegistrar.Subscribe<SavePlayerDataToCloudEvent>(OnSavePlayerDataToCloudEvent);
            EventRegistrar.Subscribe<AuthenticationCompletedEvent>(OnAuthenticationCompletedEvent);

        }

        private async UniTask OnAuthenticationCompletedEvent(AuthenticationCompletedEvent arg)
        {
            await WaitForInitializationAsync();
            await ConfigManager.FetchConfig(arg.AuthPlayerData.PlayerID);
            await AIModelManager.Instance.UpdateProvider(ConfigManager);
            await UniTask.Yield();
        }


        private async UniTask OnSavePlayerDataToCloudEvent(SavePlayerDataToCloudEvent arg)
        {
            await WaitForInitializationAsync();
            await SavePlayerDataToCloud(arg.AuthPlayerData);
            await UniTask.Yield();
        }

        private async UniTask OnRequestPlayerDataFromCloudEvent(RequestPlayerDataFromCloudEvent arg)
        {
            await WaitForInitializationAsync();
            (bool success, IAuthPlayerData playerData) = await TryGetPlayerDataFromCloud(arg.PlayerId);

            arg.PlayerDataSource.TrySetResult((success, playerData));
        }


        public async UniTask SavePlayerDataToCloud(string key, IAuthPlayerData authPlayerData)
        {
            await WaitForInitializationAsync();
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
            await WaitForInitializationAsync();
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
            await WaitForInitializationAsync();
            try
            {
                Dictionary<string, Item> data = await CloudSaveService.Data.Player.LoadAsync(new HashSet<string> { AuthenticationService.PlayerId }).AsUniTask();
                if (data.TryGetValue(key, out Item keyValue))
                {
                    AuthPlayerData authPlayerData = JsonUtility.FromJson<AuthPlayerData>(keyValue.Value.GetAsString());
                    return (true, authPlayerData);
                }
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error loading player data: {ex.Message}", this);
                return (false, null);

            }

            return (false, null);
        }


        public async UniTask<(bool, string)> TryGetPlayerName(string key)
        {
            await WaitForInitializationAsync();
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
            await WaitForInitializationAsync();
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
            await WaitForInitializationAsync();
            await SignOut(arg.AuthPlayerData);
            await UniTask.Yield();
        }

        public async UniTask SignOut(IAuthPlayerData authPlayerData)
        {
            if (authPlayerData == null) return;

            try
            {
                await SavePlayerDataToCloud(authPlayerData);
                GameLoggerScriptable.Log("Player data saved successfully.", this);
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error occurred while saving player data: {ex.Message}", this);
            }

            AuthenticationService.SignOut();

        }


    }
}