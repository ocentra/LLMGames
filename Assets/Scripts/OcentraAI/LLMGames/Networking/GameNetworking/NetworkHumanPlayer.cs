using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using static System.String;

namespace OcentraAI.LLMGames.GamesNetworking
{
    public class NetworkHumanHumanPlayer : NetworkPlayer, IHumanPlayerData
    {

        [ShowInInspector] public Player LobbyPlayerData { get; private set; }
        [ShowInInspector] private PlayerViewer PlayerViewer { get; set; } = null;
        [ShowInInspector] public string AuthenticatedPlayerIdViewer { get; private set; }

        public override async void OnNetworkSpawn()
        {

            base.OnNetworkSpawn();

            GameLoggerScriptable.Log($"Setting AuthenticatedPlayerId.Value For {PlayerId.Value}", this);

            while (AuthenticatedPlayerId.Value.IsEmpty)
            {
                GameLoggerScriptable.Log($"Stuck in setting AuthenticatedPlayerId.Value For {PlayerId}", this);
                if (IsOwner)
                {
                    AuthenticatedPlayerId.Value = AuthenticationService.Instance.PlayerId;
                }


                await UniTask.WaitForSeconds(1);
            }



            GameLoggerScriptable.Log($" AuthenticatedPlayerId.Value For {PlayerId} Value {AuthenticatedPlayerId.Value.Value}", this);


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
                    playerName = $"Player_{PlayerId.Value} {dataObject.Value}";
                }

                if (IsNullOrEmpty(playerName))
                {
                    if (LobbyPlayerData.Profile != null)
                    {
                        playerName = !IsNullOrEmpty(LobbyPlayerData.Profile.Name) ? $"Player_{PlayerId.Value} {LobbyPlayerData.Profile.Name}" : $"Player_{PlayerId.Value} LobbyPlayerData Profile Name Null";
                    }
                    else
                    {
                        playerName = $"Player_{PlayerId.Value} LobbyPlayerData Null ";
                        GameLoggerScriptable.Log($"LobbyPlayerData.Profile is null playerName is {playerName}", this);
                    }
                }



                PlayerViewer = new PlayerViewer(LobbyPlayerData);
            }

            while (PlayerName.Value.IsEmpty)
            {
                GameLoggerScriptable.Log($"Stuck In while loop setting PlayerName.Value For {PlayerId.Value}  @ OnNetworkSpawn", this);

                if (IsServer)
                {
                    PlayerName.Value = playerName;
                }


                await UniTask.WaitForSeconds(1);
            }

            if (IsServer && !IsPlayerRegistered.Value)
            {
                UniTaskCompletionSource<(bool success, IHumanPlayerData player)> completionSource = new UniTaskCompletionSource<(bool success, IHumanPlayerData player)>();
                await EventBus.Instance.PublishAsync(new RegisterHumanPlayerEvent(this, completionSource));
                (bool success, IHumanPlayerData player) registerPlayerEventResult = await completionSource.Task;
                IsPlayerRegistered.Value = registerPlayerEventResult.success;

            }


            GameLoggerScriptable.Log($"PlayerName.Value For {PlayerId.Value} @  OnNetworkSpawn Value {PlayerName.Value.Value}", this);

            SubscribeToEvents();


        }



        public override async void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                await EventBus.Instance.PublishAsync(new UnRegisterPlayerEvent(this));
            }

            GameLoggerScriptable.Log($"Player {PlayerName.Value} with Client ID {PlayerId.Value} despawned", this);
            UnsubscribeFromEvents();
        }


    }
}
