#if UNITY_EDITOR

using Assets.Plugins.ParrelSync.Editor;
using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Manager.Authentication;
using OcentraAI.LLMGames.Networking;
using OcentraAI.LLMGames.Networking.Manager;
using OcentraAI.LLMGames.Utilities;
using ParrelSync;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using static System.String;
using Object = UnityEngine.Object;


namespace OcentraAI.LLMGames.Scriptable.ScriptableSingletons
{
    [CreateAssetMenu(fileName = nameof(AutoNetworkBootstrap), menuName = "LLMGames/AutoNetworkBootstrap")]
    [GlobalConfig("Assets/Resources/")]
    public class AutoNetworkBootstrap : CustomGlobalConfig<AutoNetworkBootstrap>
    {

        private readonly object @lock = new object();
        private bool areAllPlayersReady = false;

        [ShowInInspector, ReadOnly]
        private bool AreAllPlayersReady
        {
            get
            {
                lock (@lock)
                {
                    return areAllPlayersReady;
                }
            }
            set
            {
                lock (@lock)
                {
                    areAllPlayersReady = value;
                }
            }
        }

        [ShowInInspector, ReadOnly] private bool IsNetworkManagerCreated { get; set; }
        [ShowInInspector, ReadOnly] private bool IsGameStartManagerCreated { get; set; }
        [ShowInInspector] private int MaxRetries { get; set; } = 10;
        [ShowInInspector] private float InitialRetryDelay { get; set; } = 1;
        [ShowInInspector] private NetworkManager NetworkManager { get; set; }
        [ShowInInspector] private GameStartManager GameStartManager { get; set; }
        [ShowInInspector] private ConfigManager ConfigManager { get; set; }

        [ReadOnly] public EditorData EditorData;

        public const string JoinCode = "DevLobby";
        public const string LobbyName = "DevLobby";
        public const string ServerReady = nameof(ServerReady);
        public const string ClientReady = nameof(ClientReady);
        private const string PlayerName = nameof(PlayerName);
        private const string PlayerId = nameof(PlayerId);

        [SerializeField] public bool StartAsServer = false;


        [ShowInInspector] private LobbyViewer LobbyViewer { get; set; } = null;

        [Required, ValueDropdown(nameof(GetScenesFromBuild), DropdownTitle = "Select Main Login Scene"), InfoBox("Make sure scene is in Build Settings!")]
        public string MainLoginScene;

        [Sirenix.OdinInspector.FilePath(AbsolutePath = true, Extensions = "json"), ValidateInput(nameof(IsValidJsonFile), "Selected file is not a valid JSON file.")]
        public string EditorSyncFilePath = Application.isEditor ? Path.Combine(Application.dataPath, "Resources", "EditorSync.json") : Path.Combine(Application.persistentDataPath, "EditorSync.json");

        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        private bool IsValidJsonFile(string path)
        {
            if (IsNullOrEmpty(path) || !File.Exists(path) || Path.GetExtension(path).ToLower() != ".json")
            {
                return false;
            }
            return true;
        }

        private IEnumerable GetScenesFromBuild()
        {
            List<ValueDropdownItem> sceneList = new List<ValueDropdownItem>();

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length > 0)
            {
                EditorBuildSettingsScene firstEnabledScene = scenes.FirstOrDefault(s => s.enabled);

                if (firstEnabledScene != null)
                {
                    MainLoginScene = Path.GetFileNameWithoutExtension(firstEnabledScene.path);
                }

                foreach (EditorBuildSettingsScene settingsScene in scenes)
                {
                    if (settingsScene.enabled)
                    {
                        string sceneName = Path.GetFileNameWithoutExtension(settingsScene.path);
                        string displayName = sceneName;

                        if (sceneName == MainLoginScene)
                        {
                            displayName += " (Default)";
                        }

                        sceneList.Add(new ValueDropdownItem(displayName, sceneName));
                    }
                }
            }

            return sceneList;
        }

        public async void Initialize(EditorData editorData)
        {
            EditorData = editorData;
            IsNetworkManagerCreated = false;
            AreAllPlayersReady = false;

            if (EditorData.SyncEnabled)
            {
                if (InitialRetryDelay == 0)
                {
                    InitialRetryDelay = 1;
                }

                InitializeNetworkManager();

                bool isClone = ClonesManager.IsClone();
                if (isClone)
                {
                    string customArgument = ClonesManager.GetArgument();
                    PlayerSettings.productName = customArgument;
                    GameLoggerScriptable.Log($"Product name set to: {PlayerSettings.productName}", this);
                }
                else
                {
                    await ClonesManagerExtensions.ClearLobbyDataFromFile(EditorSyncFilePath);
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }



                GameLoggerScriptable.Log($"{nameof(AutoNetworkBootstrap)}", this);

            }


        }

        private void InitializeNetworkManager()
        {
            try
            {
                if (NetworkManager == null && !Application.isPlaying)
                {
                    NetworkManager existingManager = FindAnyObjectByType<NetworkManager>();

                    if (existingManager != null)
                    {
                        NetworkManager = existingManager;
                        IsNetworkManagerCreated = false;
                    }
                    else
                    {
                        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{nameof(NetworkManager)}");

                        if (prefab != null)
                        {
                            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                            if (instance != null)
                            {
                                instance.transform.SetParent(null);
                                NetworkManager = instance.GetComponent<NetworkManager>();
                                IsNetworkManagerCreated = true;
                                GameLoggerScriptable.Log($" IsNetworkManagerCreated {IsNetworkManagerCreated}", this);
                            }
                            else
                            {
                                GameLoggerScriptable.LogError("Failed to instantiate the prefab as a linked instance.", this);

                            }
                        }
                        else
                        {
                            GameLoggerScriptable.LogError($"Prefab '{nameof(NetworkManager)}' not found in Resources/Prefabs.", this);

                        }
                    }
                }


            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error initializing NetworkManager: {ex.Message}\nStack Trace: {ex.StackTrace}", this);

            }
        }


        private async UniTask<GameStartManager> CreateGameStartManagerServer(List<Player> playerList)
        {
            await UniTask.WaitUntil(() => NetworkManager.Singleton.IsListening);

            if (Application.isPlaying)
            {
                GameStartManager gameStartManager = FindAnyObjectByType<GameStartManager>();

                if (gameStartManager == null)
                {
                    NetworkObject prefab = Resources.Load<NetworkObject>($"Prefabs/{nameof(GameStartManager)}");

                    if (prefab != null)
                    {
                        GameLoggerScriptable.Log($"GameStartManager is null; prefab found. Creating one.", this);

                        if (NetworkManager.Singleton != null)
                        {
                            NetworkObject instance = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(prefab);

                            if (instance != null)
                            {
                                instance.gameObject.name = nameof(GameStartManager);
                                instance.transform.SetParent(null);
                                gameStartManager = instance.GetComponent<GameStartManager>();
                                gameStartManager.Init(playerList);
                                IsGameStartManagerCreated = true;
                                GameLoggerScriptable.Log($"IsGameStartManagerCreated {IsGameStartManagerCreated}", this);
                                return gameStartManager;
                            }
                        }
                        else
                        {
                            GameLoggerScriptable.LogError($"NetworkManager.Singleton is null or not running as server.", this);
                        }
                    }
                    else
                    {
                        GameLoggerScriptable.LogError($"NetworkObject prefab not found.", this);
                    }
                }
                else
                {
                    IsGameStartManagerCreated = false;
                    gameStartManager.Init(playerList);
                    GameLoggerScriptable.Log($"GameStartManager exists; initialized.", this);
                    return gameStartManager;
                }


            }

            await UniTask.Yield();
            return null;
        }

        private async UniTask<GameStartManager> CreateGameStartManagerClient(List<Player> playerList)
        {

            if (Application.isPlaying)
            {
                GameStartManager gameStartManager = FindAnyObjectByType<GameStartManager>();

                if (gameStartManager == null)
                {
                    GameLoggerScriptable.Log($"Client-side: Waiting for GameStartManager to become available.", this);

                    // Wait until the GameStartManager is available
                    while (gameStartManager == null)
                    {
                        GameLoggerScriptable.Log($"Looking for GameStartManager on the client.", this);
                        await UniTask.Delay(100); // Adding a slight delay to avoid excessive logging
                        gameStartManager = FindAnyObjectByType<GameStartManager>();
                    }

                    IsGameStartManagerCreated = false;
                    gameStartManager.Init(playerList);
                    GameLoggerScriptable.Log($"GameStartManager found and initialized on client.", this);
                    return gameStartManager;
                }
                else
                {
                    IsGameStartManagerCreated = false;
                    gameStartManager.Init(playerList);
                    GameLoggerScriptable.Log($"GameStartManager already exists on client; initialized.", this);
                    return gameStartManager;
                }


            }

            await UniTask.Yield();
            return null;
        }


        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            int currentPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            if (currentPlayers > EditorData.HumanPlayersCount)
            {
                if (!ClonesManager.IsClone() && NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.DisconnectClient(clientId);
                    return;
                }
            }


        }

        private void OnClientDisconnected(ulong clientId)
        {

        }

        private async UniTask<bool> InitializeUnityServices()
        {
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            {
                try
                {
                    InitializationOptions options = new InitializationOptions();
                    options.SetOption("com.unity.services.core.environment-name", "production");
                    await UnityServices.InitializeAsync(options).AsUniTask();


                    return true;
                }
                catch (Exception e)
                {
                    GameLoggerScriptable.LogError(e.StackTrace, this);
                    return false;
                }
            }
            return true;
        }

        private async UniTask<bool> ClearAndSignIn()
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    AuthenticationService.Instance.ClearSessionToken();
                    await AuthenticationService.Instance.SignInAnonymouslyAsync().AsUniTask();
                    GameLoggerScriptable.Log($"Signed in with ID: {AuthenticationService.Instance.PlayerId}", this);
                }
                return true;
            }
            catch (Exception e)
            {
                GameLoggerScriptable.LogError($"Authentication failed: {e.Message}\nStack Trace: {e.StackTrace}", this);
                return false;
            }
        }

        public async void OnPlayModeStateChanged(PlayModeStateChange state)
        {

            await UniTask.WaitUntil(() => !EditorApplication.isCompiling && !EditorApplication.isUpdating);

            if (!EditorData.SyncEnabled || EditorData.ScenePath == MainLoginScene) { return; }

            try
            {
                switch (state)
                {
                    case PlayModeStateChange.EnteredPlayMode:

                        try
                        {


                            if (NetworkManager.Singleton != null)
                            {
                                NetworkManager = NetworkManager.Singleton;

                                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                                if (await InitializeUnityServices())
                                {
                                    if (await ClearAndSignIn())
                                    {
                                        ConfigManager = new ConfigManager();
                                        await ConfigManager.FetchConfig();

                                        if (ClonesManager.IsClone())
                                        {
                                            await StartClient();
                                        }
                                        else
                                        {
                                            await StartHost();
                                        }


                                    }
                                }
                            }
                            else
                            {
                                GameLoggerScriptable.LogError("NetworkManager.Singleton is null. Ensure that it is properly initialized.", this);
                            }


                        }
                        catch (Exception ex)
                        {
                            string message = $"Error during initialization in play mode: {ex.Message}\nStack Trace: {ex.StackTrace}";
                            GameLoggerScriptable.LogError(message, this);
                        }

                        break;

                    case PlayModeStateChange.ExitingPlayMode:

                        try
                        {
                            await Cleanup();
                        }
                        catch (Exception ex)
                        {
                            GameLoggerScriptable.LogError($"Error during cleanup in play mode: {ex.Message}\nStack Trace: {ex.StackTrace}", this);
                        }
                        break;
                }


            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Unhandled error in OnPlayModeStateChanged: {ex.Message}\nStack Trace: {ex.StackTrace}", this);
            }
        }




        // we Start for join code get joinAllocation and then SetRelayServerData we wont start game yet we need to check if we are ready
        private async UniTask<OperationResult<bool>> StartHost()
        {
            GameLoggerScriptable.Log("StartHost process initiated", this);

            try
            {
                GameLoggerScriptable.Log("Waiting for player authentication", this);
                await UniTask.WaitUntil(() => AuthenticationService.Instance.IsSignedIn);

                GameLoggerScriptable.Log("Creating and initializing a lobby", this);
                Lobby currentLobby = await CreateAndInitializeLobby();
                GameLoggerScriptable.Log($"Lobby created successfully with ID: {currentLobby.Id}", this);

                GameLoggerScriptable.Log("Saving lobby data to file", this);
                bool dataset = await ClonesManagerExtensions.TrySetLobbyDataAsync(EditorSyncFilePath, currentLobby);

                await UniTask.WaitUntil(() => dataset);

                int maxRetries = 100;
                GameLoggerScriptable.Log("Checking and waiting until all players are ready", this);
                if (!await CheckAndWaitUntilAllPlayersAreReady(currentLobby.Id, 1, maxRetries))
                {
                    GameLoggerScriptable.LogError("Players not ready after maximum retries", this);
                    return new OperationResult<bool>(false, false, maxRetries, "Players not ready after maximum retries");
                }

                if (StartAsServer)
                {
                    GameLoggerScriptable.Log("Starting as server", this);
                    NetworkManager.Singleton.StartServer();
                    GameLoggerScriptable.Log("Server started successfully", this);



                    GameLoggerScriptable.Log("Setting server ready state to true", this);
                    OperationResult<Lobby> operationResult = await SetServerReady(currentLobby.Id, true);
                    if (!operationResult.IsSuccess)
                    {
                        GameLoggerScriptable.LogError("Failed to set server ready", this);
                        return new OperationResult<bool>(false, false, 0, "Failed to set server ready");
                    }

                    currentLobby = operationResult.Value;
                    await CreateGameStartManagerServer(currentLobby.Players);

                    GameLoggerScriptable.Log("Starting client as part of host setup", this);
                    OperationResult<bool> clientResult = await StartClient();
                    return new OperationResult<bool>(clientResult.IsSuccess, clientResult.Value);
                }
                else
                {
                    GameLoggerScriptable.Log("Starting as host", this);
                    if (!NetworkManager.Singleton.StartHost())
                    {
                        GameLoggerScriptable.LogError("Host failed to start", this);
                        return new OperationResult<bool>(false, false, 0, "Host failed to start");
                    }

                    GameLoggerScriptable.Log("Host started successfully", this);
                    await CreateGameStartManagerServer(currentLobby.Players);

                    GameLoggerScriptable.Log("Setting server ready state to true", this);
                    OperationResult<Lobby> operationResult = await SetServerReady(currentLobby.Id, true);

                    if (!operationResult.IsSuccess)
                    {
                        GameLoggerScriptable.LogError("Failed to set server ready", this);
                        return new OperationResult<bool>(false, false, 0, "Failed to set server ready");
                    }

                    currentLobby = operationResult.Value;

                    GameLoggerScriptable.Log("Initializing LobbyViewer", this);
                    LobbyViewer = new LobbyViewer(currentLobby);

                    GameLoggerScriptable.Log("Publishing StartLobbyAsHostEvent event", this);
                    await EventBus.Instance.PublishAsync(new StartLobbyAsHostEvent(currentLobby.Id));

                    GameLoggerScriptable.Log("Host started successfully and game initialization event published", this);
                    return new OperationResult<bool>(true, true, 0, "Host started successfully");
                }
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"StartHost failed with exception: {ex.Message}\n{ex.StackTrace}", this);
                return new OperationResult<bool>(false, false, 0, ex.Message);
            }
        }

        private async UniTask<OperationResult<Lobby>> SetServerReady(string lobbyId, bool isReady)
        {
            try
            {
                GameLoggerScriptable.Log($"Setting server ready status to {isReady} for lobby ID: {lobbyId}", this);
                string readyStatus = isReady ? "true" : "false";

                Lobby currentLobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { ServerReady, new DataObject(DataObject.VisibilityOptions.Public, readyStatus) }
                    }
                }).AsUniTask();

                GameLoggerScriptable.Log($"Server ready status successfully set to {readyStatus} for lobby ID: {lobbyId}", this);
                return new OperationResult<Lobby>(true, currentLobby);
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Failed to set server ready status for lobby ID: {lobbyId} with exception: {ex.Message}", this);
                return new OperationResult<Lobby>(false, null);
            }
        }

        private async UniTask<Lobby> CreateAndInitializeLobby()
        {
            GameLoggerScriptable.Log("Creating and initializing a new lobby", this);

            try
            {
                string playerName = AuthenticationService.Instance.PlayerName ?? "Guest_" + UnityEngine.Random.Range(0, 9999);
                string playerId = AuthenticationService.Instance.PlayerId ?? Guid.NewGuid().ToString();

                GameLoggerScriptable.Log($"Player name set to: {playerName}, Player ID set to: {playerId}", this);

                Player player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { PlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                        { PlayerId, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerId) }
                    },
                    Profile = new PlayerProfile(playerName)
                };

                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = true,
                    Player = player,
                    Data = new Dictionary<string, DataObject>
                    {
                        { ServerReady, new DataObject(DataObject.VisibilityOptions.Public, "false") }
                    }
                };

                GameLoggerScriptable.Log("Attempting to create a lobby", this);
                Lobby newLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, EditorData.HumanPlayersCount, createLobbyOptions).AsUniTask();

                GameLoggerScriptable.Log("Lobby created successfully with initial ServerReady state set to false", this);
                return newLobby;
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Failed to create and initialize lobby with exception: {ex.Message}", this);
                throw;
            }
        }



        // we look for join code get joinAllocation and then SetRelayServerData we wont start game yet
        private async UniTask<OperationResult<bool>> StartClient()
        {
            GameLoggerScriptable.Log("StartClient process initiated", this);

            (bool success, Lobby lobbyData) result = await ClonesManagerExtensions.TryGetLobbyDataAsync(EditorSyncFilePath);

            if (!result.success)
            {
                GameLoggerScriptable.LogError("Failed to get lobby data from file", this);
                return new OperationResult<bool>(false, default, 0, "TryGetLobbyDataAsync not successful");
            }

            if (result.lobbyData == null)
            {
                GameLoggerScriptable.LogError("Lobby data is null", this);
                return new OperationResult<bool>(false, default, 0, "Failed to get lobby data from file");
            }

            try
            {
                GameLoggerScriptable.Log("Waiting for player authentication", this);
                await UniTask.WaitUntil(() => AuthenticationService.Instance.IsSignedIn);

                string playerName = AuthenticationService.Instance.PlayerName ?? "Guest_" + UnityEngine.Random.Range(0, 9999);
                string playerId = AuthenticationService.Instance.PlayerId ?? Guid.NewGuid().ToString();

                GameLoggerScriptable.Log($"Player name: {playerName}, Player ID: {playerId}", this);

                Player player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject> {
                        { PlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                        { PlayerId, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerId) }

                    },
                    Profile = new PlayerProfile(playerName)
                };

                JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions { Player = player };

                GameLoggerScriptable.Log($"Attempting to join lobby with code: {result.lobbyData.LobbyCode}", this);
                Lobby currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(result.lobbyData.LobbyCode, joinOptions).AsUniTask();

                if (currentLobby == null)
                {
                    GameLoggerScriptable.LogError("Failed to join lobby", this);
                    return new OperationResult<bool>(false, default, 0, "Failed to join lobby");
                }

                if (IsNullOrWhiteSpace(currentLobby.LobbyCode))
                {
                    GameLoggerScriptable.LogError("Joined lobby has no code", this);
                    return new OperationResult<bool>(false, default, 0, "Joined lobby has no code");
                }

                GameLoggerScriptable.Log("Setting client ready state", this);

                currentLobby = await LobbyService.Instance.UpdatePlayerAsync(
                    currentLobby.Id,
                    AuthenticationService.Instance.PlayerId,
                    new UpdatePlayerOptions
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { ClientReady, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "true") }
                        }
                    }
                ).AsUniTask();

                GameLoggerScriptable.Log("Waiting for server ready state", this);

                int maxRetries = 100;
                OperationResult<Lobby> operationResult = await WaitForServerReady(currentLobby.Id, 1, maxRetries);
                if (!operationResult.IsSuccess)
                {
                    GameLoggerScriptable.LogError("WaitForServerReady maxed out", this);
                    return new OperationResult<bool>(false, false, maxRetries, "WaitForServerReady maxed out");
                }

                currentLobby = operationResult.Value;

                if (currentLobby != null)
                {
                    LobbyViewer = new LobbyViewer(currentLobby);

                    GameLoggerScriptable.Log("Starting client", this);
                    bool startClient = NetworkManager.Singleton.StartClient();
                    GameLoggerScriptable.Log($"Starting client Success? {startClient}", this);

                    await UniTask.WaitUntil(() => startClient);

                    GameStartManager gameStartManager = await CreateGameStartManagerClient(currentLobby.Players);


                    GameLoggerScriptable.Log($"Client started successfully , GameStartManager found? {gameStartManager != null} ", this);
                    return new OperationResult<bool>(true, true, 0, "Client started successfully");
                }
                else
                {
                    GameLoggerScriptable.LogError("Client Could not Start !!!successfully", this);

                    return new OperationResult<bool>(false, false);
                }

            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogException($"StartClient failed with exception: {ex.Message}\n{ex.StackTrace}", this);
                return new OperationResult<bool>(false, false, 0, ex.Message);
            }
        }

        private async UniTask<OperationResult<Lobby>> WaitForServerReady(string lobbyId, int initialDelaySeconds = 1, int maxRetries = 100)
        {
            GameLoggerScriptable.Log($"WaitForServerReady started for lobby ID: {lobbyId}", this);

            int currentDelaySeconds = initialDelaySeconds;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                GameLoggerScriptable.Log($"Attempt {attempt + 1}/{maxRetries} - Checking server readiness. Current delay before next attempt: {currentDelaySeconds} seconds", this);

                try
                {
                    OperationResult<Lobby> lobbyResult = await GetLobbyWithExponentialBackoff(lobbyId, initialDelaySeconds, maxRetries);

                    if (!lobbyResult.IsSuccess)
                    {
                        GameLoggerScriptable.LogError($"Attempt {attempt + 1} - Failed to fetch lobby. OperationResult was not successful.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    if (lobbyResult.Value == null)
                    {
                        GameLoggerScriptable.LogError($"Attempt {attempt + 1} - Lobby result is null. Retrying after {currentDelaySeconds} seconds delay.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    Lobby currentLobby = lobbyResult.Value;

                    GameLoggerScriptable.Log($"Attempt {attempt + 1} - Lobby fetched successfully. Checking server readiness status.", this);

                    if (currentLobby == null)
                    {
                        GameLoggerScriptable.Log($"Somehow lobby data is null unexpectedly ??? . Waiting to retry.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    DataObject serverReadyData = default;

                    if (currentLobby.Data != null)
                    {
                        GameLoggerScriptable.Log("Current lobby data contents:", this);

                        foreach (KeyValuePair<string, DataObject> kvp in currentLobby.Data)
                        {
                            GameLoggerScriptable.Log($"Key: {kvp.Key}, Value: {kvp.Value.Value}", this);

                            if (kvp.Key == ServerReady)
                            {
                                serverReadyData = kvp.Value;
                                GameLoggerScriptable.Log($"ServerReady data found. Value: {serverReadyData.Value}", this);
                            }
                        }
                    }
                    else
                    {
                        GameLoggerScriptable.LogError("Lobby data is null. Waiting to retry.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    if (serverReadyData is { Value: not null } && serverReadyData.Value != "true")
                    {
                        GameLoggerScriptable.Log($"Server is not ready. Waiting to retry.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }



                    GameLoggerScriptable.Log($"Server is ready. Exiting WaitForServerReady successfully.", this);
                    return new OperationResult<Lobby>(true, currentLobby);
                }
                catch (LobbyServiceException lobbyEx) when (lobbyEx.Reason == LobbyExceptionReason.RateLimited)
                {
                    GameLoggerScriptable.LogError($"Attempt {attempt + 1} - Rate-limiting exception encountered: {lobbyEx.Message}. Retrying after {currentDelaySeconds} seconds delay.", this);
                    attempt++;
                    await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                    currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                }
                catch (LobbyServiceException lobbyEx)
                {
                    GameLoggerScriptable.LogError($"Attempt {attempt + 1} - LobbyServiceException encountered: {lobbyEx.Message} with reason: {lobbyEx.Reason}. Retrying after {currentDelaySeconds} seconds delay.", this);
                    attempt++;
                    await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                    currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Attempt {attempt + 1} - General exception encountered: {ex.Message} {ex.StackTrace}. Retrying after {currentDelaySeconds} seconds delay.", this);
                    attempt++;
                    await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                    currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                }
            }

            GameLoggerScriptable.LogError($"Max retries ({maxRetries}) reached. Server is not ready. Exiting WaitForServerReady with failure.", this);
            return new OperationResult<Lobby>(false, null);
        }




        private async UniTask<bool> CheckAndWaitUntilAllPlayersAreReady(string lobbyId, int initialDelaySeconds = 1, int maxRetries = 100)
        {
            GameLoggerScriptable.Log($"CheckAndWaitUntilAllPlayersAreReady started for lobby ID: {lobbyId}", this);

            int currentDelaySeconds = initialDelaySeconds;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                GameLoggerScriptable.Log($"Attempt {attempt + 1}/{maxRetries} - Current delay before next attempt: {currentDelaySeconds} seconds", this);

                try
                {
                    OperationResult<Lobby> lobbyResult = await GetLobbyWithExponentialBackoff(lobbyId, initialDelaySeconds, maxRetries);

                    if (!lobbyResult.IsSuccess)
                    {
                        GameLoggerScriptable.Log($"Failed to fetch lobby. OperationResult was not successful. Attempt {attempt + 1}.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    if (lobbyResult.Value == null)
                    {
                        GameLoggerScriptable.Log($"Lobby result is null. Attempt {attempt + 1}. Retrying...", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    Lobby lobby = lobbyResult.Value;
                    GameLoggerScriptable.Log($"Lobby fetched successfully. Players in lobby: {lobby.Players.Count}/{lobby.MaxPlayers}", this);

                    if (lobby.Players.Count < lobby.MaxPlayers)
                    {
                        GameLoggerScriptable.Log($"Not all players have joined the lobby. Current: {lobby.Players.Count}/{lobby.MaxPlayers}. Waiting for more players to join.", this);
                        attempt++;
                        await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                        currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                        continue;
                    }

                    bool allPlayersReady = true;

                    foreach (Player player in lobby.Players)
                    {
                        if (player.Id == lobby.HostId)
                        {
                            GameLoggerScriptable.Log($"Skipping readiness check for host player (Player ID: {player.Id})", this);
                            continue;
                        }

                        if (player.Data.TryGetValue(ClientReady, out PlayerDataObject playerDataObject))
                        {
                            if (playerDataObject?.Value != "true")
                            {
                                GameLoggerScriptable.Log($"Player {player.Id} is not ready. Value: {playerDataObject?.Value}", this);
                                allPlayersReady = false;
                                break;
                            }
                        }
                        else
                        {
                            GameLoggerScriptable.Log($"ClientReady data not found for player {player.Id}.", this);
                            allPlayersReady = false;
                            break;
                        }
                    }

                    if (allPlayersReady)
                    {
                        GameLoggerScriptable.Log($"All players are ready in the lobby.", this);
                        return true;
                    }
                    else
                    {
                        GameLoggerScriptable.Log($"Not all players are ready. Retrying...", this);
                    }

                    attempt++;
                    await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                    currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Exception encountered: {ex.Message}. Attempt {attempt + 1}. Retrying...", this);
                    attempt++;
                    await UniTask.Delay(TimeSpan.FromSeconds(currentDelaySeconds));
                    currentDelaySeconds = Math.Min(currentDelaySeconds * 2, 60);
                }
            }

            GameLoggerScriptable.LogError($"Max retries reached. Exiting CheckAndWaitUntilAllPlayersAreReady without success.", this);
            return false;
        }


        private async UniTask<OperationResult<Lobby>> GetLobbyWithExponentialBackoff(string lobbyId, float initialRetryDelay = 1, int maxRetries = 100, int maxDelay = 600)
        {
            float delay = initialRetryDelay;

            for (int i = 0; i < maxRetries; i++)
            {
                GameLoggerScriptable.Log($"Attempt {i + 1}/{maxRetries} - Trying to fetch lobby with ID: {lobbyId}", this);

                try
                {
                    Lobby result = await LobbyService.Instance.GetLobbyAsync(lobbyId).AsUniTask();

                    if (result == null)
                    {
                        GameLoggerScriptable.LogError($"Attempt {i + 1} - Lobby result is null. Retrying after {delay} seconds delay.", this);
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                        delay = Math.Min(delay * 2, maxDelay);
                        continue;
                    }

                    GameLoggerScriptable.Log($"Attempt {i + 1} - Lobby fetched successfully.", this);
                    return new OperationResult<Lobby>(true, result, maxRetries);
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Attempt {i + 1} - Exception encountered: {ex.Message}", this);

                    // Check if this is the last attempt or if the exception is not related to rate limiting
                    if (i == maxRetries - 1 || !(ex is LobbyServiceException { Reason: LobbyExceptionReason.RateLimited }))
                    {
                        GameLoggerScriptable.LogError($"Attempt {i + 1} - Max retries reached or non-rate limiting exception. Returning failure result.", this);
                        return new OperationResult<Lobby>(false, null, i + 1, ex.Message);
                    }

                    GameLoggerScriptable.Log($"Attempt {i + 1} - Rate limiting encountered. Retrying after {delay} seconds delay.", this);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    delay = Math.Min(delay * 2, maxDelay);
                }
            }

            GameLoggerScriptable.LogError($"Max retries ({maxRetries}) exhausted without success. Returning failure result.", this);
            return new OperationResult<Lobby>(false, null, maxRetries, "Max retries exhausted without success.");
        }


        private async UniTask Cleanup()
        {

            try
            {
                if (GameStartManager != null && IsGameStartManagerCreated)
                {
                    DestroyImmediate(GameStartManager.gameObject);
                    GameStartManager = null;
                    IsGameStartManagerCreated = false;
                }

                if (AuthenticationService.Instance != null)
                {
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        AuthenticationService.Instance.SignOut();

                    }
                    AuthenticationService.Instance.ClearSessionToken();
                }

                if (NetworkManager.Singleton != null)
                {
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                    NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                    NetworkManager.Singleton.Shutdown();
                }


                ConfigManager = new ConfigManager();

                if (NetworkManager != null)
                {
                    NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                    NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;

                    if (NetworkManager.gameObject != null && IsNetworkManagerCreated)
                    {
                        DestroyImmediate(NetworkManager.gameObject);
                        NetworkManager = null;
                        IsNetworkManagerCreated = false;
                    }
                }

                await ClonesManagerExtensions.ClearLobbyDataFromFile(EditorSyncFilePath);

                await UniTask.Yield();
            }
            catch (Exception e)
            {
                GameLoggerScriptable.LogError($"Error disposing cancellation token: {e.Message} {e.StackTrace}", this);
            }
        }


    }


    [Serializable]
    public class EditorData
    {
        public int HumanPlayersCount = 1;
        public int AIPlayersCount = 0;
        public string ScenePath = "";
        public bool SyncEnabled = true;
        public bool IsNewSceneLoaded = false;
    }
}

#endif