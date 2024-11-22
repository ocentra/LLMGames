using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    public class PlayerManager : NetworkBehaviour
    {
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<int, IPlayerData> NetworkPlayers { get; } = new ConcurrentDictionary<int, IPlayerData>();

        [ShowInInspector] public IPlayerData LocalNetworkPlayer { get; set; }

        [ShowInInspector] private List<Player> PlayerList { get; set; }

        public bool EditorLog = true;

        void Awake()
        {
            SubscribeToEvents();
            DontDestroyOnLoad(this);
        }

        public override void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        public void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<RegisterPlayerEvent>(OnRegisterPlayer);
            EventBus.Instance.SubscribeAsync<UnRegisterPlayerEvent>(OnUnregisterPlayer);
            EventBus.Instance.SubscribeAsync<RequestLobbyPlayerDataEvent>(OnRequestLobbyPlayerData);
            EventBus.Instance.SubscribeAsync<GetLocalPlayer>(OnGetLocalPlayer);
        }



        public void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<RegisterPlayerEvent>(OnRegisterPlayer);
            EventBus.Instance.UnsubscribeAsync<UnRegisterPlayerEvent>(OnUnregisterPlayer);
            EventBus.Instance.UnsubscribeAsync<RequestLobbyPlayerDataEvent>(OnRequestLobbyPlayerData);
            EventBus.Instance.UnsubscribeAsync<GetLocalPlayer>(OnGetLocalPlayer);
        }

        private UniTask OnGetLocalPlayer(GetLocalPlayer arg)
        {
            arg.PlayerDataSource.TrySetResult(LocalNetworkPlayer != null
                ? new OperationResult<IPlayerData>(true, LocalNetworkPlayer)
                : new OperationResult<IPlayerData>(false, null));
            return UniTask.CompletedTask;
        }

        private async UniTask OnRegisterPlayer(RegisterPlayerEvent e)
        {
            GameLoggerScriptable.Log($"OnRegisterPlayer called for Player ID: {e.PlayerData.OwnerClientId}", this);
            IPlayerData player = e.PlayerData;

            if (AuthenticationService.Instance.PlayerId == player.AuthenticatedPlayerId.Value.Value)
            {
                LocalNetworkPlayer = player;
            }


            if (IsServer)
            {
                lock (NetworkPlayers)
                {
                    bool playerExists = NetworkPlayers.Values.Any(existingPlayer => existingPlayer.AuthenticatedPlayerId == player.AuthenticatedPlayerId);

                    if (!playerExists)
                    {
                        int playerIndex = NetworkPlayers.Count;
                        player.SetPlayerIndex(playerIndex);
                        NetworkPlayers[playerIndex] = player;
                        GameLoggerScriptable.Log($"New player registered - PlayerIndex: {playerIndex}, OwnerClientId: {player.OwnerClientId}", this);
                        e.PlayerDataSource.TrySetResult(player);
                    }
                    else
                    {
                        GameLoggerScriptable.Log($"Player already exists - OwnerClientId: {player.OwnerClientId}", this);
                        e.PlayerDataSource.TrySetResult(player);
                    }
                }
            }

            await EnsureAllPlayersRegisteredAndSendDataAsync();
        }

        private async UniTask OnUnregisterPlayer(UnRegisterPlayerEvent e)
        {
            GameLoggerScriptable.Log($"OnUnregisterPlayer called for Player ID: {e.PlayerData.OwnerClientId}", this);
            IPlayerData player = e.PlayerData;

            if (IsServer)
            {
                lock (NetworkPlayers)
                {
                    if (player.PlayerIndex.Value >= 0 &&
                        NetworkPlayers.TryGetValue(player.PlayerIndex.Value, out IPlayerData existingPlayer) &&
                        existingPlayer.AuthenticatedPlayerId == player.AuthenticatedPlayerId)
                    {
                        NetworkPlayers.TryRemove(player.PlayerIndex.Value, out _);
                        GameLoggerScriptable.Log($"Player unregistered - PlayerIndex: {player.PlayerIndex.Value}, OwnerClientId: {player.OwnerClientId}", this);
                    }
                }

                GameLoggerScriptable.Log("Sending updated player data to clients after unregistration", this);
                SendPlayerDataToClientsServerRpc();
            }

            await UniTask.Yield();
        }

        private async UniTask<bool> EnsureAllPlayersRegisteredAndSendDataAsync()
        {
            if (IsServer)
            {
                GameLoggerScriptable.Log("Waiting for all players to be registered", this);
                await UniTask.WaitUntil(() => NetworkPlayers.Count == PlayerList.Count);

                GameLoggerScriptable.Log("All players are registered. Sending data to clients", this);
                SendPlayerDataToClientsServerRpc();
            }

            return true;
        }

        [ServerRpc]
        private void SendPlayerDataToClientsServerRpc()
        {
            if (IsServer)
            {
                GameLoggerScriptable.Log("Preparing to send player data to clients", this);
                List<PlayerDataDTO> playerDataList = new List<PlayerDataDTO>();

                foreach (KeyValuePair<int, IPlayerData> entry in NetworkPlayers)
                {
                    GameLoggerScriptable.Log($"Adding player data - PlayerIndex: {entry.Key}, OwnerClientId: {entry.Value.OwnerClientId}", this);
                    playerDataList.Add(new PlayerDataDTO(entry.Key, entry.Value.OwnerClientId));
                }

                GameLoggerScriptable.Log("Sending player data to clients via ClientRpc", this);
                UpdateClientPlayerDataClientRpc(playerDataList.ToArray());
            }
        }

        [ClientRpc]
        private void UpdateClientPlayerDataClientRpc(PlayerDataDTO[] playerDataList)
        {
            if (IsClient && !IsHost)
            {
                GameLoggerScriptable.Log("Received client player data update", this);
                foreach (PlayerDataDTO data in playerDataList)
                {
                        GameLoggerScriptable.Log($"Processing player data - PlayerIndex: {data.PlayerIndex}, OwnerClientId: {data.OwnerClientId}", this);

                    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(data.OwnerClientId, out NetworkObject networkObject))
                    {
                        IPlayerData networkPlayer = networkObject.GetComponent<IPlayerData>();
                        if (networkPlayer != null)
                        {
                            if (!NetworkPlayers.ContainsKey(data.PlayerIndex))
                            {
                                GameLoggerScriptable.Log($"Adding player to NetworkPlayers - PlayerIndex: {data.PlayerIndex}", this);
                                NetworkPlayers.TryAdd(data.PlayerIndex, networkPlayer);
                            }
                            else
                            {
                                GameLoggerScriptable.Log($"Updating existing player in NetworkPlayers - PlayerIndex: {data.PlayerIndex}", this);
                                NetworkPlayers[data.PlayerIndex] = networkPlayer;
                            }
                        }
                        else
                        {
                            GameLoggerScriptable.LogWarning($"NetworkObject for OwnerClientId {data.OwnerClientId} does not have an IPlayerData component", this);
                        }
                    }
                    else
                    {
                        GameLoggerScriptable.LogWarning($"Could not find NetworkObject for OwnerClientId: {data.OwnerClientId}", this);
                    }
                }
            }
        }

        public IReadOnlyList<IPlayerData> GetAllPlayers()
        {
            GameLoggerScriptable.Log("Fetching all registered players", this);
            return new List<IPlayerData>(NetworkPlayers.Values);
        }

        private async UniTask OnRequestLobbyPlayerData(RequestLobbyPlayerDataEvent request)
        {
            try
            {
                GameLoggerScriptable.Log($"Processing lobby player data request for Player ID: {request.PlayerId}", this);
                if (PlayerList is { Count: > 0 })
                {
                    Player foundPlayer = PlayerList.FirstOrDefault(player => player.Data != null &&
                        player.Data.TryGetValue("PlayerId", out PlayerDataObject playerData) &&
                        playerData.Value == request.PlayerId);

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
                GameLoggerScriptable.LogError($"Failed to fetch lobby data: {ex.Message} {ex.StackTrace}", this);
                request.PlayerDataSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Unexpected error while fetching lobby data: {ex.Message} {ex.StackTrace}", this);
                request.PlayerDataSource.TrySetException(ex);
            }
        }



        public void Init(List<Player> playerList)
        {
            PlayerList = playerList;
        }

      
    }
}
