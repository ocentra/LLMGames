#if UNITY_EDITOR


using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class LLMProviderDrawer : OdinValueDrawer<ILLMProvider>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {

            ILLMProvider currentValue = this.ValueEntry.SmartValue;
            List<ILLMProvider> providers = LLMProvider.GetAllProvidersStatic();
            List<string> providerNames = providers.Select(t => t.Name).ToList();
            int currentIndex = providers.IndexOf(currentValue);
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }
            EditorGUILayout.BeginHorizontal();
            if (label != null)
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            }
            int newSelectedIndex = EditorGUILayout.Popup(currentIndex, providerNames.ToArray());
            EditorGUILayout.EndHorizontal();
            if (newSelectedIndex != currentIndex)
            {
                this.ValueEntry.SmartValue = providers[newSelectedIndex];
            }
        }
    }
}
#endif