#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class RichTextDrawer : OdinAttributeDrawer<RichTextAttribute, string>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        bool editable = Attribute.Editable;

        if (!string.IsNullOrEmpty(Attribute.FieldName))
        {
            object parentObject = ValueEntry.Property.Parent.ValueEntry.WeakSmartValue;

            FieldInfo fieldInfo = parentObject.GetType().GetField(Attribute.FieldName);
            if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
            {
                editable = (bool)fieldInfo.GetValue(parentObject);
            }
        }

        GUIStyle richTextStyle = new GUIStyle(GUI.skin.label)
        {
            richText = true,
            wordWrap = true
        };

        if (label != null)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        if (editable)
        {
            ValueEntry.SmartValue = EditorGUILayout.TextArea(ValueEntry.SmartValue, GUILayout.Height(60));
        }
        else
        {
            EditorGUILayout.LabelField(ValueEntry.SmartValue, richTextStyle);
        }
    }
}
#endif