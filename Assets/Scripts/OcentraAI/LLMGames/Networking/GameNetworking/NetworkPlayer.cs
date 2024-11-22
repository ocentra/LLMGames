using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Networking.Manager;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using static System.String;

namespace OcentraAI.LLMGames.GamesNetworking
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerData
    {
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public NetworkVariable<int> PlayerIndex { get; private set; } = new NetworkVariable<int>();

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        public NetworkVariable<FixedString64Bytes> AuthenticatedPlayerId { get; private set; } = new NetworkVariable<FixedString64Bytes>(
                new FixedString64Bytes(),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public NetworkVariable<FixedString64Bytes> PlayerName { get; private set; } = new NetworkVariable<FixedString64Bytes>();

        [ShowInInspector] public bool IsPlayerRegistered { get; set; } = false;
        [ShowInInspector] public bool NameSet { get; set; } = false;
        public Player LobbyPlayerData { get; private set; }
        [ShowInInspector] private PlayerViewer PlayerViewer { get; set; } = null;

        // Temporary public field to show the time taken in seconds for syncing
        [ShowInInspector] public float TimeTakenToSyncAuthId { get; private set; }
        [ShowInInspector] public string AuthenticatedPlayerIdViewer { get; private set; }

        public override async void OnNetworkSpawn()
        {

            base.OnNetworkSpawn();


            GameLoggerScriptable.Log($"Setting AuthenticatedPlayerId.Value For {OwnerClientId}", this);

            PlayerManager findAnyObjectByType = null;
            while (findAnyObjectByType == null)
            {
                GameLoggerScriptable.Log($"Stuck in setting AuthenticatedPlayerId.Value For {OwnerClientId}", this);

                findAnyObjectByType = FindAnyObjectByType<PlayerManager>();
                await UniTask.DelayFrame(10);
            }

            DateTime startTime = DateTime.Now;




            while (AuthenticatedPlayerId.Value.IsEmpty)
            {
                GameLoggerScriptable.Log($"Stuck in setting AuthenticatedPlayerId.Value For {OwnerClientId}", this);
                if (IsOwner)
                {
                    AuthenticatedPlayerId.Value = AuthenticationService.Instance.PlayerId;
                }


                await UniTask.WaitForSeconds(1);
            }

            GameLoggerScriptable.Log($" AuthenticatedPlayerId.Value For {OwnerClientId} Value {AuthenticatedPlayerId.Value.Value}", this);


            AuthenticatedPlayerIdViewer = AuthenticatedPlayerId.Value.Value;

            UniTaskCompletionSource<(bool success, Player player)> lobbyDataSource = new UniTaskCompletionSource<(bool success, Player player)>();
            await EventBus.Instance.PublishAsync(new RequestLobbyPlayerDataEvent(lobbyDataSource, AuthenticatedPlayerId.Value.Value));
            (bool success, Player player) result = await lobbyDataSource.Task;

            LobbyPlayerData = result.player;

            string playerName = Empty;

            if (result.player != null)
            {
                LobbyPlayerData = result.player;

                if (LobbyPlayerData.Data.TryGetValue(nameof(PlayerName), out PlayerDataObject dataObject) && !IsNullOrEmpty(dataObject.Value))
                {
                    playerName = $"Player_{OwnerClientId} {dataObject.Value}";
                }

                if (IsNullOrEmpty(playerName))
                {
                    if (LobbyPlayerData.Profile != null)
                    {
                        playerName = !IsNullOrEmpty(LobbyPlayerData.Profile.Name) ? $"Player_{OwnerClientId} {LobbyPlayerData.Profile.Name}" : $"Player_{OwnerClientId} LobbyPlayerData Profile Name Null";
                    }
                    else
                    {
                        playerName = $"Player_{OwnerClientId} LobbyPlayerData Null ";
                        GameLoggerScriptable.Log($"LobbyPlayerData.Profile is null playerName is {playerName}", this);
                    }
                }



                PlayerViewer = new PlayerViewer(LobbyPlayerData);
            }

            while (PlayerName.Value.IsEmpty)
            {
                GameLoggerScriptable.Log($"Stuck In while loop setting PlayerName.Value For {OwnerClientId}  @ OnNetworkSpawn", this);

                if (IsServer)
                {
                    PlayerName.Value = playerName;
                }


                await UniTask.WaitForSeconds(1);
            }

            if (IsServer)
            {
                if (IsPlayerRegistered) return;

                UniTaskCompletionSource<IPlayerData> completionSource = new UniTaskCompletionSource<IPlayerData>();
                await EventBus.Instance.PublishAsync(new RegisterPlayerEvent(this, completionSource));
                IPlayerData playerData = await completionSource.Task;

                if (playerData != null)
                {
                    PlayerIndex = playerData.PlayerIndex;
                }

                IsPlayerRegistered = true;
            }


            GameLoggerScriptable.Log($"PlayerName.Value For {OwnerClientId} @  OnNetworkSpawn Value {PlayerName.Value.Value}", this);


            gameObject.name = PlayerName.Value.Value;
            TimeSpan timeTaken = DateTime.Now - startTime;
            TimeTakenToSyncAuthId = (float)timeTaken.TotalSeconds;

        }


        public void SetPlayerIndex(int index)
        {

            if (IsServer)
            {
                PlayerIndex.Value = index;
            }
        }

        public override async void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                await EventBus.Instance.PublishAsync(new UnRegisterPlayerEvent(this));
            }

            GameLoggerScriptable.Log($"Player {PlayerName.Value} with Client ID {OwnerClientId} despawned", this);
        }
    }
}
