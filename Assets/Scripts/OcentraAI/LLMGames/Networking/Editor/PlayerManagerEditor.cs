using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkPlayerManager))]
public class PlayerManagerEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var manager = (NetworkPlayerManager)target;

    }
}