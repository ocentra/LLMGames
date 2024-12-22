using Sirenix.OdinInspector;
using Unity.Services.Lobbies.Models;

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