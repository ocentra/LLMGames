#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class ToolBarExt
{
    private static ScriptableObject CurrentToolbar { get; set; }
    public static Action OnToolbarClick;
    public static List<Action> ToolbarGUI { get; set; } = new List<Action>();

    static ToolBarExt()
    {
        OnToolbarClick = GUIClick;
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    public static void GUIClick()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            GUILayout.BeginHorizontal();
            foreach (Action handler in ToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }
    }

    static void OnUpdate()
    {
        if (CurrentToolbar == null)
        {
            Object[] objects = Resources.FindObjectsOfTypeAll(typeof(Editor).Assembly.GetType("UnityEditor.Toolbar"));
            CurrentToolbar = objects.Length > 0 ? (ScriptableObject)objects[0] : null;
            if (CurrentToolbar != null)
            {
                FieldInfo fieldInfo = CurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                object rawRoot = fieldInfo?.GetValue(CurrentToolbar);
                VisualElement mRoot = rawRoot as VisualElement;
                RegisterCallback("ToolbarZoneLeftAlign", OnToolbarClick);

                void RegisterCallback(string root, Action cb)
                {
                    VisualElement toolbarZone = mRoot.Q(root);
                    VisualElement parent = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    IMGUIContainer container = new IMGUIContainer();
                    container.style.flexGrow = 1;
                    container.onGUIHandler += () => { cb?.Invoke(); };
                    parent.Add(container);
                    toolbarZone.Add(parent);
                }
            }
        }
    }
}


#endif