
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public static class EditorSaveManager
    {
        private static bool isSavePending = false;
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
                UnityEditor.EditorApplication.delayCall += SaveAllPendingChanges;
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
                UnityEditor.EditorUtility.SetDirty(obj);
            }

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log("All pending changes saved and assets refreshed.");

#endif

        }

    }


}