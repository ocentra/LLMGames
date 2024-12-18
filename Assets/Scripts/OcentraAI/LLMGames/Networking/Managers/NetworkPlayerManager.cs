using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        [ShowInInspector] public IPlayerData LocalNetworkPlayer { get; set; }
        [ShowInInspector] private List<Player> LobbyPlayerList { get; set; }
        [ShowInInspector] private List<string> ComputerPlayerList { get; set; }

        [ShowInInspector] public bool AreAllPlayersSynced { get; set; } = false;

        [ShowInInspector][ReadOnly] protected readonly HashSet<IPlayerBase> FoldedPlayers = new HashSet<IPlayerBase>();

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
            EventBus.Instance.SubscribeAsync<RegisterPlayerEvent>(OnRegisterPlayer);
            EventBus.Instance.SubscribeAsync<UnRegisterPlayerEvent>(OnUnregisterPlayer);
            EventBus.Instance.SubscribeAsync<RequestLobbyPlayerDataEvent>(OnRequestLobbyPlayerData);
            EventBus.Instance.SubscribeAsync<GetLocalPlayerEvent>(OnGetLocalPlayer);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<RegisterPlayerEvent>(OnRegisterPlayer);
            EventBus.Instance.UnsubscribeAsync<UnRegisterPlayerEvent>(OnUnregisterPlayer);
            EventBus.Instance.UnsubscribeAsync<RequestLobbyPlayerDataEvent>(OnRequestLobbyPlayerData);
            EventBus.Instance.UnsubscribeAsync<GetLocalPlayerEvent>(OnGetLocalPlayer);
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
            arg.PlayerDataSource.TrySetResult(LocalNetworkPlayer != null
                ? new OperationResult<IPlayerData>(true, LocalNetworkPlayer)
                : new OperationResult<IPlayerData>(false, null));
            return UniTask.CompletedTask;
        }

        private async UniTask OnRegisterPlayer(RegisterPlayerEvent e)
        {
            GameLoggerScriptable.Log($"OnRegisterPlayer called for Player ID: {e.PlayerData.PlayerId}", this, ToEditor, ToFile, UseStackTrace);
            IPlayerData player = e.PlayerData;
            if (AuthenticationService.Instance.PlayerId == player.AuthenticatedPlayerId.Value.Value)
            {
                LocalNetworkPlayer = player;
            }

            if (IsServer)
            {

                if (!TryGetMatchingPlayer(player, out IPlayerBase _))
                {
                    int playerIndex = GetNextAvailableIndex();
                    player.SetPlayerIndex(playerIndex);

                    RegisterPlayer(player);
                    GameLoggerScriptable.Log($"New player registered - PlayerIndex: {playerIndex}, PlayerId: {player.PlayerId}", this, ToEditor, ToFile, UseStackTrace);

                }
                else
                {
                    GameLoggerScriptable.Log($"Player already exists - PlayerId: {player.PlayerId}", this, ToEditor, ToFile, UseStackTrace);

                }

            }

            bool allPlayersRegisteredAndSendDataAsync = await EnsureAllPlayersRegisteredAndSendDataAsync();
            e.PlayerDataSource.TrySetResult((allPlayersRegisteredAndSendDataAsync, player));

            await UniTask.Yield();

        }

        private async UniTask OnUnregisterPlayer(UnRegisterPlayerEvent e)
        {
            GameLoggerScriptable.Log($"OnUnregisterPlayer called for Player ID: {e.PlayerData.PlayerId}", this, ToEditor, ToFile, UseStackTrace);
            IPlayerData player = e.PlayerData;

            if (IsServer)
            {
                if (TryGetMatchingPlayer(player, out IPlayerBase existingPlayer))
                {
                    if (player.PlayerIndex.Value >= 0 && UnregisterPlayer(existingPlayer))
                    {
                        GameLoggerScriptable.Log($"Player unregistered - PlayerIndex: {player.PlayerIndex.Value}, PlayerId: {player.PlayerId}", this, ToEditor, ToFile, UseStackTrace);
                    }
                    
                }
            }

            await UniTask.Yield();
        }

        public void RegisterPlayer(IPlayerBase player)
        {
            if (player.PlayerManager == null)
            {
                player.PlayerManager = this;
            }

            player.GameObject.name = player.PlayerName.Value.Value;


            if (!NetworkPlayers.ContainsKey(player.PlayerIndex.Value))
            {
                NetworkPlayers.TryAdd(player.PlayerIndex.Value, player);
            }
            else
            {
                NetworkPlayers[player.PlayerIndex.Value] = player;
            }

            player.SetPlayerRegistered(true);
        }

        public bool UnregisterPlayer(IPlayerBase player)
        {
            return NetworkPlayers.TryRemove(player.PlayerIndex.Value, out _);
        }


        private bool TryGetMatchingPlayer(IPlayerData player, out IPlayerBase matchingPlayer)
        {
            matchingPlayer = null;

            foreach (IPlayerBase existingPlayer in NetworkPlayers.Values)
            {
                if (existingPlayer is IPlayerData playerData)
                {
                    if (playerData.AuthenticatedPlayerId == player.AuthenticatedPlayerId)
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
              
                if (AreAllPlayersSynced)
                {
                    EventBus.Instance.Publish(new StartTurnManagerEvent(this));
                }
              
            }

            return true;
        }

        [ServerRpc]
        private void SendPlayerDataToClientsServerRpc()
        {
            if (IsServer)
            {
                GameLoggerScriptable.Log("Sending player data to clients via ClientRpc", this, ToEditor, ToFile, UseStackTrace);
                UpdateClientPlayerDataClientRpc();
                EventBus.Instance.Publish(new RegisterUIPlayerEvent(NetworkPlayers.Values));
                AreAllPlayersSynced = true;
               
            }
        }

        [ClientRpc]
        private void UpdateClientPlayerDataClientRpc()
        {


            if (IsClient && !IsHost)
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
                        if (playerBase is IPlayerData playerData)
                        {
                            if (AuthenticationService.Instance.PlayerId == playerData.AuthenticatedPlayerId.Value.Value)
                            {
                                LocalNetworkPlayer = playerData;
                            }
                        }
                    }
                }

                AreAllPlayersSynced = GetAllPlayers().Count == LobbyPlayerList.Count + ComputerPlayerList.Count;

                EventBus.Instance.Publish(new RegisterUIPlayerEvent(NetworkPlayers.Values));

                GameLoggerScriptable.Log($"All players synced: {AreAllPlayersSynced}", this, ToEditor, ToFile, UseStackTrace);
            }


        }


        public IReadOnlyList<IPlayerData> GetAllHumanPlayers()
        {
            GameLoggerScriptable.Log("Fetching all registered Human players", this, ToEditor, ToFile, UseStackTrace);

            List<IPlayerData> players = new List<IPlayerData>();

            foreach (IPlayerBase player in NetworkPlayers.Values)
            {
                if (player is IPlayerData playerData)
                {
                    players.Add(playerData);
                }
            }

            return players.AsReadOnly();
        }

        public IReadOnlyList<IComputerPlayerData> GetAllComputerPlayers()
        {
            GameLoggerScriptable.Log("Fetching all registered computer players", this, ToEditor, ToFile, UseStackTrace);

            List<IComputerPlayerData> players = new List<IComputerPlayerData>();

            foreach (IPlayerBase player in NetworkPlayers.Values)
            {
                if (player is IComputerPlayerData playerData)
                {
                    players.Add(playerData);
                }
            }

            return players.AsReadOnly();
        }

        public IReadOnlyList<IPlayerBase> GetAllPlayers()
        {
            GameLoggerScriptable.Log("Fetching all registered players", this, ToEditor, ToFile, UseStackTrace);

            List<IPlayerBase> players = new List<IPlayerBase>();

            foreach (IPlayerBase player in NetworkPlayers.Values)
            {
                players.Add(player);
            }

            return players.AsReadOnly();
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
                FoldedPlayers.Add(player);
            }
           
        }

        public void ResetFoldedPlayer()
        {
            if (IsServer)
            {
                FoldedPlayers.Clear();
            }
           
        }

        private async UniTask OnRequestLobbyPlayerData(RequestLobbyPlayerDataEvent request)
        {
            try
            {
                GameLoggerScriptable.Log($"Processing lobby player data request for Player ID: {request.PlayerId}", this, ToEditor, ToFile, UseStackTrace);

                if (LobbyPlayerList != null && LobbyPlayerList.Count > 0)
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
                computerPlayer.DifficultyLevel = 0;
                computerPlayer.AIModelName = computerPlayerName;
                RegisterPlayer(computerPlayer);
                GameLoggerScriptable.Log($"AI Player registered: {computerPlayer.PlayerName.Value.Value} (Index: {playerIndex})", this);
            }
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
