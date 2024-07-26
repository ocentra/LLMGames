using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable
{
    [CustomEditor(typeof(Card))]
    public class CardEditor : OdinEditor
    {

        public override void OnInspectorGUI()
        {
            Card card = (Card)target;

            // Draw default inspector
            DrawDefaultInspector();

            // Custom display for RankSymbol
            if (!string.IsNullOrEmpty(card.RankSymbol))
            {
                GUILayout.Space(10);
                GUILayout.Label("Rank Symbol", EditorStyles.boldLabel);

                GUIStyle style = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    fontSize = 16
                };

                GUILayout.Label(new GUIContent(card.RankSymbol), style);
            }

            // Save changes if any
            if (GUI.changed)
            {
                EditorUtility.SetDirty(card);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}