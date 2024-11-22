using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Manager.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.Utilities
{
    public abstract class CustomGlobalConfig<T> : GlobalConfig<T>, ISaveScriptable where T : GlobalConfig<T>, new()
    {
        public UniTaskCompletionSource<bool> QuitCompletionSource = new UniTaskCompletionSource<bool>();
        public static bool IsQuitting = false;

        [RuntimeInitializeOnLoadMethod]
        private static void SetupApplicationQuitHandler()
        {
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        private static bool OnApplicationWantsToQuit()
        {
            CustomGlobalConfig<T> globalConfig = Instance as CustomGlobalConfig<T>;
            if (globalConfig != null && globalConfig.QuitCompletionSource == null && !IsQuitting)
            {
                globalConfig.QuitCompletionSource = new UniTaskCompletionSource<bool>();
                IsQuitting = true;
                globalConfig.HandleApplicationQuit(globalConfig.QuitCompletionSource).Forget();
                return false;
            }
            return true;
        }
        private async UniTaskVoid HandleApplicationQuit(UniTaskCompletionSource<bool> completionSource)
        {
            bool success = await ApplicationWantsToQuit();
            completionSource.TrySetResult(success);
            if (success)
            {
                Application.Quit();
            }
            else
            {
                IsQuitting = false;
            }

            QuitCompletionSource = null;
        }

        protected virtual UniTask<bool> ApplicationWantsToQuit()
        {
            return UniTask.FromResult(true);
        }

        public virtual void SaveChanges()
        {
            // Centralized save logic
#if UNITY_EDITOR
            EditorSaveManager.RequestSave(this);
#endif
        }


    }
}