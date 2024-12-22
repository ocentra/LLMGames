using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    public class UsageInfoDrawer : OdinValueDrawer<UsageInfo>
    {
        private bool isFoldedOut = true;
        private Vector2 scrollPosition;
        private const float FixedHeight = 400f;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            UsageInfo usageInfo = ValueEntry.SmartValue;
            if (usageInfo == null)
            {
                SirenixEditorGUI.ErrorMessageBox("UsageInfo is null.");
                return;
            }

            Dictionary<MonoScript, List<MonoScript>> subscribers = usageInfo.Subscribers.GetAll();
            Dictionary<MonoScript, List<MonoScript>> publishers = usageInfo.Publishers.GetAll();

            isFoldedOut = SirenixEditorGUI.Foldout(isFoldedOut, "Usage Info");
            if (isFoldedOut)
            {
                float columnWidth = EditorGUIUtility.currentViewWidth / 3;
                DrawStickyHeader(columnWidth);
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true, GUILayout.Height(FixedHeight), GUILayout.Width(EditorGUIUtility.currentViewWidth - 20));

                int index = 0;
                Color originalColor = GUI.backgroundColor; 

                foreach (KeyValuePair<MonoScript, List<MonoScript>> kvp in subscribers)
                {
                    MonoScript eventScript = kvp.Key;
                    List<MonoScript> subscriberUsages = kvp.Value;
                    publishers.TryGetValue(eventScript, out List<MonoScript> publisherUsages);

                    Color backgroundColor = index % 2 == 0
                        ? new Color(0.5f, 1f, 0.5f, 1)    
                        : new Color(0.5f, 1f, 0.5f, 0.1f);     

                    GUI.backgroundColor = backgroundColor;

                    SirenixEditorGUI.BeginBox(GUILayout.Width(EditorGUIUtility.currentViewWidth - 25));
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth - 30));

                    DrawTableCell(eventScript, columnWidth);
                    GUILayout.Space(10);
                    DrawTableCell(subscriberUsages, columnWidth);
                    GUILayout.Space(10);
                    DrawTableCell(publisherUsages, columnWidth);
                    GUILayout.Space(10);

                    EditorGUILayout.EndHorizontal();
                    SirenixEditorGUI.EndBox();
                    GUILayout.Space(1);

                    GUI.backgroundColor = originalColor; // Restore the original color

                    index++;
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStickyHeader(float columnWidth)
        {
            Color originalColor = GUI.backgroundColor;

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.3f, 0.95f);

            SirenixEditorGUI.BeginBox();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) {normal = {textColor = Color.white}};

            GUILayout.Label("Event", headerStyle, GUILayout.Width(columnWidth));
            GUILayout.Label("Subscribers", headerStyle, GUILayout.Width(columnWidth));
            GUILayout.Label("Publishers", headerStyle, GUILayout.Width(columnWidth));

            EditorGUILayout.EndHorizontal();
            SirenixEditorGUI.EndBox();

            GUI.backgroundColor = originalColor;
            GUILayout.Space(10);
        }

        private void DrawTableCell(MonoScript script, float columnWidth)
        {
            EditorGUILayout.ObjectField("", script, typeof(MonoScript), false, GUILayout.Width(columnWidth));
        }

        private void DrawTableCell(List<MonoScript> scripts, float columnWidth)
        {
            EditorGUILayout.BeginVertical();
            if (scripts != null)
            {
                foreach (MonoScript script in scripts)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.ObjectField("", script, typeof(MonoScript), false, GUILayout.Width(columnWidth));
                    GUILayout.Space(2);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSeparator()
        {
            GUILayout.Space(5);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            GUILayout.Space(5);
        }
    }
}
