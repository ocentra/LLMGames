using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace OcentraAI.LLMGames.Manager
{
    [CustomEditor(typeof(NetworkDeckManager))]
    public class NetworkDeckManagerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            NetworkDeckManager manager = (NetworkDeckManager)target;

        }
    }
}