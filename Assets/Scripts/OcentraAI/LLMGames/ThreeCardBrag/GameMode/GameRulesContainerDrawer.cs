#if UNITY_EDITOR

using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Editor
{
    public class GameRulesContainerDrawer : OdinValueDrawer<GameRulesContainer>
    {
        private Vector2 playerScrollPosition;
        private Vector2 llmScrollPosition;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = this.ValueEntry.SmartValue;

            if (value != null)
            {
                GUILayout.Label("Player Rules", EditorStyles.boldLabel);
                playerScrollPosition = DrawScrollableTextArea(playerScrollPosition, value.Player);

                GUILayout.Label("LLM Rules", EditorStyles.boldLabel);
                llmScrollPosition = DrawScrollableTextArea(llmScrollPosition, value.LLM);
            }
        }

        private Vector2 DrawScrollableTextArea(Vector2 scrollPosition, string text)
        {
            var style = new GUIStyle(EditorStyles.textArea)
            {
                richText = true,
                wordWrap = true
            };

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(800), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();

            return scrollPosition;
        }
    }
}

#endif