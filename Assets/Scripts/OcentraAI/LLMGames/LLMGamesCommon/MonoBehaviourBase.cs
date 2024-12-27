using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.Manager
{
    public abstract class MonoBehaviourBase<T> : SerializedMonoBehaviour, IEventHandler, IApplicationQuitter,IMonoBehaviourBase where T : Component
    {
        [Header("File Logging Settings")]
        [ShowInInspector] public bool ToEditor { get; set; } = true;
        [ShowInInspector] public bool ToFile { get; set; } 
        [ShowInInspector] public bool UseStackTrace { get; set; } 

        [ShowInInspector, Required]
        public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();

       public UniTaskCompletionSource InitializationSource { get; set; } = new UniTaskCompletionSource();

        public virtual async UniTask InitializeAsync()
        {
            InitializationSource.TrySetResult();
            await UniTask.Yield();
        }
        

        public virtual async UniTask<bool> ApplicationWantsToQuit()
        {
            bool fromResult = await UniTask.FromResult(true);
            return fromResult;
        }


        protected virtual void Awake()
        {
            if (EventRegistrar == null)
            {
                LogError($"EventRegistrar is required for {typeof(T).Name}", this);
            }

            InitializeAsync().Forget();
        }

        protected virtual void Start() { }

        protected virtual void OnValidate()
        {
            if (EventRegistrar == null)
            {
                LogError($"EventRegistrar is required for {typeof(T).Name}", this);
            }
        }

        protected virtual void OnEnable()
        {
            SubscribeToEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        protected virtual void OnDestroy() { }

        public virtual void SubscribeToEvents() { }
        public virtual void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();
        }

        public async UniTask WaitForInitializationAsync()
        {
            await InitializationSource.Task;
        }
        

        public virtual void Log(string message, Object context, bool toEditor = default, bool toFile = default, bool useStack = default)
        {
            if (toEditor == default) toEditor = ToEditor;
            if (toFile == default) toFile = ToFile;
            if (useStack == default) useStack = UseStackTrace;
            GlobalConfig<GameLoggerScriptable>.Instance?.Log(message, context, toEditor, toFile, useStack);
        }

        public virtual void LogWarning(string message, Object context, bool toEditor = default, bool toFile = default, bool useStack = default)
        {
            if (toEditor == default) toEditor = ToEditor;
            if (toFile == default) toFile = ToFile;
            if (useStack == default) useStack = UseStackTrace;
            GlobalConfig<GameLoggerScriptable>.Instance?.LogWarning(message, context, toEditor, toFile, useStack);
        }

        public virtual void LogError(string message, Object context, bool toEditor = default, bool toFile = default, bool useStack = default)
        {
            if (toEditor == default) toEditor = ToEditor;
            if (toFile == default) toFile = ToFile;
            if (useStack == default) useStack = UseStackTrace;
            GlobalConfig<GameLoggerScriptable>.Instance?.LogError(message, context, toEditor, toFile, useStack);
        }


    }
}