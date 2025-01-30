using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    [CreateAssetMenu(fileName = nameof(EventBus), menuName = "LLMGames/EventBus")]
    [GlobalConfig("Assets/Resources/")]
    public class EventBus : CustomGlobalConfig<EventBus>
    {
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<Type, ConcurrentQueue<Delegate>> Subscribers { get; set; } = new ConcurrentDictionary<Type, ConcurrentQueue<Delegate>>();

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<Type, ConcurrentQueue<Delegate>> AsyncSubscribers { get; set; } = new ConcurrentDictionary<Type, ConcurrentQueue<Delegate>>();

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<Type, ConcurrentQueue<IEventArgs>> QueuedEvents { get; set; } = new ConcurrentDictionary<Type, ConcurrentQueue<IEventArgs>>();

        private readonly ConcurrentDictionary<Guid, bool> processedEvents = new();


        public void Subscribe<T>(Action<T> subscriber, bool force = false) where T : IEventArgs
        {
            Type eventType = typeof(T);

            ConcurrentQueue<Delegate> subscriberQueue = Subscribers.GetOrAdd(eventType, _ => new ConcurrentQueue<Delegate>());

            if (force || !subscriberQueue.Contains(subscriber))
            {
                subscriberQueue.Enqueue(subscriber);
            }

            ProcessQueuedEventsAsync<T>().Forget();
        }

        public void SubscribeAsync<T>(Func<T, UniTask> subscriber, bool force = false) where T : IEventArgs
        {
            Type eventType = typeof(T);

            ConcurrentQueue<Delegate> subscriberQueue = AsyncSubscribers.GetOrAdd(eventType, _ => new ConcurrentQueue<Delegate>());

            if (force || !subscriberQueue.Contains(subscriber))
            {
                subscriberQueue.Enqueue(subscriber);
            }

            ProcessQueuedEventsAsync<T>().Forget();
        }



        public void Unsubscribe<T>(Action<T> subscriber) where T : IEventArgs
        {
            Type eventType = typeof(T);

            if (Subscribers.TryGetValue(eventType, out ConcurrentQueue<Delegate> subscriberQueue))
            {
                ConcurrentQueue<Delegate> tempQueue = new ConcurrentQueue<Delegate>();

                while (subscriberQueue.TryDequeue(out Delegate existingSubscriber))
                {
                    if (!(existingSubscriber.Target == subscriber.Target && existingSubscriber.Method == subscriber.Method))
                    {
                        tempQueue.Enqueue(existingSubscriber); 
                    }
                }

                Subscribers[eventType] = tempQueue; 
            }
        }


        public void UnsubscribeAsync<T>(Func<T, UniTask> subscriber) where T : IEventArgs
        {
            Type eventType = typeof(T);

            if (AsyncSubscribers.TryGetValue(eventType, out ConcurrentQueue<Delegate> subscriberQueue))
            {
                ConcurrentQueue<Delegate> tempQueue = new ConcurrentQueue<Delegate>();

                while (subscriberQueue.TryDequeue(out Delegate existingSubscriber))
                {
                    if (!(existingSubscriber.Target == subscriber.Target && existingSubscriber.Method == subscriber.Method))
                    {
                        tempQueue.Enqueue(existingSubscriber); 
                    }
                }

                AsyncSubscribers[eventType] = tempQueue; 
            }
        }


        public void Publish<T>(T eventArgs, bool force = false) where T : IEventArgs
        {
            try
            {
                if (!force)
                {
                    if (!eventArgs.IsRePublishable)
                    {
                        if (processedEvents.ContainsKey(eventArgs.UniqueIdentifier))
                        {
                            return;
                        }
                    }
                }

                UniTask<bool>.Awaiter awaiter = PublishInternal(eventArgs, false).GetAwaiter();
                if (awaiter.IsCompleted)
                {
                    eventArgs.Dispose();
                    processedEvents.TryRemove(eventArgs.UniqueIdentifier, out _);
                }
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in Publish: {ex.Message} {ex.StackTrace}", this);
            }
        }

        public async UniTask<bool> PublishAsync<T>(T eventArgs, bool force = false) where T : IEventArgs
        {
            try
            {
                if (!force)
                {
                    if (!eventArgs.IsRePublishable)
                    {
                        if (processedEvents.ContainsKey(eventArgs.UniqueIdentifier))
                        {
                            return false;
                        }
                    }
                }
                bool publishInternal = await PublishInternal(eventArgs, true);
                if (publishInternal)
                {
                    eventArgs.Dispose();
                    processedEvents.TryRemove(eventArgs.UniqueIdentifier, out _);
                }
                return publishInternal;
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in PublishAsync: {ex.Message} {ex.StackTrace}", this);
                return false;
            }
        }

        private async UniTask<bool> PublishInternal<T>(T eventArgs, bool awaitAsyncSubscribers) where T : IEventArgs
        {
            processedEvents.TryAdd(eventArgs.UniqueIdentifier, false);

            bool eventHandled = await HandlePublishActionAsync(eventArgs, awaitAsyncSubscribers);

            processedEvents[eventArgs.UniqueIdentifier] = eventHandled;

            if (!eventHandled)
            {

                bool success = await QueueEvent(eventArgs);

                if (success)
                {
                    GameLoggerScriptable.LogWarning($"Event of type {typeof(T).Name} was published but " +
                                                    $"not all potential subscribers are registered. Queuing for later processing.", this);
                }
                else
                {
                    GameLoggerScriptable.LogError($"Failed to queue event of type {typeof(T).Name}.", this);
                    return false;
                }
            }

            return eventHandled;
        }

        private async UniTask<bool> HandlePublishActionAsync<T>(T eventArgs, bool awaitAsyncSubscribers) where T : IEventArgs
        {
            Type eventType = typeof(T);
            bool eventHandled = false;

            if (Subscribers.TryGetValue(eventType, out ConcurrentQueue<Delegate> subscriberList) && subscriberList != null)
            {
                foreach (Delegate subscriber in subscriberList)
                {
                    try
                    {
                        ((Action<T>)subscriber)(eventArgs);
                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.LogError($"Error in subscriber for event type {eventType.Name}: {ex.Message} {ex.StackTrace}", this);
                    }
                }
                eventHandled = true;
            }

            if (AsyncSubscribers.TryGetValue(eventType, out ConcurrentQueue<Delegate> asyncSubscriberList) && asyncSubscriberList != null)
            {
                foreach (Delegate subscriber in asyncSubscriberList)
                {
                    try
                    {
                        if (subscriber is Func<T, UniTask> uniTaskSubscriber)
                        {
                            if (awaitAsyncSubscribers)
                            {
                                await uniTaskSubscriber(eventArgs);
                            }
                            else
                            {
                                uniTaskSubscriber(eventArgs).Forget();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.LogError($"Error in async subscriber for event type {eventType.Name}: {ex.Message} {ex.StackTrace}", this);
                    }
                }
                eventHandled = true;
            }

            await UniTask.Yield();
            return eventHandled;
        }

        private async UniTask<bool> QueueEvent<T>(T eventArgs) where T : IEventArgs
        {
            try
            {
                Type eventType = typeof(T);

                if (!QueuedEvents.TryGetValue(eventType, out ConcurrentQueue<IEventArgs> eventQueue))
                {
                    eventQueue = new ConcurrentQueue<IEventArgs>();
                    QueuedEvents[eventType] = eventQueue;
                }

                eventQueue.Enqueue(eventArgs);

                await UniTask.Yield();
                return true;
            }
            catch (Exception e)
            {
                GameLoggerScriptable.LogException($"QueueEvent failed: {e.Message} {e.StackTrace}", this);
                return false;
            }
        }

        private async UniTask ProcessQueuedEventsAsync<T>() where T : IEventArgs
        {
            Type eventType = typeof(T);

            if (QueuedEvents.TryGetValue(eventType, out ConcurrentQueue<IEventArgs> eventQueue) && eventQueue != null)
            {
                const int batchSize = 10;
                const int maxRetryAttempts = 5;
                Dictionary<IEventArgs, int> retryAttempts = new();
                ConcurrentQueue<IEventArgs> failedEventsQueue = new();

                while (!eventQueue.IsEmpty)
                {
                    for (int i = 0; i < batchSize && eventQueue.TryDequeue(out IEventArgs queuedEvent); i++)
                    {
                        try
                        {
                            await HandlePublishActionAsync((T)queuedEvent, false).Timeout(TimeSpan.FromSeconds(5));
                        }
                        catch (TimeoutException)
                        {
                            GameLoggerScriptable.LogError($"Timeout occurred while processing PublishAction for event type {eventType}", this);
                            AddToFailedQueue(queuedEvent, failedEventsQueue, retryAttempts, maxRetryAttempts);
                        }
                        catch (Exception ex)
                        {
                            GameLoggerScriptable.LogException($"Unexpected error during PublishAction for event type {eventType}: {ex.Message}\n{ex.StackTrace}", this);
                            AddToFailedQueue(queuedEvent, failedEventsQueue, retryAttempts, maxRetryAttempts);
                        }
                    }

                    await UniTask.Yield();
                }

                if (!failedEventsQueue.IsEmpty)
                {
                    while (failedEventsQueue.TryDequeue(out IEventArgs failedEvent))
                    {
                        if (retryAttempts.TryGetValue(failedEvent, out int attempts) && attempts < maxRetryAttempts)
                        {
                            eventQueue.Enqueue(failedEvent);
                        }
                    }

                    QueuedEvents[eventType] = eventQueue;
                }
                else
                {
                    QueuedEvents.TryRemove(eventType, out _);
                }
            }
        }

        private void AddToFailedQueue(IEventArgs eventArgs, ConcurrentQueue<IEventArgs> failedEventsQueue, Dictionary<IEventArgs, int> retryAttempts, int maxRetryAttempts)
        {
            if (!retryAttempts.ContainsKey(eventArgs))
            {
                retryAttempts[eventArgs] = 1;
            }
            else
            {
                retryAttempts[eventArgs]++;
            }

            if (retryAttempts[eventArgs] <= maxRetryAttempts)
            {
                failedEventsQueue.Enqueue(eventArgs);
            }
            else
            {
                GameLoggerScriptable.LogWarning($"Event {eventArgs.GetType().Name} has exceeded maximum retry attempts and will be dropped.", this);
            }
        }




        public void Clear()
        {
            if (!Subscribers.IsEmpty || !AsyncSubscribers.IsEmpty || !QueuedEvents.IsEmpty)
            {
                GameLoggerScriptable.LogWarning("EventBus is being cleared. All current subscribers and queued events will be removed.", this);
            }

            Subscribers = new ConcurrentDictionary<Type, ConcurrentQueue<Delegate>>();
            AsyncSubscribers = new ConcurrentDictionary<Type, ConcurrentQueue<Delegate>>();
            QueuedEvents = new ConcurrentDictionary<Type, ConcurrentQueue<IEventArgs>>();

            GameLoggerScriptable.LogWarning("All event subscriptions and queues have been cleared.", this);
        }



        #region EventBusManager

        [SerializeField, FoldoutGroup("Event Info")] public List<string> AssemblyFiles = new List<string>();
        [SerializeField, FoldoutGroup("Event Info")] public List<ScriptInfo> AllScripts = new List<ScriptInfo>();
        [SerializeField, FoldoutGroup("Event Info")] public List<ScriptInfo> EventMonoScript = new List<ScriptInfo>();
        [SerializeField, FoldoutGroup("Event Info")] public UsageInfo UsageInfo = new UsageInfo();


        #endregion
    }
}
