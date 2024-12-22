using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class ScriptInfo
    {
        [ShowInInspector, AssetsOnly, ReadOnly, LabelText("Script")]
        public readonly MonoScript MonoScript;

        [HideInInspector] public readonly Type ScriptType;

        [HideInInspector] public readonly string DisplayName;

        [HideInInspector] public readonly string MonoScriptPath;

        [HideInInspector] public readonly string CodeContent;

        public ScriptInfo(Type scriptType, MonoScript monoScript, string monoScriptPath)
        {
            ScriptType = scriptType;
            MonoScript = monoScript;
            MonoScriptPath = monoScriptPath;
            DisplayName = GetFormattedTypeName();
            CodeContent = GetCodeContent();
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptInfo other)
            {
                return ScriptType == other.ScriptType &&
                       MonoScript == other.MonoScript &&
                       MonoScriptPath == other.MonoScriptPath;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ScriptType, MonoScript, MonoScriptPath);
        }

        private string GetFormattedTypeName()
        {
            if (ScriptType is { IsGenericType: true })
            {
                string typeName = ScriptType.Name;
                int backtickIndex = typeName.IndexOf('`');
                if (backtickIndex > 0)
                {
                    typeName = typeName.Substring(0, backtickIndex);
                }

                return typeName;
            }
            else
            {
                return ScriptType.Name;
            }
        }

        private string GetCodeContent()
        {
            if (!string.IsNullOrEmpty(MonoScriptPath))
            {
                try
                {
                    return File.ReadAllText(MonoScriptPath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading script content from {MonoScriptPath}: {ex.Message}");
                }
            }
            else if (MonoScript != null)
            {
                try
                {
                    return MonoScript.text;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error accessing script text for {MonoScript.name}: {ex.Message}");
                }
            }

            return string.Empty;
        }
    }
}