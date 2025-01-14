using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager
{
    public abstract class ScriptableSingletonBase<T> : CustomGlobalConfig<T>, IEventHandler, IScriptableSingletonManagerBase where T : GlobalConfig<T>, new()
    {

        [Header("File Logging Settings")]
        [ShowInInspector] public bool ToEditor { get; set; } = true;
        [ShowInInspector] public bool ToFile { get; set; }
        [ShowInInspector] public bool UseStackTrace { get; set; }

        [ShowInInspector, Required]
        public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();

        public UniTaskCompletionSource InitializationSource { get; set; } = new UniTaskCompletionSource();

        [ShowInInspector] public bool IsInitialized => InitializationSource.Task.Status == UniTaskStatus.Succeeded;

        protected GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        public virtual async UniTask InitializeAsync()
        {
            InitializationSource.TrySetResult();
            SubscribeToEvents();
            await UniTask.Yield();
        }


        protected virtual void OnValidate()
        {
            if (EventRegistrar == null)
            {
                GameLoggerScriptable.LogError($"EventRegistrar is required for {typeof(T).Name}", this);
            }
        }

        protected virtual void OnEnable()
        {
            if (Application.isPlaying)
            {
                SubscribeToEvents();
            }

        }

        protected virtual void OnDisable()
        {
            if (Application.isPlaying)
            {
                UnsubscribeFromEvents();
            }

        }

        protected virtual void OnDestroy() { }

        public virtual void SubscribeToEvents()
        {

        }

        public virtual void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();
        }

        public async UniTask WaitForInitializationAsync()
        {
            await InitializationSource.Task;
        }

        public override UniTask<bool> ApplicationWantsToQuit()
        {
            UnsubscribeFromEvents();
            return base.ApplicationWantsToQuit();
        }

       
    }
}
