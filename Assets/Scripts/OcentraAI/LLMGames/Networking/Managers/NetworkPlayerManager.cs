using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    public class NetworkPlayerManager : NetworkManagerBase, IPlayerManager
    {
        [ShowInInspector, Required] GameObject ComputerPlayerPrefab { get; set; }

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<int, IPlayerBase> NetworkPlayers { get; } = new ConcurrentDictionary<int, IPlayerBase>();
        [ShowInInspector] public IHumanPlayerData LocalNetworkHumanPlayer { get; set; }
        [ShowInInspector] private List<Player> LobbyPlayerList { get; set; }
        [ShowInInspector] private List<string> ComputerPlayerList { get; set; }

        [ShowInInspector] public bool AreAllPlayersSynced { get; set; } = false;

        [ShowInInspector, ReadOnly] protected readonly HashSet<IPlayerBase> FoldedPlayers  = new HashSet<IPlayerBase>();

        [ShowInInspector, ReadOnly] bool StartTurnManagerEventPublished { get; set; } = false;

        [ShowInInspector, ReadOnly] private List<IHumanPlayerData> HumanPlayers { get; set; } = new List<IHumanPlayerData>();
        [ShowInInspector, ReadOnly] private List<IComputerPlayerData> ComputerPlayers { get; set; } = new List<IComputerPlayerData>();
        [ShowInInspector, ReadOnly] private List<IPlayerBase> AllPlayers { get; set; } = new List<IPlayerBase>();


        public ScriptableObject GetGameMode()
        {
            return NetworkGameManager.GameMode;
        }


        public override void OnValidate()
        {
            base.OnValidate();
            LoadComputerPlayerPrefab();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            LoadComputerPlayerPrefab();
        }

        public override void Awake()
        {
            base.Awake();
            LoadComputerPlayerPrefab();
            AreAllPlayersSynced = false;
            StartTurnManagerEventPublished = false;
        }



        private GameObject LoadComputerPlayerPrefab()
        {
            if (ComputerPlayerPrefab == null)
            {
                ComputerPlayerPrefab = Resources.Load<GameObject>($"Prefabs/{nameof(ComputerPlayerPrefab)}");
            }
            return ComputerPlayerPrefab;
        }


        public override void SubscribeToEvents()
        {
            EventRegistrar.Subscribe<RegisterHumanPlayerEvent>(OnRegisterPlayer);
            EventRegistrar.Subscribe<UnRegisterPlayerEvent>(OnUnregisterPlayer);
            EventRegistrar.Subscribe<RequestLobbyPlayerDataEvent>(OnRequestLobbyPlayerData);
            EventRegistrar.Subscribe<GetLocalPlayerEvent>(OnGetLocalPlayer);
            EventRegistrar.Subscribe<RequestAllPlayersDataEvent>(OnRequestAllPlayersDataEvent);
            EventRegistrar.Subscribe<RequestHumanPlayersDataEvent>(OnRequestHumanPlayersDataEvent);
            EventRegistrar.Subscribe<RequestComputerPlayersDataEvent>(OnRequestComputerPlayersDataEvent);
            EventRegistrar.Subscribe<RequestActivePlayersEvent>(OnRequestActivePlayersEvent);

            base.SubscribeToEvents();
        }



        public async UniTask<bool> ResetForNewRound()
        {



            if (Application.isEditor && GameSettings.Instance.DevModeEnabled)
            {
#if UNITY_EDITOR

                // todo implement way to store player and dev mode card via event susbcription, 
                foreach (IPlayerBase playerBase in NetworkPlayers.Values)
                {
                    //todo  send Hand instead as Event
                }

                // NetworkDeckManager.RemoveCardsFromDeck();

#endif
            }



            await UniTask.Yield();
            return true;
        }



        private UniTask OnGetLocalPlayer(GetLocalPlayerEvent arg)
        {
            arg.PlayerDataSource.TrySetResult(LocalNetworkHumanPlayer != null
                ? new OperationResult<IHumanPlayerData>(true, LocalNetworkHumanPlayer)
                : new OperationResult<IHumanPlayerData>(false, null));
            return UniTask.CompletedTask;
        }

        private async UniTask OnRegisterPlayer(RegisterHumanPlayerEvent e)
        {
            GameLoggerScriptable.Log($"OnRegisterPlayer called for Player ID: {e.HumanPlayerData.PlayerId}", this, ToEditor, ToFile, UseStackTrace);
            IHumanPlayerData humanPlayer = e.HumanPlayerData;
            if (AuthenticationService.Instance.PlayerId == humanPlayer.AuthenticatedPlayerId.Value.Value)
            {
                LocalNetworkHumanPlayer = humanPlayer;
            }

            if (IsServer)
            {

                if (!TryGetMatchingPlayer(humanPlayer, out IPlayerBase _))
                {
                    int playerIndex = GetNextAvailableIndex();
                    humanPlayer.SetPlayerIndex(playerIndex);

                    RegisterPlayer(humanPlayer);
                    GameLoggerScriptable.Log($"New player registered - PlayerIndex: {playerIndex}, PlayerId: {humanPlayer.PlayerId}", this, ToEditor, ToFile, UseStackTrace);

                }
                else
                {
                    GameLoggerScriptable.Log($"Player already exists - PlayerId: {humanPlayer.PlayerId}", this, ToEditor, ToFile, UseStackTrace);

                }

            }

            bool allPlayersRegisteredAndSendDataAsync = await EnsureAllPlayersRegisteredAndSendDataAsync();
            e.PlayerDataSource.TrySetResult((allPlayersRegisteredAndSendDataAsync, humanPlayer));

            await UniTask.Yield();

        }

        private async UniTask OnUnregisterPlayer(UnRegisterPlayerEvent e)
        {
            GameLoggerScriptable.Log($"OnUnregisterPlayer called for Player ID: {e.HumanPlayerData.PlayerId}", this, ToEditor, ToFile, UseStackTrace);
            IHumanPlayerData humanPlayer = e.HumanPlayerData;

            if (IsServer)
            {
                if (TryGetMatchingPlayer(humanPlayer, out IPlayerBase existingPlayer))
                {
                    if (humanPlayer.PlayerIndex.Value >= 0 && UnregisterPlayer(existingPlayer))
                    {
                        GameLoggerScriptable.Log($"Player unregistered - PlayerIndex: {humanPlayer.PlayerIndex.Value}, PlayerId: {humanPlayer.PlayerId}", this, ToEditor, ToFile, UseStackTrace);
                    }

                }
            }

            await UniTask.Yield();
        }

        public void RegisterPlayer(IPlayerBase player)
        {

            if (!NetworkPlayers.ContainsKey(player.PlayerIndex.Value))
            {
                NetworkPlayers.TryAdd(player.PlayerIndex.Value, player);
            }
            else
            {
                NetworkPlayers[player.PlayerIndex.Value] = player;
            }

            if (player is IHumanPlayerData humanPlayer)
            {
                if (!HumanPlayers.Contains(humanPlayer))
                {
                    HumanPlayers.Add(humanPlayer);
                }
            }
            else if (player is IComputerPlayerData computerPlayer)
            {
                if (!ComputerPlayers.Contains(computerPlayer))
                {
                    ComputerPlayers.Add(computerPlayer);
                }
            }

            if (!AllPlayers.Contains(player))
            {
                AllPlayers.Add(player);
            }

            player.RegisterPlayer(this, player.PlayerName.Value.Value);
        }

        public bool UnregisterPlayer(IPlayerBase player)
        {
            bool removed = NetworkPlayers.TryRemove(player.PlayerIndex.Value, out _);

            if (removed)
            {
                if (player is IHumanPlayerData humanPlayer && HumanPlayers.Contains(humanPlayer))
                {
                    HumanPlayers.Remove(humanPlayer);
                }

                if (player is IComputerPlayerData computerPlayer && ComputerPlayers.Contains(computerPlayer))
                {
                    ComputerPlayers.Remove(computerPlayer);
                }

                if (AllPlayers.Contains(player))
                {
                    AllPlayers.Remove(player);
                }
            }

            return removed;
        }


        private bool TryGetMatchingPlayer(IHumanPlayerData humanPlayer, out IPlayerBase matchingPlayer)
        {
            matchingPlayer = null;

            foreach (IPlayerBase existingPlayer in NetworkPlayers.Values)
            {
                if (existingPlayer is IHumanPlayerData playerData)
                {
                    if (playerData.AuthenticatedPlayerId == humanPlayer.AuthenticatedPlayerId)
                    {
                        matchingPlayer = existingPlayer;
                        return true;
                    }
                }
            }

            return false;
        }

        private async UniTask<bool> EnsureAllPlayersRegisteredAndSendDataAsync()
        {

            if (IsServer && !AreAllPlayersSynced)
            {
                GameLoggerScriptable.Log("Waiting for all players to be registered", this, ToEditor, ToFile, UseStackTrace);
                await UniTask.WaitUntil(() => GetAllHumanPlayers().Count == LobbyPlayerList.Count);

                GameLoggerScriptable.Log("All players are registered. Sending data to clients", this, ToEditor, ToFile, UseStackTrace);

                foreach (string aiName in ComputerPlayerList)
                {

                    SpawnAndRegisterAIPlayer(aiName);
                }

                await UniTask.WaitUntil(() => GetAllPlayers().Count == LobbyPlayerList.Count + ComputerPlayerList.Count);


                GameLoggerScriptable.Log("Preparing to send player data to clients", this, ToEditor, ToFile, UseStackTrace);

                SendPlayerDataToClientsServerRpc();

                await UniTask.WaitUntil(() => AreAllPlayersSynced);


                if (!StartTurnManagerEventPublished)
                {
                    StartTurnManagerEventPublished = await EventBus.Instance.PublishAsync(new StartTurnManagerEvent(this));
                    return true;
                }

            }

            return false;
        }

        [ServerRpc]
        private void SendPlayerDataToClientsServerRpc()
        {
            if (IsServer)
            {
                GameLoggerScriptable.Log("Sending player data to clients via ClientRpc", this, ToEditor, ToFile, UseStackTrace);
                UpdateClientPlayerDataClientRpc();
            }
        }

        [ClientRpc]
        private void UpdateClientPlayerDataClientRpc()
        {

            bool spawned = false;
            while (!spawned)
            {
                int playerDataCount = 0;

                foreach (NetworkObject networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
                {
                    if (networkObject.GetComponent<IPlayerBase>() != null)
                    {
                        playerDataCount++;
                    }
                }

                if (playerDataCount >= LobbyPlayerList.Count)
                {
                    spawned = true;
                }
            }


            // GameLoggerScriptable.Log($"Received client player data update playerDataList {playerDataList.Length} SpawnedObjects {NetworkManager.Singleton.SpawnManager.SpawnedObjects.Count} ", this);

            foreach (NetworkObject spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
            {
                IPlayerBase playerBase = spawnedObject.GetComponent<IPlayerBase>();
                if (playerBase != null)
                {
                    RegisterPlayer(playerBase);
                    if (playerBase is IHumanPlayerData playerData)
                    {
                        if (AuthenticationService.Instance.PlayerId == playerData.AuthenticatedPlayerId.Value.Value)
                        {
                            LocalNetworkHumanPlayer = playerData;
                        }
                    }
                }
            }

            AreAllPlayersSynced = GetAllPlayers().Count == LobbyPlayerList.Count + ComputerPlayerList.Count;

            EventBus.Instance.Publish(new RegisterPlayerListEvent(NetworkPlayers.Values));

            GameLoggerScriptable.Log($"All players synced: {AreAllPlayersSynced}", this, ToEditor, ToFile, UseStackTrace);


        }


        private async UniTask OnRequestActivePlayersEvent(RequestActivePlayersEvent e)
        {
            IReadOnlyList<IPlayerBase> activePlayers = GetActivePlayers();
            e.PlayerDataSource.TrySetResult(activePlayers is { Count: > 0 } ? (true, activePlayers) : (false, null));
            await UniTask.Yield();
        }
        private async UniTask OnRequestComputerPlayersDataEvent(RequestComputerPlayersDataEvent e)
        {
            e.PlayerDataSource.TrySetResult(ComputerPlayers is { Count: > 0 } ? (true, ComputerPlayers.AsReadOnly()) : (false, null));
            await UniTask.Yield();
        }

        private async UniTask OnRequestHumanPlayersDataEvent(RequestHumanPlayersDataEvent e)
        {
            e.PlayerDataSource.TrySetResult(HumanPlayers is { Count: > 0 } ? (true, HumanPlayers.AsReadOnly()) : (false, null));
            await UniTask.Yield();
        }

        private async UniTask OnRequestAllPlayersDataEvent(RequestAllPlayersDataEvent e)
        {
            e.PlayerDataSource.TrySetResult(AllPlayers is { Count: > 0 } ? (true, AllPlayers.AsReadOnly()) : (false, null));
            await UniTask.Yield();
        }


        public IReadOnlyList<IHumanPlayerData> GetAllHumanPlayers()
        {
            return HumanPlayers.AsReadOnly();
        }

        public IReadOnlyList<IComputerPlayerData> GetAllComputerPlayers()
        {
            return ComputerPlayers.AsReadOnly();
        }

        public IReadOnlyList<IPlayerBase> GetAllPlayers()
        {
            return AllPlayers.AsReadOnly();
        }

        public bool TryGetPlayer(ulong playerId, out IPlayerBase playerBase)
        {
            GameLoggerScriptable.Log($"Fetching player at index {playerId}", this, ToEditor, ToFile, UseStackTrace);

            playerBase = default;

            foreach (IPlayerBase player in NetworkPlayers.Values)
            {
                if (player.PlayerId.Value == playerId)
                {
                    playerBase = player;
                    return true;
                }
            }

            return false; // Player not found
        }

        public bool TryGetPlayerHand(ulong playerId, out IPlayerBase playerBase)
        {

            playerBase = default;

            foreach (IPlayerBase player in NetworkPlayers.Values)
            {
                if (player.PlayerId.Value == playerId)
                {
                    playerBase = player;
                  break;
                }
            }
            

            return false; // Player not found
        }

        public IReadOnlyList<IPlayerBase> GetActivePlayers()
        {
            List<IPlayerBase> activePlayers = new List<IPlayerBase>();

            if (IsServer)
            {
                foreach (KeyValuePair<int, IPlayerBase> player in NetworkPlayers)
                {
                    if (!FoldedPlayers.Contains(player.Value))
                    {
                        activePlayers.Add(player.Value);
                    }
                }

            }


            return activePlayers;
        }


        public void AddFoldedPlayer(IPlayerBase player)
        {
            if (IsServer)
            {
                player.HasFolded.Value = true;
                FoldedPlayers.Add(player);
            }

        }

        public void ResetFoldedPlayer(bool forNewRound = true)
        {
            if (IsServer)
            {
                FoldedPlayers.Clear();
                if (forNewRound)
                {
                    foreach (IPlayerBase player in NetworkPlayers.Values)
                    {
                        if (player.IsBankrupt.Value)
                        {
                            FoldedPlayers.Add(player);
                        }
                    }
                }

            }
        }

        private async UniTask OnRequestLobbyPlayerData(RequestLobbyPlayerDataEvent request)
        {
            try
            {
                GameLoggerScriptable.Log($"Processing lobby player data request for Player ID: {request.PlayerId}", this, ToEditor, ToFile, UseStackTrace);

                if (LobbyPlayerList is { Count: > 0 })
                {
                    Player foundPlayer = null;

                    foreach (Player player in LobbyPlayerList)
                    {
                        if (player.Data != null &&
                            player.Data.TryGetValue("PlayerId", out PlayerDataObject playerData) &&
                            playerData.Value == request.PlayerId)
                        {
                            foundPlayer = player;
                            break;
                        }
                    }

                    if (foundPlayer != null)
                    {
                        request.PlayerDataSource.TrySetResult((true, foundPlayer));
                    }
                    else
                    {
                        request.PlayerDataSource.TrySetResult((false, null));
                    }
                }
                else
                {
                    request.PlayerDataSource.TrySetResult((false, null));
                }

                await UniTask.Yield();
            }
            catch (LobbyServiceException ex)
            {
                GameLoggerScriptable.LogError($"Failed to fetch lobby data: {ex.Message} {ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                request.PlayerDataSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Unexpected error while fetching lobby data: {ex.Message} {ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                request.PlayerDataSource.TrySetException(ex);
            }
        }

        public void Init(List<Player> playerList, List<string> computerPlayerList)
        {

            LobbyPlayerList = playerList;
            ComputerPlayerList = computerPlayerList;

        }

        private void SpawnAndRegisterAIPlayer(string computerPlayerName)
        {
            if (!IsServer)
            {
                return;
            }

            foreach (IPlayerBase playerBase in NetworkPlayers.Values)
            {
                if (playerBase is IComputerPlayerData computerPlayerData)
                {
                    if (computerPlayerData.PlayerName.Value.Value == computerPlayerName)
                    {
                        return;
                    }
                }
            }

            GameObject computerPlayerObject = Instantiate(ComputerPlayerPrefab);
            NetworkObject networkObject = computerPlayerObject.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                GameLoggerScriptable.LogError("AI Player prefab is missing a NetworkObject component.", this);
                return;
            }

            networkObject.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);

            IComputerPlayerData computerPlayer = computerPlayerObject.GetComponent<IComputerPlayerData>();

            if (computerPlayer != null)
            {
                int playerIndex = GetNextAvailableIndex();
                computerPlayer.SetPlayerIndex(playerIndex);
                computerPlayer.PlayerName.Value = computerPlayerName;
                computerPlayer.PlayerId.Value = GenerateUniquePlayerId();
                computerPlayer.DifficultyLevel = 0;
                computerPlayer.AIModelName = computerPlayerName;

                RegisterPlayer(computerPlayer);
                GameLoggerScriptable.Log($"AI Player registered: {computerPlayer.PlayerName.Value.Value} (Index: {playerIndex})", this);
            }
        }
        private ulong GenerateUniquePlayerId()
        {
            ulong baseId = (ulong)DateTime.UtcNow.Ticks;
            while (NetworkPlayers.Values.Any(player => player.PlayerId.Value == baseId))
            {
                baseId++; // Ensure no collisions
            }
            return baseId;
        }

        private int GetNextAvailableIndex()
        {
            int index = 0;
            while (NetworkPlayers.ContainsKey(index))
            {
                index++;
            }
            return index;
        }


        public void ShowHand(bool showHand, bool isRoundEnd = false)
        {
            foreach (IPlayerBase player in NetworkPlayers.Values)
            {
                EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent(player, isRoundEnd));
            }
        }
    }
}
