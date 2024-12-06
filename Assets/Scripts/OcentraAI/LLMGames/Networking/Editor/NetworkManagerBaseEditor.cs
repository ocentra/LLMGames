using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace OcentraAI.LLMGames.Manager
{
    [CustomEditor(typeof(NetworkManagerBase))]
    public class NetworkManagerBaseEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            NetworkManagerBase manager = (NetworkManagerBase)target;

        }
    }
}