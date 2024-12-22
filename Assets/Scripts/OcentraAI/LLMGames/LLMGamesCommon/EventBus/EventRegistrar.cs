using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class EventRegistrar : IEventRegistrar, IDisposable
    {
        [ShowInInspector] private List<EventRegistrarBase> Subscriptions { get;  set; }

        private readonly object subscriptionsLock = new();

        public EventRegistrar()
        {
            Subscriptions = new List<EventRegistrarBase>();
        }

        public void Subscribe<T>(Action<T> handler) where T : IEventArgs
        {
            lock (subscriptionsLock)
            {
                if (!Subscriptions.Exists(entry => entry is EventRegistrarSync<T> sync && sync.Handler.Equals(handler)))
                {
                    EventBus.Instance.Subscribe(handler);
                    Subscriptions.Add(new EventRegistrarSync<T>(handler));
                }
            }
        }

        public void Subscribe<T>(Func<T, UniTask> handler) where T : IEventArgs
        {
            lock (subscriptionsLock)
            {
                if (!Subscriptions.Exists(entry => entry is EventRegistrarAsync<T> asyncReg && asyncReg.Handler.Equals(handler)))
                {
                    EventBus.Instance.SubscribeAsync(handler);
                    Subscriptions.Add(new EventRegistrarAsync<T>(handler));
                }
            }
        }

        public void UnsubscribeAll()
        {
            lock (subscriptionsLock)
            {
                foreach (var entry in Subscriptions)
                {
                    try
                    {
                        entry.Unsubscribe(EventBus.Instance);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to unsubscribe event of type {entry.GetType()}: {ex.Message}");
                    }
                }
                Subscriptions.Clear();
            }
        }



        private void LogError(string message)
        {
            Debug.LogError($"[EventRegistrar] {message}");
        }

        public void Dispose()
        {
            UnsubscribeAll();
        }
    }

    [Serializable]
    public abstract class EventRegistrarBase
    {
        public abstract void Unsubscribe(EventBus eventBus);
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
    }
}
