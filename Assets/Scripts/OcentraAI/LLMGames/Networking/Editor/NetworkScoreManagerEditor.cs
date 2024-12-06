using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace OcentraAI.LLMGames.Manager
{
    [CustomEditor(typeof(NetworkScoreManager))]
    public class NetworkScoreManagerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var manager = (NetworkScoreManager)target;

        }
    }
}