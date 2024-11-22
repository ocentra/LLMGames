#if UNITY_EDITOR
using OcentraAI.LLMGames.Networking.Manager;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(GameStartManager))]
public class GameStartManagerEditor : OdinEditor
{
}

[CustomEditor(typeof(PlayerManager))]
public class PlayerManagerEditor : OdinEditor
{
}


#endif

