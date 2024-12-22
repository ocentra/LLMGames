using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.Manager
{
    public static class ApplicationQuitHandler
    {
        private static readonly UniTaskCompletionSource<bool> QuitCompletionSource = new();
        private static bool isQuitting = false;

        public static bool IsQuitting => isQuitting;
        public static UniTask<bool> QuitTask => QuitCompletionSource.Task;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        private static bool OnApplicationWantsToQuit()
        {
            MonoBehaviour[] managers = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (MonoBehaviour manager in managers)
            {
                if (manager is IApplicationQuitter quitter)
                {
                    quitter.HandleApplicationQuitAsync().Forget();
                }
            }
            return isQuitting;
        }

        public static void SetQuitting(bool value)
        {
            isQuitting = value;
            if (isQuitting)
            {
                QuitCompletionSource.TrySetResult(true);
            }
        }

        public static void HandleQuitError(Exception ex)
        {
            QuitCompletionSource.TrySetException(ex);
        }
    }

   

}