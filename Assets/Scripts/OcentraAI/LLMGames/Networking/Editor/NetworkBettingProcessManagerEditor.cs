using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkBettingProcessManager))]
public class NetworkBettingProcessManagerEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var manager = (NetworkBettingProcessManager)target;

    }
}