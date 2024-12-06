using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;


namespace OcentraAI.LLMGames.Manager
{
    [CustomEditor(typeof(NetworkPlayerManager))]
    public class NetworkPlayerManagerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var manager = (NetworkPlayerManager)target;

        }
    }
}

