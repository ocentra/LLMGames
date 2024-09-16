using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.Utilities;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable
{
    [CustomEditor(typeof(Card))]
    public class CardEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Card card = (Card)target;
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

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

            GUILayout.Space(10);


            if (EditorGUI.EndChangeCheck())
            {
                EditorSaveManager.RequestSave(card);
            }
        }
    }
}
