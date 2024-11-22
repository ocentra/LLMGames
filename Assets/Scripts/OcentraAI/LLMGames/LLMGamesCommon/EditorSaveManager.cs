using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.Manager.Utilities
{
    public static class EditorSaveManager
    {
        private static bool isSavePending;
        private static readonly Queue<ScriptableObject> ObjectsToSave = new Queue<ScriptableObject>();

        public static void RequestSave(ScriptableObject obj)
        {
#if UNITY_EDITOR


            if (!ObjectsToSave.Contains(obj))
            {
                ObjectsToSave.Enqueue(obj);
            }

            if (!isSavePending)
            {
                isSavePending = true;
                EditorApplication.delayCall += SaveAllPendingChanges;
            }
#endif
        }

        private static void SaveAllPendingChanges()
        {
#if UNITY_EDITOR

            isSavePending = false;

            while (ObjectsToSave.Count > 0)
            {
                ScriptableObject obj = ObjectsToSave.Dequeue();
                EditorUtility.SetDirty(obj);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Debug.Log("All pending changes saved and assets refreshed.");

#endif
        }
    }
}