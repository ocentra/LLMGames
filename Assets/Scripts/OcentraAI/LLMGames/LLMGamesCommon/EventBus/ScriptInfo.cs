using Sirenix.OdinInspector;
using System;
using System.IO;

#if true
using UnityEditor;
#endif

using UnityEngine;
using static System.String;

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
        [HideInInspector] public EventState State { get; set; }
        public enum EventState
        {
            Pass,
            Fail,
            NoSubscriber,
            NoPublisher,
        }
        public ScriptInfo(Type scriptType, MonoScript monoScript, string monoScriptPath)
        {
            ScriptType = scriptType;
            MonoScript = monoScript;
            MonoScriptPath = monoScriptPath;
            DisplayName = GetFormattedTypeName();
            CodeContent = GetCodeContent();
        }

        public ScriptInfo(string displayName)
        {
            DisplayName = displayName;
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
            if (ScriptType == null) return string.Empty;

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

            return ScriptType.Name;

        }

        private string GetCodeContent()
        {
            if (!IsNullOrEmpty(MonoScriptPath))
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

            return Empty;
        }

        public Texture2D GetIcon()
        {
            
            Texture2D statusIcon = State switch
            {
                EventState.Pass => EditorGUIUtility.IconContent("TestPassed").image as Texture2D,
                EventState.Fail => EditorGUIUtility.IconContent("console.erroricon.sml").image as Texture2D,
                EventState.NoSubscriber => EditorGUIUtility.IconContent("console.warnicon.sml").image as Texture2D,
                EventState.NoPublisher => EditorGUIUtility.IconContent("console.warnicon.sml").image as Texture2D,
                _ => null
            };

            return statusIcon;
        }

        public (SdfIconType Icon, Color IconColor) GetSdfIconTypeIcon()
        {
            return State switch
            {
                EventState.Pass => (SdfIconType.Check2All, Color.green),        // Green for success
                EventState.Fail => (SdfIconType.ExclamationCircle, Color.red),   // Red for failure
                EventState.NoSubscriber => (SdfIconType.QuestionCircle, Color.yellow), // Yellow for no subscriber
                EventState.NoPublisher => (SdfIconType.QuestionCircle, Color.yellow),  // Yellow for no publisher
                _ => (SdfIconType.None, Color.clear) // Default: No icon and no color
            };
        }





        public void SetEventState(EventInfo subscribers, EventInfo publishers)
        {
            bool hasPublishers = publishers != null && publishers.TryGetValue(MonoScript, out _);
            bool hasSubscribers = subscribers != null && subscribers.TryGetValue(MonoScript, out _);

            bool pass = hasPublishers && hasSubscribers;
            bool warningNoPublishers = !hasPublishers && hasSubscribers;
            bool warningNoSubscribers = !hasSubscribers && hasPublishers;

            if (pass)
            {
                State = EventState.Pass;
            }
            else if (warningNoPublishers)
            {
                State = EventState.NoPublisher;
            }
            else if (warningNoSubscribers)
            {
                State = EventState.NoSubscriber;
            }
            else
            {
                State = EventState.Fail;
            }


        }
    }
}