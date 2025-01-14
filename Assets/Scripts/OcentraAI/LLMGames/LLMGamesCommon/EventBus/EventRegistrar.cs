using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class EventRegistrar : IEventRegistrar, IDisposable
    {
        [ShowInInspector, DictionaryDrawerSettings] protected ConcurrentDictionary<EventRegistrarBase, byte> Subscriptions { get; set; } = new();

        public void Subscribe<T>(Action<T> handler) where T : IEventArgs
        {
            if (isDisposed) return;

            if (handler != null && Subscriptions.TryAdd(new EventRegistrarSync<T>(handler), 0))
            {
                EventBus.Instance.Subscribe(handler);

            }
        }

        public void Subscribe<T>(Func<T, UniTask> handler) where T : IEventArgs
        {
            if (isDisposed) return;

            if (handler != null && Subscriptions.TryAdd(new EventRegistrarAsync<T>(handler), 0))
            {
                EventBus.Instance.SubscribeAsync(handler);

            }
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
        

        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed) return;

            UnsubscribeAll();
            Subscriptions.Clear();
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
