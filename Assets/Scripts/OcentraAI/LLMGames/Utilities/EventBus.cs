using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Utilities
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> subscriber) where T : EventArgs
        {
            var eventType = typeof(T);
            if (!Subscribers.TryGetValue(eventType, out var subscriberList))
            {
                subscriberList = new List<Delegate>();
                Subscribers.Add(eventType, subscriberList);
            }
            subscriberList.Add(subscriber);
        }

        public static void Unsubscribe<T>(Action<T> subscriber) where T : EventArgs
        {
            var eventType = typeof(T);
            if (Subscribers.TryGetValue(eventType, out var subscriberList))
            {
                subscriberList.Remove(subscriber);
            }
        }

        public static void Publish<T>(T eventArgs) where T : EventArgs
        {
            var eventType = typeof(T);
            if (Subscribers.TryGetValue(eventType, out var subscriberList))
            {
                foreach (var subscriber in subscriberList)
                {
                    ((Action<T>)subscriber)(eventArgs);
                }
            }
        }
    }
}