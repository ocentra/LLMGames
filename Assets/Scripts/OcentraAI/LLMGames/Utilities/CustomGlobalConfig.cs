using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.Utilities;

namespace OcentraAI.LLMGames.Utilities
{
    public abstract class CustomGlobalConfig<T> : GlobalConfig<T>, ISaveScriptable where T : GlobalConfig<T>, new()
    {
        public virtual void SaveChanges()
        {
            // Centralized save logic
#if UNITY_EDITOR
            EditorSaveManager.RequestSave(this);
#endif
        }

        public virtual void LogError(string message, string method = "")
        {
            // Centralized logging for errors
            GameLogger.LogError($"[{GetType().Name}] {message}", method);
        }

        public virtual void Log(string message, string method = "")
        {
            GameLogger.Log($"[{GetType().Name}] {message}", method);
        }

        public virtual void LogWarning(string message, string method = "")
        {
            GameLogger.LogWarning($"[{GetType().Name}] {message}", method);
        }
    }
}