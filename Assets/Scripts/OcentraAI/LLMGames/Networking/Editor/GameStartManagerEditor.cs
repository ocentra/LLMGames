


using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkGameManager))]
public class NetworkGameManagerEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var manager = (NetworkGameManager)target;
      
    }
}