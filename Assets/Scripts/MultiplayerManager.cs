using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MultiplayerManager 
{
    private Lobby currentLobby;

    protected string PlayerId;
    

    public class PlayerData
    {
        // Simple data container for lobby state
        public NetworkVariable<bool> IsReady;
        public NetworkVariable<bool> AllPlayersReady;
        public NetworkVariable<bool> IsServer;
        public NetworkVariable<bool> IsClient;
        public PlayerData(bool isServer = false)
        {
            IsServer = new NetworkVariable<bool>(isServer);
            IsClient = new NetworkVariable<bool>(!isServer);
            IsReady = new NetworkVariable<bool>(false);
            AllPlayersReady = new NetworkVariable<bool>(false);
        }
    }

    private Dictionary<string, PlayerData> PlayerChoices { get; set; } = new Dictionary<string, PlayerData>();

    private async UniTaskVoid Start()
    {
        await UnityServices.InitializeAsync().AsUniTask();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync().AsUniTask();

        }

        PlayerId = AuthenticationService.Instance.PlayerId;
    }

    public async UniTaskVoid CreateLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("My Game Lobby", 4, new CreateLobbyOptions
            {
                IsPrivate = false
            }).AsUniTask();

            Debug.Log("Lobby created with ID: " + currentLobby.Id);

            PlayerChoices.TryAdd(PlayerId, new PlayerData(true));


            await AllocateRelayServerAndGetJoinCode(4);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    public async UniTaskVoid JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync().AsUniTask();
            if (queryResponse.Results.Count > 0)
            {
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id).AsUniTask();
                Debug.Log("Joined lobby with ID: " + currentLobby.Id);

                await JoinRelayServerFromJoinCode(currentLobby.Data["joinCode"].Value);

                PlayerChoices.TryAdd(PlayerId, new PlayerData());
            }
            else
            {
                Debug.Log("No available lobbies found.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to join lobby: " + e.Message);
        }
    }

    public async void SetReadyStatus(bool ready)
    {
        if (PlayerChoices.TryGetValue(AuthenticationService.Instance.PlayerId, out PlayerData playerData))
        {
            playerData.IsReady.Value = ready;

            if (playerData.IsServer.Value)
            {
                // Update lobby for browsing players
                await UpdateLobbyReadyStatus();

                // Check and update AllPlayersReady
                bool areAllReady = true;
                foreach (PlayerData player in PlayerChoices.Values)
                {
                    if (!player.IsReady.Value)
                    {
                        areAllReady = false;
                        break;
                    }
                }

                // This will sync to all clients automatically
                foreach (PlayerData player in PlayerChoices.Values)
                {
                    player.AllPlayersReady.Value = areAllReady;
                }
            }
        }
    }





    private async UniTask UpdateLobbyReadyStatus()
    {
        Dictionary<string, DataObject> updatedData = new Dictionary<string, DataObject>();

        // Update individual player ready states
        foreach (KeyValuePair<string, PlayerData> player in PlayerChoices)
        {
            updatedData[player.Key] = new DataObject(
                DataObject.VisibilityOptions.Public,
                player.Value.IsReady.Value.ToString()
            );
        }

        // Add overall ready status
        if (PlayerChoices.TryGetValue(PlayerId, out PlayerData hostData))
        {
            updatedData["AllReady"] = new DataObject(
                DataObject.VisibilityOptions.Public,
                hostData.AllPlayersReady.Value.ToString()
            );
        }

        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
        {
            Data = updatedData
        }).AsUniTask();
    }



    private async UniTask<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxPlayers)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers).AsUniTask();
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId).AsUniTask();

        Debug.Log("Relay join code: " + joinCode);

        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
            }
        }).AsUniTask();

        bool isWebSocket = Application.platform == RuntimePlatform.WebGLPlayer;

        RelayServerData relayServerData = new RelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.ConnectionData,
            allocation.ConnectionData,
            allocation.Key,
            true,
            isWebSocket
        );

        return relayServerData;
    }

    private async UniTask<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode).AsUniTask();

        bool isWebSocket = Application.platform == RuntimePlatform.WebGLPlayer;

        RelayServerData relayServerData = new RelayServerData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData,
            joinAllocation.Key,
            true,
            isWebSocket
        );

        return relayServerData;
    }
    

}