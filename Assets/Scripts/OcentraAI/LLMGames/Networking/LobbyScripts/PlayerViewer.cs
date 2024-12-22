using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

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