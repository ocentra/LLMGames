
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CustomDropdownWindow : EditorWindow
{
    private List<(string label, Action onClick)> items = new();
    private Vector2 windowPosition;
    private float windowWidth;
    private static GUIStyle itemStyle;
    private int hoverIndex = -1;
    public static CustomDropdownWindow CurrentWindow;
    private static Rect lastButtonRect;
    private bool shouldClose = false;

    public static void Show(Rect buttonRect, List<(string label, Action onClick)> menuItems)
    {
        if (CurrentWindow != null)
        {
            CurrentWindow.Close();
            if (lastButtonRect.Equals(buttonRect))
            {
                CurrentWindow = null;
                return;
            }
        }

        CustomDropdownWindow window = CreateInstance<CustomDropdownWindow>();
        window.items = menuItems;
        window.windowWidth = Mathf.Min(
            buttonRect.width,
            EditorStyles.label.CalcSize(new GUIContent(menuItems.Max(item => item.label))).x + 16
        );

        window.windowPosition = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.y + buttonRect.height));
        window.ShowPopup();

        window.position = new Rect(
            window.windowPosition.x,
            window.windowPosition.y,
            window.windowWidth,
            menuItems.Count * EditorGUIUtility.singleLineHeight + 2
        );

        CurrentWindow = window;
        lastButtonRect = buttonRect;
    }

    private void OnGUI()
    {
        if (shouldClose)
        {
            Close();
            return;
        }

        if (itemStyle == null)
        {
            itemStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                active = { textColor = Color.white },
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = EditorGUIUtility.singleLineHeight
            };
        }

        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.2f, 0.2f, 0.2f, 1f));
        EditorGUI.DrawRect(new Rect(1, 1, position.width - 2, position.height - 2), new Color(0.3f, 0.3f, 0.3f, 1f));

        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;

        if (e.type == EventType.MouseDown && !position.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)))
        {
            shouldClose = true;
            e.Use();
            return;
        }

        hoverIndex = -1;
        for (int i = 0; i < items.Count; i++)
        {
            Rect itemRect = new Rect(1, i * EditorGUIUtility.singleLineHeight + 1, position.width - 2, EditorGUIUtility.singleLineHeight);
            if (itemRect.Contains(mousePos))
            {
                hoverIndex = i;
                break;
            }
        }

        if (e.type == EventType.MouseDown)
        {
            if (new Rect(0, 0, position.width, position.height).Contains(mousePos))
            {
                if (hoverIndex != -1)
                {
                    items[hoverIndex].onClick?.Invoke();
                    shouldClose = true;
                }
                e.Use();
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            Rect itemRect = new Rect(1, i * EditorGUIUtility.singleLineHeight + 1, position.width - 2, EditorGUIUtility.singleLineHeight);

            if (i == hoverIndex)
            {
                EditorGUI.DrawRect(itemRect, new Color(0.3f, 0.3f, 0.5f, 1f));
            }

            if (e.type == EventType.Repaint)
            {
                itemStyle.Draw(itemRect, items[i].label, itemRect.Contains(mousePos), false, false, false);
            }
        }

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            shouldClose = true;
            e.Use();
        }

        if (focusedWindow != this)
        {
            shouldClose = true;
            Repaint();
        }
    }

    private void OnLostFocus()
    {
        shouldClose = true;
        Repaint();
    }

    private void OnDestroy()
    {
        if (CurrentWindow == this)
        {
            CurrentWindow = null;
        }
    }
}


#endif