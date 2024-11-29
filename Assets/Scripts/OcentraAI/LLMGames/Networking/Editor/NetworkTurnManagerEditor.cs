using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkTurnManager))]
public class NetworkTurnManagerEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var manager = (NetworkTurnManager)target;

    }
}