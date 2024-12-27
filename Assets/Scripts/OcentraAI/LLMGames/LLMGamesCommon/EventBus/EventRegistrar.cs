using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class EventRegistrar : IEventRegistrar, IDisposable
    {
        [ShowInInspector, DictionaryDrawerSettings] protected ConcurrentDictionary<EventRegistrarBase, byte> Subscriptions { get; set; } = new();
        [ShowInInspector, DictionaryDrawerSettings] protected static ConcurrentDictionary<Type, ConcurrentDictionary<Type, byte>> DependencyTracker { get; set; } = new();

        public void Subscribe<T>(Action<T> handler) where T : IEventArgs
        {
            if (isDisposed) return;

            if (typeof(T).IsSubclassOf(typeof(WaitForInitializationEvent)))
            {
                OperationResult<WaitForInitializationData> result = HandleCircularDependency(handler).GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    SubscribeInternal(handler, result.Value);
                }
                else
                {
                    Debug.LogError($"Failed to subscribe Action<T>: {result.ErrorMessage}");
                }
            }
            else
            {
                SubscribeInternal(handler, null);
            }
        }

        public void Subscribe<T>(Func<T, UniTask> handler) where T : IEventArgs
        {
            if (isDisposed) return;

            if (typeof(T).IsSubclassOf(typeof(WaitForInitializationEvent)))
            {
                OperationResult<WaitForInitializationData> result = HandleCircularDependency(handler).GetAwaiter().GetResult();

                if (result.IsSuccess)
                {
                    SubscribeInternal(handler, result.Value);
                }
                else
                {
                    Debug.LogError($"Failed to subscribe Func<T>: {result.ErrorMessage}");
                }
            }
            else
            {
                SubscribeInternal(handler, null);
            }
        }


        protected async UniTask<OperationResult<WaitForInitializationData>> HandleCircularDependency<T>(Action<T> handler) where T : IEventArgs
        {
            OperationResult<WaitForInitializationData> handleCircularDependencyInternal = await HandleCircularDependencyInternal<T>(handler);
            return handleCircularDependencyInternal;
        }

        protected async UniTask<OperationResult<WaitForInitializationData>> HandleCircularDependency<T>(Func<T, UniTask> handler) where T : IEventArgs
        {
            OperationResult<WaitForInitializationData> handleCircularDependencyInternal = await HandleCircularDependencyInternal<T>(handler);
            return handleCircularDependencyInternal;
        }

        protected void SubscribeInternal<T>(Action<T> handler, WaitForInitializationData data) where T : IEventArgs
        {
            if (isDisposed)
            {
                return;
            }
            if (handler != null && Subscriptions.TryAdd(new EventRegistrarSync<T>(handler), 0))
            {
                EventBus.Instance.Subscribe(handler);
                RemoveDependency(data).Forget();
            }
        }

        protected void SubscribeInternal<T>(Func<T, UniTask> handler, WaitForInitializationData data) where T : IEventArgs
        {
            if (isDisposed)
            {
                return;
            }

            if (handler != null && Subscriptions.TryAdd(new EventRegistrarAsync<T>(handler), 0))
            {
                EventBus.Instance.SubscribeAsync(handler);
                RemoveDependency(data).Forget();
            }
        }



        private async UniTask<OperationResult<WaitForInitializationData>> HandleCircularDependencyInternal<T>(object handler) where T : IEventArgs
        {

            if (Activator.CreateInstance(typeof(T)) is WaitForInitializationEvent initializationEvent)
            {

                WaitForInitializationData data = new WaitForInitializationData(
                    initializationEvent.CompletionSource,
                    initializationEvent.SourceType,
                    initializationEvent.TargetType,
                    initializationEvent.Priority
                );

                if (data.SourceType != null && data.TargetType != null)
                {
                    if (Object.FindAnyObjectByType(data.TargetType) is IMonoBehaviourBase monoBehaviourBase && monoBehaviourBase.InitializationSource.Task.Status == UniTaskStatus.Succeeded)
                    {
                        return OperationResult<WaitForInitializationData>.Success(data);
                    }

                    if (data.SourceType == data.TargetType)
                    {
                        return OperationResult<WaitForInitializationData>.Failure($"Circular dependency detected: {data.SourceType.Name} -> {data.TargetType.Name}. Tracker: {string.Join(", ", DependencyTracker.Keys)}");
                    }

                    if (!DependencyTracker.ContainsKey(data.SourceType))
                    {
                        DependencyTracker[data.SourceType] = new ConcurrentDictionary<Type, byte>();
                    }

                    DependencyTracker[data.SourceType].TryAdd(data.TargetType, 0);

                    if (HasCircularDependency(data))
                    {
                        bool resolved = await ResolveDependenciesWithQueue(handler, data);
                        if (!resolved)
                        {
                            return OperationResult<WaitForInitializationData>.Failure($"Unresolvable circular dependency: {data.SourceType.Name} -> {data.TargetType.Name}");
                        }
                    }

                    return OperationResult<WaitForInitializationData>.Success(data);
                }

                return  OperationResult<WaitForInitializationData>.Failure($"Missing source or target type for event {typeof(T).Name}");
            }

            return  OperationResult<WaitForInitializationData>.Failure("Failed to create initialization event");
        }


        private async UniTask<bool> ResolveDependenciesWithQueue(object handler, WaitForInitializationData initialData)
        {
            Queue<WaitForInitializationData> resolutionQueue = new();
            resolutionQueue.Enqueue(initialData);

            HashSet<(Type Source, Type Target)> visitedDependencies = new();

            while (resolutionQueue.Count > 0)
            {
                WaitForInitializationData currentData = resolutionQueue.Dequeue();

                if (!visitedDependencies.Add((currentData.SourceType, currentData.TargetType)))
                {
                    Debug.LogError($"Circular dependency detected: {currentData.SourceType.Name} -> {currentData.TargetType.Name}");
                    return false;
                }

                if (handler is Action<IEventArgs> actionHandler)
                {
                    SubscribeInternal(actionHandler, currentData);
                }
                else if (handler is Func<IEventArgs, UniTask> funcHandler)
                {
                    SubscribeInternal(funcHandler, currentData);
                }
                else
                {
                    Debug.LogError($"Handler type mismatch or invalid source type for {currentData.SourceType.Name} -> {currentData.TargetType.Name}");
                    return false;
                }


                await currentData.CompletionSource.Task;

                if (DependencyTracker.TryGetValue(currentData.TargetType, out ConcurrentDictionary<Type, byte> dependencies))
                {
                    foreach (Type dependency in dependencies.Keys)
                    {
                        WaitForInitializationData nextData = new WaitForInitializationData(
                            new UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>>(),
                            currentData.TargetType,
                            dependency,
                            currentData.Priority - 1
                        );
                        resolutionQueue.Enqueue(nextData);
                    }
                }

                RemoveDependency(currentData).Forget();
            }

            return true;
        }


        protected class WaitForInitializationData
        {
            public UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>> CompletionSource;
            public Type SourceType;
            public Type TargetType;
            public int Priority;
            public WaitForInitializationData(UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>> completionSource, Type sourceType, Type targetType, int priority)
            {
                CompletionSource = completionSource;
                SourceType = sourceType;
                TargetType = targetType;
                Priority = priority;
            }
        }


        private bool HasCircularDependency(WaitForInitializationData data)
        {
            if (!DependencyTracker.TryGetValue(data.TargetType, out ConcurrentDictionary<Type, byte> dependencies))
                return false;

            if (dependencies.ContainsKey(data.SourceType))
                return true;

            foreach (Type dependency in dependencies.Keys)
            {
                WaitForInitializationData nextData = new WaitForInitializationData(data.CompletionSource, data.SourceType, dependency, data.Priority);
                if (HasCircularDependency(nextData))
                    return true;
            }

            return false;
        }




        public void UnsubscribeAll()
        {
            foreach (EventRegistrarBase entry in Subscriptions.Keys)
            {
                try
                {
                    entry.Unsubscribe(EventBus.Instance);
                }
                catch (Exception ex)
                {

                    Debug.LogError($"[EventRegistrar] Failed to unsubscribe event of type {entry.GetType()}: {ex.Message}");
                }
            }
            Subscriptions.Clear();
        }

        protected async UniTaskVoid RemoveDependency(WaitForInitializationData data)
        {
            if (data != null)
            {
                await data.CompletionSource.Task;

                if (DependencyTracker.TryGetValue(data.SourceType, out ConcurrentDictionary<Type, byte> dependencies))
                {
                    if (dependencies.TryRemove(data.TargetType, out _) && dependencies.IsEmpty)
                    {
                        DependencyTracker.TryRemove(data.SourceType, out _);
                    }
                }
            }
        }



        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed) return;

            UnsubscribeAll();

            Subscriptions.Clear();

            foreach (Type source in DependencyTracker.Keys)
            {
                if (DependencyTracker.TryRemove(source, out ConcurrentDictionary<Type, byte> dependencies))
                {
                    dependencies.Clear();
                }
            }

            isDisposed = true;
        }

    }

    [Serializable]
    public abstract class EventRegistrarBase
    {
        public abstract void Unsubscribe(EventBus eventBus);

        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }


    [Serializable]
    public class EventRegistrarSync<T> : EventRegistrarBase where T : IEventArgs
    {
        [ShowInInspector]
        public Action<T> Handler { get; }

        public EventRegistrarSync(Action<T> handler)
        {
            Handler = handler;
        }

        public override void Unsubscribe(EventBus eventBus)
        {
            eventBus.Unsubscribe(Handler);
        }

        public override bool Equals(object obj)
        {
            if (obj is EventRegistrarSync<T> other)
            {
                return Handler != null && Handler.Equals(other.Handler);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Handler != null ? Handler.GetHashCode() : 0;
        }
    }


    [Serializable]
    public class EventRegistrarAsync<T> : EventRegistrarBase where T : IEventArgs
    {
        [ShowInInspector]
        public Func<T, UniTask> Handler { get; }

        public EventRegistrarAsync(Func<T, UniTask> handler)
        {
            Handler = handler;
        }

        public override void Unsubscribe(EventBus eventBus)
        {
            eventBus.UnsubscribeAsync(Handler);
        }

        public override bool Equals(object obj)
        {
            if (obj is EventRegistrarAsync<T> other)
            {
                return Handler != null && Handler.Equals(other.Handler);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Handler != null ? Handler.GetHashCode() : 0;
        }
    }

}
