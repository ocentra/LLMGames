using OcentraAI.LLMGames.Manager.Utilities;
using OcentraAI.LLMGames.Utilities;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable
{
    [CustomEditor(typeof(Card))]
    public class CardEditor : OdinEditor
    {
        private GUIStyle richTextStyle;

        public override void OnInspectorGUI()
        {
            Card card = (Card)target;

            if (richTextStyle == null)
            {
                richTextStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            base.OnInspectorGUI();

            if (!string.IsNullOrEmpty(CardUtility.GetRankSymbol(card.Suit, card.Rank)))
            {
                GUILayout.Space(10);
                GUILayout.Label("Rank Symbol", EditorStyles.boldLabel);
                GUILayout.Label(new GUIContent(card.RankSymbol), richTextStyle);
            }

            if (GUI.changed)
            {
                EditorSaveManager.RequestSave(card);
            }
        }
    }
}