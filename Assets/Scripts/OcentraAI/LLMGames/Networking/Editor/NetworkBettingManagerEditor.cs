using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace OcentraAI.LLMGames.Manager
{
    [CustomEditor(typeof(NetworkBettingManager))]
    public class NetworkBettingManagerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var manager = (NetworkBettingManager)target;

        }
    }
}