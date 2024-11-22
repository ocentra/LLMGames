using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static System.String;

namespace OcentraAI.LLMGames.Networking.Manager
{
    public class LobbyManager : ManagerBase<LobbyManager>
    {
        [ShowInInspector, ReadOnly] private string JoinCode { get; set; } = Empty;
        [ShowInInspector, ReadOnly] private string JoinedLobbyId { get; set; } = Empty;
        [ShowInInspector] private Player Player { get; set; }



        protected override void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<CreateProfileEvent>(OnCreateProfile);
            EventBus.Instance.SubscribeAsync<CreateLobbyEvent>(OnCreateLobby);
            EventBus.Instance.SubscribeAsync<JoinLobbyEvent>(OnJoinLobby);
            EventBus.Instance.SubscribeAsync<UpdateLobbyEvent>(OnUpdateLobby);
            EventBus.Instance.SubscribeAsync<PlayerLeftLobbyEvent>(OnPlayerLeave);
        }
        protected override void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<CreateProfileEvent>(OnCreateProfile);
            EventBus.Instance.UnsubscribeAsync<CreateLobbyEvent>(OnCreateLobby);
            EventBus.Instance.UnsubscribeAsync<JoinLobbyEvent>(OnJoinLobby);
            EventBus.Instance.UnsubscribeAsync<UpdateLobbyEvent>(OnUpdateLobby);
            EventBus.Instance.UnsubscribeAsync<PlayerLeftLobbyEvent>(OnPlayerLeave);
        }



        private async UniTask OnCreateProfile(CreateProfileEvent createProfileEvent)
        {
            PlayerDataObject playerDataObject = new(PlayerDataObject.VisibilityOptions.Public, createProfileEvent.AuthPlayerData.PlayerName);

            Player = new Player(createProfileEvent.AuthPlayerData.PlayerID, data:
                new Dictionary<string, PlayerDataObject>
                {
                    {"Name", playerDataObject}
                });


            await EventBus.Instance.PublishAsync(new ProfileCreatedEvent(Player));
        }

        public async UniTask OnCreateLobby(CreateLobbyEvent createLobby)
        {

            CreateLobbyOptions options = new()
            {

                Player = Player,
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, createLobby.Options.GameMode)},
                    {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public, Empty)}
                }

            };

            if (createLobby.Options.IsPrivate && !IsNullOrEmpty(createLobby.Options.OptionsPassword))
            {
                options.Password = createLobby.Options.OptionsPassword;
                options.IsPrivate = true;
            }

            Lobby createdLobby = null;

            try
            {
                createdLobby = await LobbyService.Instance.CreateLobbyAsync(createLobby.Options.LobbyName, createLobby.Options.MaxPlayers, options).AsUniTask();

            }
            catch (LobbyServiceException e)
            {
                LogError(e.Message, this);
            }

            if (createdLobby != null)
            {
                try
                {
                    LobbyHeartbeat(createdLobby).Forget();
                    JoinedLobbyId = createdLobby.Id;
                    await StartHost();
                    await EventBus.Instance.PublishAsync(new ShowScreenEvent("JoinedLobbyScreen"));

                }
                catch (LobbyServiceException e)
                {
                    LogError(e.Message, this);
                }
            }


        }

        private async UniTask OnJoinLobby(JoinLobbyEvent joinLobbyEvent)
        {
            try
            {
                JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions { Player = Player };

                if (joinLobbyEvent.IsProtectedLobby)
                {
                    UniTaskCompletionSource<string> passwordSetSource = new UniTaskCompletionSource<string>();
                    await EventBus.Instance.PublishAsync(new InputLobbyPasswordEvent(passwordSetSource));
                    string password = await passwordSetSource.Task;
                    joinLobbyByIdOptions.Password = password;
                }


                await LobbyService.Instance.JoinLobbyByIdAsync(joinLobbyEvent.LobbyId, joinLobbyByIdOptions).AsUniTask();

                JoinedLobbyId = joinLobbyEvent.LobbyId;
                await EventBus.Instance.PublishAsync(new ShowScreenEvent("JoinedLobbyScreen"));
                await UpdateLobbyPlayers(JoinedLobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                // Handle error (show error screen, etc.)
            }
        }

        private async UniTask OnUpdateLobby(UpdateLobbyEvent updateLobby)
        {
            CancellationToken externalCancellationToken = updateLobby.CancellationTokenSource.Token;
            int retryCount = 0;
            int baseDelay = 1000;

            while (Application.isPlaying && !externalCancellationToken.IsCancellationRequested)
            {
                using CancellationTokenSource retryCancellationTokenSource = new CancellationTokenSource();
                try
                {
                    QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions();

                    if (!IsNullOrEmpty(updateLobby.SearchLobbyName))
                    {
                        queryLobbiesOptions.Filters = new List<QueryFilter>
                        {
                            new QueryFilter(QueryFilter.FieldOptions.Name, updateLobby.SearchLobbyName, QueryFilter.OpOptions.CONTAINS)
                        };
                    }

                    QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions).AsUniTask();
                    await EventBus.Instance.PublishAsync(new UpdateLobbyListEvent(queryResponse.Results));

                    retryCount = 0;
                    await UniTask.Delay(baseDelay, cancellationToken: externalCancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (LobbyServiceException ex) when (ex.Message.Contains("Rate limit"))
                {
                    retryCount++;
                    int delay = baseDelay * retryCount;
                    LogWarning($"Rate limit exceeded. Retrying in {delay / 1000} seconds...", this);
                    await UniTask.Delay(delay, cancellationToken: retryCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    LogError($"Unexpected error in OnUpdateLobby: {ex.Message}", this);
                    await UniTask.Delay(baseDelay, cancellationToken: retryCancellationTokenSource.Token);
                }
            }
        }








        private async UniTask StartHost()
        {
            if (JoinedLobbyId != null)
            {
                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobbyId).AsUniTask();

                    //if (RelayManager.Instance != null)
                    //{
                    //    OperationResult<string> relayResult = await RelayManager.Instance.StartHostRelay(lobby.MaxPlayers);
                    //    if (!relayResult.IsSuccess)
                    //    {
                    //        LogError($"Failed to start host with relay. Attempts: {relayResult.Attempts}, Error: {relayResult.ErrorMessage}");
                    //        return;
                    //    }

                    //    JoinCode = relayResult.Value;
                    //}
                    //else
                    //{
                    //    LogError($"Error in LobbyStart: RelayManager Cannot Be Found");
                    //}
                }
                catch (Exception ex)
                {
                    LogError($"Error in LobbyStart: {ex.Message}", this);
                }


                try
                {
                    await LobbyService.Instance.UpdateLobbyAsync(JoinedLobbyId,
                        new UpdateLobbyOptions
                        {
                            Data = new Dictionary<string, DataObject>
                            {
                                {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode)}
                            }
                        }).AsUniTask();
                }
                catch (Exception updateEx)
                {
                    LogError($"Failed to update lobby with join code. Error: {updateEx.Message}", this);
                }

                await UpdateLobbyPlayers(JoinedLobbyId);
            }
            else
            {
                LogError($"Error in LobbyStart: JoinedLobbyId is null", this);
            }
        }

        private async UniTask UpdateLobbyPlayers(string joinedLobbyId)
        {
            while (Application.isPlaying)
            {
                if (IsNullOrEmpty(joinedLobbyId))
                {
                    return;
                }

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyId).AsUniTask();

                DataObject dataObject = lobby.Data["JoinCode"];
                if (dataObject != null)
                {
                    JoinCode = dataObject.Value;
                    if (!IsNullOrEmpty(JoinCode))
                    {
                        //await RelayManager.Instance.StartClientRelay(JoinCode).AsUniTask();
                        await EventBus.Instance.PublishAsync(new JoinedLobbyEvent());
                    }

                }

                await EventBus.Instance.PublishAsync(new UpdateLobbyPlayerListEvent(lobby, Player.Id == lobby.HostId));

                await UniTask.Delay(1000);
            }
        }

        private async UniTask OnPlayerLeave(PlayerLeftLobbyEvent e)
        {
            if (e.PlayerId == Player.Id && !IsNullOrEmpty(JoinedLobbyId))
            {
                try
                {
                    UniTask task = LobbyService.Instance.RemovePlayerAsync(JoinedLobbyId, Player.Id).AsUniTask();
                    await task;
                    if (task.Status == UniTaskStatus.Succeeded)
                    {
                        e.LeaveCompletionSource.TrySetResult(true);
                    }
                    JoinedLobbyId = null;
                    JoinCode = Empty;

                }
                catch (LobbyServiceException ex)
                {
                    e.LeaveCompletionSource.TrySetException(ex);
                    Debug.LogError($"Error leaving lobby: {ex.Message}");
                }
            }



        }

        protected override async UniTask<bool> ApplicationWantsToQuit()
        {
            UniTaskCompletionSource<bool> completionSource = new UniTaskCompletionSource<bool>();
            await EventBus.Instance.PublishAsync(new PlayerLeftLobbyEvent(Player.Id, completionSource));
            return await completionSource.Task;
        }


        private async UniTask LobbyHeartbeat(Lobby lobby)
        {
            while (true)
            {
                if (lobby == null)
                {
                    return;
                }

                await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id).AsUniTask();
                await UniTask.Delay(15 * 1000);
            }
        }

    }
}