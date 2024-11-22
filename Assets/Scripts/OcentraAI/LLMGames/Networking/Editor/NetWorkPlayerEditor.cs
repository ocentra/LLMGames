#if UNITY_EDITOR
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkPlayer))]
public class NetWorkPlayerEditor : OdinEditor
{
}
#endif