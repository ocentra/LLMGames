using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class ChildElementHolder<T, TData> where T : ChildElement<TData>, new()
    {
        [ShowInInspector, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        public List<T> ViewItems;

        [ShowInInspector, DictionaryDrawerSettings]
        private readonly SortedDictionary<int, T> holders;

        [ShowInInspector, ReadOnly]
        public int ViewCapacity { get; set; }

        [ShowInInspector, ReadOnly]
        public int Count => holders.Count;

        public ChildElementHolder()
        {
            ViewCapacity = 5;
            ViewItems = new List<T>(ViewCapacity);
            holders = new SortedDictionary<int, T>();
        }
        public ChildElementHolder(int viewCapacity)
        {
            ViewCapacity = viewCapacity;
            ViewItems = new List<T>(ViewCapacity);
            holders = new SortedDictionary<int, T>();
        }

        public void Add(T value)
        {

            if (!holders.ContainsKey(value.Index))
            {
                holders[value.Index] = value;

                if (ViewItems.Count < ViewCapacity)
                {
                    ViewItems.Add(value);
                }
            }
            else
            {
                holders[value.Index] = value;

                int index = ViewItems.FindIndex(x => x.Index == value.Index);
                if (index != -1)
                {
                    ViewItems[index] = value;
                }
                else
                {
                    if (ViewItems.Count < ViewCapacity)
                    {
                        ViewItems.Add(value);
                    }
                }
            }

           
        }


        public void FilterView(TData targetData)
        {
            for (int currentViewIndex = 0; currentViewIndex < ViewItems.Count;)
            {
                T holder = ViewItems[currentViewIndex];
                if (!CheckGenreMatch(holder, targetData))
                {
                    bool replaced = false;

                    foreach (T potentialElement in holders.Values)
                    {
                        if (CheckGenreMatch(potentialElement, targetData) &&
                            !ViewItems.Exists(item => item.InstanceId == potentialElement.InstanceId))
                        {
                            ViewItems[currentViewIndex] = potentialElement; 
                            replaced = true;
                            break;
                        }
                    }

                    if (!replaced)
                    {
                        ViewItems.RemoveAt(currentViewIndex); 
                        continue; 
                    }
                }

                currentViewIndex++; 
            }

            while (ViewItems.Count < ViewCapacity)
            {
                bool added = false;

                foreach (T holder in holders.Values)
                {
                    if (CheckGenreMatch(holder, targetData) &&
                        !ViewItems.Exists(item => item.InstanceId == holder.InstanceId))
                    {
                        ViewItems.Add(holder);
                        added = true;

                        if (ViewItems.Count >= ViewCapacity)
                        {
                            break;
                        }
                    }
                }

                if (!added)
                {
                    break; 
                }
            }

            if (ViewItems.Count > ViewCapacity)
            {
                Debug.LogWarning($"ViewItems exceeded ViewCapacity. CurrentCount: {ViewItems.Count}, Capacity: {ViewCapacity}");
            }
        }



        public void ScrollRight(TData targetData = default)
        {
            if (holders.Count > ViewCapacity)
            {
                T last = ViewItems[^1];
                int currentKey = last.Index;

                int nextKey = -1;
                foreach (int key in holders.Keys)
                {
                    if (key > currentKey && (targetData == null || CheckGenreMatch(holders[key], targetData)))
                    {
                        nextKey = key;
                        break;
                    }
                }

                if (nextKey == -1)
                {
                    foreach (int key in holders.Keys)
                    {
                        if (targetData == null || CheckGenreMatch(holders[key], targetData))
                        {
                            nextKey = key;
                            break;
                        }
                    }
                }

                if (nextKey != -1 && holders.TryGetValue(nextKey, out T holder))
                {
                    ViewItems.Add(holder);
                    if (ViewItems.Count > ViewCapacity)
                    {
                        ViewItems.RemoveAt(0);
                    }
                }
            }

           
        }

        public void ScrollLeft(TData targetGameGenre = default)
        {
            if (holders.Count > ViewCapacity)
            {
                T first = ViewItems[0];
                int currentKey = first.Index;

                int prevKey = -1;
                foreach (int key in holders.Keys)
                {
                    if (key < currentKey && (targetGameGenre == null || CheckGenreMatch(holders[key], targetGameGenre)))
                    {
                        prevKey = key;
                    }
                    else
                    {
                        break;
                    }
                }

                if (prevKey == -1)
                {
                    foreach (int key in holders.Keys)
                    {
                        if (targetGameGenre == null || CheckGenreMatch(holders[key], targetGameGenre))
                        {
                            prevKey = key;
                        }
                    }
                }

                if (prevKey != -1 && holders.TryGetValue(prevKey, out T holder))
                {
                    ViewItems.Insert(0, holder);
                    if (ViewItems.Count > ViewCapacity)
                    {
                        ViewItems.RemoveAt(ViewItems.Count - 1);
                    }
                }
            }

           
        }

        private bool CheckGenreMatch(T value, TData targetData)
        {
            if (targetData is ILabeledItem {Id: 0})
            {
                return true;
            }
            return targetData != null && value.FilterContextData?.Equals(targetData) == true;
        }
        


        public void Clear()
        {
            holders.Clear();
            ViewItems.Clear();
        }

        public bool Contains(int instanceId)
        {
            foreach (KeyValuePair<int, T> holder in holders)
            {
                if (holder.Value.InstanceId == instanceId)
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<T> GetAll()
        {
            foreach (T element in holders.Values)
            {
                yield return element;
            }
        }

        public T GetItem(int instanceId)
        {
            foreach (KeyValuePair<int, T> holder in holders)
            {
                if (holder.Value.InstanceId == instanceId)
                {
                    return holder.Value;
                }
            }
            return default;
        }
    }
}