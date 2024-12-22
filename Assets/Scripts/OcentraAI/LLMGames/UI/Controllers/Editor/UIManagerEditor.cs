using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [CustomEditor(typeof(UIManager))]
    public class UIManagerEditor : OdinEditor
    {
        private bool showTopCardViews = true;
        private bool showBettingButtons = true;

        public override void OnInspectorGUI()
        {
            UIManager manager = (UIManager)target;

            // Draw default inspector first
            DrawDefaultInspector();

            // Top Card Views Dictionary
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showTopCardViews = EditorGUILayout.Foldout(showTopCardViews, "Top Card Views", true);
            if (showTopCardViews && manager.TopCardViews != null)
            {
                EditorGUI.indentLevel++;
                foreach (KeyValuePair<PlayerDecision, TopCardView> kvp in manager.TopCardViews)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key?.Name ?? "None", GUILayout.Width(100));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(kvp.Value, typeof(CardView), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Betting Buttons Dictionary
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showBettingButtons = EditorGUILayout.Foldout(showBettingButtons, "Betting Buttons", true);
            if (showBettingButtons && manager.BettingButtons != null)
            {
                EditorGUI.indentLevel++;
                foreach (KeyValuePair<PlayerDecision, PlayerDecisionButton> kvp in manager.BettingButtons)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key?.Name ?? "None", GUILayout.Width(100));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(kvp.Value, typeof(Button3D), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            // Betting Buttons Dictionary
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showBettingButtons = EditorGUILayout.Foldout(showBettingButtons, "UI Buttons", true);
            if (showBettingButtons && manager.UIOrientedDecisions != null)
            {
                EditorGUI.indentLevel++;
                foreach (KeyValuePair<PlayerDecision, PlayerDecisionButton> kvp in manager.UIOrientedDecisions)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key?.Name ?? "None", GUILayout.Width(100));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(kvp.Value, typeof(Button3D), true);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

        }
    }
}