using Cysharp.Threading.Tasks;
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
        private static UniTaskCompletionSource<bool> completionSource;
        public static async UniTask RequestSave(ScriptableObject obj, UniTaskCompletionSource<bool> uniTaskCompletionSource = null)
        {
#if UNITY_EDITOR

            if (!ObjectsToSave.Contains(obj))
            {
                ObjectsToSave.Enqueue(obj);
            }

            if (!isSavePending)
            {
                isSavePending = true;
                completionSource = uniTaskCompletionSource??new UniTaskCompletionSource<bool>();
                UnityEditor.EditorApplication.delayCall += SaveAllPendingChanges;
                await completionSource.Task;
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
            completionSource.TrySetResult(true);
#endif
        }
    }
}