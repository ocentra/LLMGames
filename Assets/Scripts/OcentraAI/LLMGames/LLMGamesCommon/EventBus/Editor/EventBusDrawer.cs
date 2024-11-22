#if UNITY_EDITOR
using NUnit.Framework.Interfaces;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Events.Editor
{
    //[CustomEditor(typeof(EventBus))]
    public class EventBusEditor : OdinEditor
    {
        private Vector2 eventsScrollPosition;
        private Vector2 detailsScrollPosition;
        private readonly float BOX_HEIGHT = 300f;
        private readonly float HEADER_HEIGHT = 30f;
        private readonly float TOOLBAR_HEIGHT = 25f;
        private readonly float LIST_ITEM_HEIGHT = 40f;
        private readonly float ITEM_PADDING = 5f;
        private string searchText = "";
        private Type selectedEventType;

        private GUIStyle headerLabelStyle;
        private GUIStyle searchBoxStyle;
        private GUIStyle eventItemStyle;
        private GUIStyle eventItemHoverStyle;
        private GUIStyle eventItemSelectedStyle;

        private void InitializeStyles()
        {
            if (headerLabelStyle == null)
            {
                headerLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
                };
            }

            if (searchBoxStyle == null)
            {
                searchBoxStyle = new GUIStyle(EditorStyles.toolbarSearchField)
                {
                    fixedHeight = 20,
                    margin = new RectOffset(5, 5, 2, 2)
                };
            }

            if (eventItemStyle == null)
            {
                eventItemStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(5, 5, 2, 2),
                    normal = { background = EditorGUIUtility.whiteTexture }
                };
            }

            if (eventItemHoverStyle == null)
            {
                eventItemHoverStyle = new GUIStyle(eventItemStyle);
                eventItemHoverStyle.normal.background = EditorGUIUtility.whiteTexture;
            }

            if (eventItemSelectedStyle == null)
            {
                eventItemSelectedStyle = new GUIStyle(eventItemStyle);
                eventItemSelectedStyle.normal.background = EditorGUIUtility.whiteTexture;
            }
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            DrawDefaultInspector();
            EditorGUILayout.Space(10);

            EventBus eventBus = (EventBus)target;
            DrawEventsPanel(eventBus);
            EditorGUILayout.Space(10);
            DrawDetailsPanel(eventBus);
        }

        private void DrawEventsPanel(EventBus eventBus)
        {
            // Events Panel Container
            Rect mainBoxRect = EditorGUILayout.GetControlRect(false, BOX_HEIGHT);
            GUI.Box(mainBoxRect, "", EditorStyles.helpBox);

            // Header
            Rect headerRect = new Rect(
                mainBoxRect.x + ITEM_PADDING,
                mainBoxRect.y + ITEM_PADDING,
                mainBoxRect.width - (ITEM_PADDING * 2),
                HEADER_HEIGHT);
            EditorGUI.DrawRect(headerRect, new Color(0.7f, 0.8f, 1f, 0.5f));
            GUI.Label(new Rect(headerRect.x + 10, headerRect.y, 100, headerRect.height), "Events", headerLabelStyle);

            // Toolbar
            DrawToolbar(headerRect, eventBus);

            // Events List
            Rect scrollViewRect = new Rect(
                mainBoxRect.x + ITEM_PADDING,
                headerRect.y + headerRect.height + ITEM_PADDING,
                mainBoxRect.width - (ITEM_PADDING * 2),
                mainBoxRect.height - headerRect.height - (ITEM_PADDING * 3)
            );

            eventsScrollPosition = GUI.BeginScrollView(
                scrollViewRect,
                eventsScrollPosition,
                new Rect(0, 0, scrollViewRect.width - 20, GetEventsContentHeight(eventBus))
            );

            //if (eventBus.ExpectedEventTypes != null)
            //{
            //    float currentY = 0;
            //    foreach (EventBus.EventUsageInfo info in eventBus.ExpectedEventTypes)
            //    {
            //        Type eventType = info.ScriptInfo.ScriptType;
            //        DrawEventItem(currentY, eventType, scrollViewRect.width - 25, info);
            //        currentY += LIST_ITEM_HEIGHT + ITEM_PADDING;
            //    }
            //}

            GUI.EndScrollView();
        }

        private void DrawToolbar(Rect headerRect, EventBus eventBus)
        {
            Rect toolbarRect = new Rect(
                headerRect.x + headerRect.width - 300,
                headerRect.y + (headerRect.height - TOOLBAR_HEIGHT) / 2,
                290,
                TOOLBAR_HEIGHT);
            GUI.Box(toolbarRect, "", EditorStyles.toolbar);

            // Search Field
            Rect searchRect = new Rect(toolbarRect.x + 5, toolbarRect.y + 2, 200, 20);
            searchText = GUI.TextField(searchRect, searchText, searchBoxStyle);

            // Analyze Button
            Rect analyzeRect = new Rect(searchRect.x + searchRect.width + 5, toolbarRect.y + 2, 80, 20);
            if (GUI.Button(analyzeRect, "Analyze"))
            {
                eventBus.CollectEventTypesAsync().Forget();
            }
        }

        private void DrawEventItem(float currentY, Type eventType, float width, EventBus.EventUsageInfo eventInfo)
        {
            Rect itemRect = new Rect(5, currentY, width, LIST_ITEM_HEIGHT);
            bool isHovered = itemRect.Contains(Event.current.mousePosition);
            bool isSelected = selectedEventType == eventType;

            Color backgroundColor = isSelected ? new Color(0.7f, 0.8f, 1f, 0.3f) : isHovered ? new Color(0.7f, 0.7f, 0.7f, 0.1f) : new Color(0.5f, 0.5f, 0.5f, 0.05f);
            EditorGUI.DrawRect(itemRect, backgroundColor);

            MonoScript script = eventInfo.ScriptInfo.MonoScript;

            // Draw the script icon and name label
            if (script != null)
            {
                Rect iconRect = new Rect(itemRect.x + 10, itemRect.y + 5, 20, 20);
                Rect labelRect = new Rect(iconRect.xMax + 5, itemRect.y + 5, itemRect.width - iconRect.width - 30, 20);

                // Draw the icon
                Texture2D icon = EditorGUIUtility.ObjectContent(script, typeof(MonoScript)).image as Texture2D;
                if (icon != null)
                {
                    GUI.DrawTexture(iconRect, icon);
                }

                // Draw the label
                GUI.Label(labelRect, script.name, EditorStyles.objectField);
            }
            else
            {
                // Draw a label with the event type name if the script is not found
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 5, itemRect.width - 20, 20), eventType.Name, EditorStyles.label);
            }

            // Handle mouse click for pinging the script
            if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
            {
                if (script != null)
                {
                    EditorGUIUtility.PingObject(script);
                }
                selectedEventType = eventType;
                Event.current.Use();
                Repaint();
            }

            EditorGUIUtility.AddCursorRect(itemRect, MouseCursor.Link);
        }


        private void DrawDetailsPanel(EventBus eventBus)
        {
            if (selectedEventType == null) return;

            // Details Panel (placeholder for now)
            Rect detailsRect = EditorGUILayout.GetControlRect(false, BOX_HEIGHT);
            GUI.Box(detailsRect, "", EditorStyles.helpBox);

            // Header
            Rect headerRect = new Rect(
                detailsRect.x + ITEM_PADDING,
                detailsRect.y + ITEM_PADDING,
                detailsRect.width - (ITEM_PADDING * 2),
                HEADER_HEIGHT);
            EditorGUI.DrawRect(headerRect, new Color(0.7f, 0.9f, 0.7f, 0.5f));
            GUI.Label(new Rect(headerRect.x + 10, headerRect.y, headerRect.width - 20, headerRect.height),
                     $"Details: {selectedEventType.Name}", headerLabelStyle);
        }

        private float GetEventsContentHeight(EventBus eventBus)
        {
            //if (eventBus.ExpectedEventTypes == null) return 0;
            //return eventBus.ExpectedEventTypes.Count * (LIST_ITEM_HEIGHT + ITEM_PADDING);
            return 0;
        }
    }
}
#endif