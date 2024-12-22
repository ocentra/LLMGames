#if UNITY_EDITOR
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkHumanHumanPlayer))]
public class NetWorkPlayerEditor : OdinEditor
{
}

[CustomEditor(typeof(NetworkComputerPlayer))]
public class NetworkComputerPlayerEditor : OdinEditor
{
}
#endif