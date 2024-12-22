using Sirenix.OdinInspector;
using Unity.Services.Lobbies.Models;

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