using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public static class ApplicationQuitHandler
    {
        private static bool isQuitting;
        private static bool quitInProgress;
        public static IReadOnlyList<IApplicationQuitter> Quitters;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        private static bool OnApplicationWantsToQuit()
        {
            Quitters = GetAllQuitters();
            if (quitInProgress) return isQuitting;
            quitInProgress = true;
            RunQuitProcessAsync().Forget();
            return false;
        }

        private static async UniTaskVoid RunQuitProcessAsync()
        {
            try
            {
                List<UniTask<bool>> tasks = new List<UniTask<bool>>(Quitters.Count);
                foreach (var quitter in Quitters)
                {
                    tasks.Add(quitter.ApplicationWantsToQuit());
                }

                bool[] results = await UniTask.WhenAll(tasks);

                bool allTrue = true;
                for (int i = 0; i < results.Length; i++)
                {
                    if (!results[i])
                    {
                        allTrue = false;
                        break;
                    }
                }

                if (allTrue)
                {
                    isQuitting = true;
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
                }
            }
            finally
            {
                quitInProgress = false;
            }
        }


        private static IReadOnlyList<IApplicationQuitter> GetAllQuitters()
        {
            MonoBehaviour[] monoBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IApplicationQuitter> list = new List<IApplicationQuitter>();
            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour is IApplicationQuitter applicationQuitter)
                {
                    list.Add(applicationQuitter);
                }
            }

            return list;
        }
    }
}