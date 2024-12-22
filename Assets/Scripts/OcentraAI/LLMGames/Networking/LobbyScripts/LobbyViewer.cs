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