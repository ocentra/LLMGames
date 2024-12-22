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
    public abstract class ManagerBase<T> : SerializedMonoBehaviour, IEventHandler, IApplicationQuitter,IManager
        where T : Component
    {
        [Header("File Logging Settings")]
        [ShowInInspector] public bool ToEditor { get; set; } = true;
        [ShowInInspector] public bool ToFile { get; set; } 
        [ShowInInspector] public bool UseStackTrace { get; set; } 

        [ShowInInspector, Required]
        public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();

        private readonly UniTaskCompletionSource initializationSource = new UniTaskCompletionSource();

        protected virtual async UniTask Initialize()
        {
            initializationSource.TrySetResult();
            await UniTask.Yield();
        }



        public async UniTaskVoid HandleApplicationQuitAsync()
        {
            try
            {
                bool shouldQuit = await ApplicationWantsToQuit();
                ApplicationQuitHandler.SetQuitting(shouldQuit);

                if (shouldQuit && !Application.isEditor)
                {
                    Application.Quit();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error during application quit: {ex.Message}", this);
                ApplicationQuitHandler.HandleQuitError(ex);
            }
        }

        protected virtual async UniTask<bool> ApplicationWantsToQuit()
        {
            Log("ApplicationWantsToQuit: Base implementation", this);
            bool fromResult = await UniTask.FromResult(true);
            return fromResult;
        }


        protected virtual void Awake()
        {
            Initialize().Forget();
        }

        protected virtual void Start() { }

        protected virtual void OnValidate()
        {
            if (EventRegistrar == null)
            {
                Debug.LogError($"EventRegistrar is required for {typeof(T).Name}", this);
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

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
            {
                HandleApplicationQuitAsync().Forget();
            }
        }

        public virtual void SubscribeToEvents()
        {
           
        }
        

        public async UniTask WaitForInitializationAsync()
        {
            await initializationSource.Task;
        }

        public virtual void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();
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