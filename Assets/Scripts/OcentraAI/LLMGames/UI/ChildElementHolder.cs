using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class ChildElementHolder<T, TData> where T : ChildElement<TData>, new()
    {
        [ShowInInspector, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        public List<T> ViewItems;
        [ShowInInspector, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        public List<T> FilteredItems =  new List<T>();

        [ShowInInspector, DictionaryDrawerSettings]
        private readonly SortedDictionary<int, T> holders;

        [ShowInInspector, ReadOnly]
        public int ViewCapacity { get; set; }

        [ShowInInspector, ReadOnly]
        public int Count => holders.Count;
        [ShowInInspector] public int FilterItemsCount;
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
            FilteredItems.Clear();

            foreach (T holder in holders.Values)
            {
                if (CheckMatch(holder, targetData))
                {
                    FilteredItems.Add(holder);
                }
            }
            FilterItemsCount = FilteredItems.Count;

            ViewItems.Clear();

            for (int i = 0; i < FilteredItems.Count && i < ViewCapacity; i++)
            {
                ViewItems.Add(FilteredItems[i]);
            }
            

        }
        

        public void ScrollRight(TData data)
        {
            if (FilterItemsCount <= ViewCapacity)
            {
                return;
            }

            if (holders.Count > ViewCapacity)
            {
                T last = ViewItems[^1];
                int currentKey = last.Index;

                int nextKey = -1;
                foreach (int key in holders.Keys)
                {
                    if (key > currentKey && CheckMatch(holders[key], data))
                    {
                        nextKey = key;
                        break;
                    }
                }

                if (nextKey == -1)
                {
                    foreach (int key in holders.Keys)
                    {
                        if (CheckMatch(holders[key], data))
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

        public void ScrollLeft(TData data)
        {
            if (FilterItemsCount <= ViewCapacity)
            {
                return;
            }

            if (holders.Count > ViewCapacity)
            {
                T first = ViewItems[0];
                int currentKey = first.Index;

                int prevKey = -1;
                foreach (int key in holders.Keys)
                {
                    if (key < currentKey && CheckMatch(holders[key], data))
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
                        if (CheckMatch(holders[key], data))
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

        private bool CheckMatch(T value, TData targetData)
        {
            if (targetData is ILabeledItem { Id: 0 })
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