using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Manager.Utilities;
using Sirenix.Utilities;

namespace OcentraAI.LLMGames.Utilities
{
    public abstract class CustomGlobalConfig<T> : GlobalConfig<T>, ISaveScriptable, IApplicationQuitter where T : GlobalConfig<T>, new()
    {
        
        public virtual async UniTask<bool> ApplicationWantsToQuit()
        {
            bool fromResult = await UniTask.FromResult(true);
            return fromResult;
        }


        public virtual void SaveChanges()
        {
            // Centralized save logic
#if UNITY_EDITOR
            EditorSaveManager.RequestSave(this).Forget();
#endif
        }
        

    }
}