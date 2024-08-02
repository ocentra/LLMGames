#if UNITY_EDITOR

using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Editor
{
    public class GameRulesContainerDrawer : OdinValueDrawer<GameRulesContainer>
    {
        private Vector2 playerScrollPosition;
        private Vector2 llmScrollPosition;

        [OdinSerialize]
        private float playerDisplayHeight = 100;

        [OdinSerialize]
        private float llmDisplayHeight = 100;

        [OdinSerialize]
        private const float widthPadding = 30f;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = this.ValueEntry.SmartValue;

            if (value != null)
            {
                EditorGUILayout.BeginVertical();

                playerDisplayHeight = EditorGUILayout.FloatField("Player", playerDisplayHeight);

               // GUILayout.Label("Player Rules", EditorStyles.boldLabel);
                playerScrollPosition = DrawScrollableTextArea(playerScrollPosition, value.Player, playerDisplayHeight);

                llmDisplayHeight = EditorGUILayout.FloatField("LLM", llmDisplayHeight);

              //  GUILayout.Label("LLM Rules", EditorStyles.boldLabel);
                llmScrollPosition = DrawScrollableTextArea(llmScrollPosition, value.LLM, llmDisplayHeight);

                EditorGUILayout.EndVertical();
            }
        }

        private Vector2 DrawScrollableTextArea(Vector2 scrollPosition, string text, float scrollViewHeight)
        {
            var style = new GUIStyle(EditorStyles.textArea)
            {
                richText = true,
                wordWrap = true
            };

            // Measure the height of the text based on the current view width minus some padding
            var content = new GUIContent(text);
            var calculatedHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - widthPadding);

            // Ensure the selectable label height is never greater than the scroll view height
            var displayHeight = Mathf.Max(calculatedHeight, scrollViewHeight);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));
            EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(displayHeight), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();

            return scrollPosition;
        }
    }
}

#endif
