using OcentraAI.LLMGames.Utilities;
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
            if (!string.IsNullOrEmpty(CardUtility.GetRankSymbol(card.Suit, card.Rank)))
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

            // Display the sprite asset linked to the path as an ObjectField
            GUILayout.Space(10);


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
