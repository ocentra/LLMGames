using Sirenix.OdinInspector;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class LobbyViewer
{
    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public string Id { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public string LobbyCode { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public string Name { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public int MaxPlayers { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public int AvailableSlots { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public bool IsPrivate { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public bool IsLocked { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public bool HasPassword { get; }

    [ShowInInspector, FoldoutGroup("Players"), ReadOnly, TableList(ShowIndexLabels = true)]
    public List<PlayerViewer> Players { get; }

    [ShowInInspector, FoldoutGroup("Custom Data"), ReadOnly]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, DataObjectViewer> Data { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public string HostId { get; }

    [ShowInInspector, FoldoutGroup("Timestamps"), ReadOnly]
    public string Created { get; }

    [ShowInInspector, FoldoutGroup("Timestamps"), ReadOnly]
    public string LastUpdated { get; }

    [ShowInInspector, FoldoutGroup("Lobby Details"), ReadOnly]
    public int Version { get; }

    public LobbyViewer(Lobby lobby)
    {
        if (lobby == null) return;

        Id = lobby.Id;
        LobbyCode = lobby.LobbyCode;
        Name = lobby.Name;
        MaxPlayers = lobby.MaxPlayers;
        AvailableSlots = lobby.AvailableSlots;
        IsPrivate = lobby.IsPrivate;
        IsLocked = lobby.IsLocked;
        HasPassword = lobby.HasPassword;

        // Convert players to PlayerViewer list
        if (lobby.Players != null)
        {
            Players = new List<PlayerViewer>();
            foreach (var player in lobby.Players)
            {
                Players.Add(new PlayerViewer(player));
            }
        }

        // Convert data to DataObjectViewer dictionary
        if (lobby.Data != null)
        {
            Data = new Dictionary<string, DataObjectViewer>();
            foreach (var kvp in lobby.Data)
            {
                Data[kvp.Key] = new DataObjectViewer(kvp.Value);
            }
        }

        HostId = lobby.HostId;
        Created = $"{lobby.Created}";
        LastUpdated = $"{lobby.LastUpdated}";
        Version = lobby.Version;
    }
}

public class PlayerViewer
{
    [ShowInInspector, ReadOnly]
    public string Id { get; }

    [ShowInInspector, ReadOnly]
    public string ConnectionInfo { get; }

    [ShowInInspector, ReadOnly]
    public string AllocationId { get; }

    [ShowInInspector, ReadOnly]
    public string Joined { get; }

    [ShowInInspector, ReadOnly]
    public string LastUpdated { get; }

    [ShowInInspector, ReadOnly, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, PlayerDataObjectViewer> Data { get; }

    public PlayerViewer(Player player)
    {
        if (player == null) return;

        Id = player.Id;
        ConnectionInfo = player.ConnectionInfo;
        AllocationId = player.AllocationId;
        Joined = $"{player.Joined}";
        LastUpdated = $"{player.LastUpdated}";

        // Convert player.Data to a dictionary of PlayerDataObjectViewer
        if (player.Data != null)
        {
            Data = new Dictionary<string, PlayerDataObjectViewer>();
            foreach (var kvp in player.Data)
            {
                Data[kvp.Key] = new PlayerDataObjectViewer(kvp.Value);
            }
        }
    }
}

public class PlayerDataObjectViewer
{
    [ShowInInspector, ReadOnly]
    public string Value { get; }

    [ShowInInspector, ReadOnly]
    public PlayerDataObject.VisibilityOptions Visibility { get; }

    public PlayerDataObjectViewer(PlayerDataObject dataObject)
    {
        if (dataObject == null) return;

        Value = dataObject.Value;
        Visibility = dataObject.Visibility;
    }
}


public class DataObjectViewer
{
    [ShowInInspector, ReadOnly]
    public string Value { get; }

    [ShowInInspector, ReadOnly]
    public DataObject.VisibilityOptions Visibility { get; }

    [ShowInInspector, ReadOnly]
    public DataObject.IndexOptions Index { get; }

    public DataObjectViewer(DataObject dataObject)
    {
        if (dataObject == null) return;

        Value = dataObject.Value;
        Visibility = dataObject.Visibility;
        Index = dataObject.Index;
    }
}
