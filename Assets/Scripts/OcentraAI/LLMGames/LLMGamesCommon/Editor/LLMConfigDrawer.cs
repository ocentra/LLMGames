#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class LLMConfigDrawer : OdinValueDrawer<ILLMConfig>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ILLMConfig config = this.ValueEntry.SmartValue;

            if (label != null)
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }

            SirenixEditorGUI.BeginBox();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Property", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Value", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            DrawRow("ApiKey", config.ApiKey);
            DrawRow("ApiKey2", config.ApiKey2);
            DrawRow("ApiUrl", config.ApiUrl);
            DrawRow("Endpoint", config.Endpoint);
            DrawRow("MaxTokens", config.MaxTokens.ToString());
            DrawRow("Model", config.Model);
            DrawRow("Provider", config.Provider?.Name ?? "None");
            DrawRow("Stream", config.Stream.ToString());
            DrawRow("Temperature", $"{config.Temperature}");
            DrawRow("ProviderName", $"{config.ProviderName}");

            SirenixEditorGUI.EndBox();
        }

        private void DrawRow(string property, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(property, GUILayout.Width(150));
            SirenixEditorGUI.BeginBox();
            GUILayout.Label(value ?? "None", GUILayout.ExpandWidth(true));
            SirenixEditorGUI.EndBox();
            GUILayout.EndHorizontal();
        }


    }
}
#endif