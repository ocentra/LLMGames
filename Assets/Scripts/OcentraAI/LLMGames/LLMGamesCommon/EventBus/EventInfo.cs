using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class EventInfo
    {
        private readonly ConcurrentDictionary<MonoScript, List<MonoScript>> eventUsages;

        public EventInfo()
        {
            eventUsages = new ConcurrentDictionary<MonoScript, List<MonoScript>>();

        }

        public bool TryAdd(MonoScript eventScript, MonoScript usageScript)
        {
            if (eventScript == null || usageScript == null)
            {
                Debug.Log("Event script or usage script cannot be null.");
                return false;
            }

            List<MonoScript> usages = eventUsages.GetOrAdd(eventScript, _ => new List<MonoScript>());

            if (!usages.Contains(usageScript))
            {
                usages.Add(usageScript);
                return true;
            }

            return false;
        }

        public bool TryAdd(MonoScript eventScript, List<MonoScript> usageScripts)
        {
            if (eventScript == null || usageScripts == null)
            {
                Debug.Log("Event script or usage scripts cannot be null.");
                return false;
            }

            foreach (MonoScript monoScript in usageScripts)
            {
                TryAdd(eventScript, monoScript);
            }

            return true;
        }

        public bool TryGetValue(MonoScript eventScript, out List<MonoScript> usages)
        {
            return eventUsages.TryGetValue(eventScript, out usages);
        }

        public Dictionary<MonoScript, List<MonoScript>> GetAll()
        {
            return new Dictionary<MonoScript, List<MonoScript>>(eventUsages);
        }

        public string GetUsageType()
        {
            return "type";
        }
    }

    [Serializable]
    public class UsageInfo
    {
        public EventInfo Publishers;
        public EventInfo Subscribers;

        public UsageInfo()
        {
            Publishers = new EventInfo();
            Subscribers = new EventInfo();
        }
    }
}
