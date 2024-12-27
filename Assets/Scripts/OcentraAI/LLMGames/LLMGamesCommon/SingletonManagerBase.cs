using Cysharp.Threading.Tasks;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager
{
    public abstract class SingletonManagerBase<T> : MonoBehaviourBase<T> where T : Component
    {
        public static T Instance { get; private set; }
        
        public static T GetInstance()
        {
            CreateSingletonInstance();
            return Instance;
        }
        
        protected override void Awake()
        {
            CreateSingletonInstance();
            InitializeAsync().Forget();
        }

        protected override void OnValidate()
        {
            InitializeAsync().Forget();
        }

        public static void CreateSingletonInstance()
        {
            if (Instance == null)
            {
                Instance = FindFirstObjectByType<T>();
                if (Instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).Name);
                    Instance = gameObject.AddComponent<T>();
                    DontDestroyOnLoad(gameObject);
                }
            }
        }
        
    }
}