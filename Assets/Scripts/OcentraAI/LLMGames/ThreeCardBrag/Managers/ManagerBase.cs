using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public abstract class ManagerBase<T> : SerializedMonoBehaviour where T : Component
    {
        protected static T instance;
        public static T Instance => instance;

        
        protected virtual void Awake()
        {
            CreateSingletonInstance();
            Initialize();
        }

        public static void CreateSingletonInstance()
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).Name);
                    instance = gameObject.AddComponent<T>();
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }

        protected virtual void OnValidate() => Initialize();

        protected virtual void Start() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void Initialize() { }

        protected virtual void Log(string message, string method = "") => GameLogger.Log($"[{GetType().Name}] {message}", method);


        protected virtual void LogError(string message, string method = "") => GameLogger.LogError($"[{GetType().Name}] {message}", method);

        protected virtual void LogWarning(string message, string method = "") => GameLogger.LogWarning($"[{GetType().Name}] {message}", method);
    }
}