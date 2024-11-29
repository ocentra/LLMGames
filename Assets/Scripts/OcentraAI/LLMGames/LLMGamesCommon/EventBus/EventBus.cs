using Cysharp.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor.Compilation;
using UnityEditor;
#endif

using UnityEngine;
using Assembly = System.Reflection.Assembly;
using System.Reflection;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;

namespace OcentraAI.LLMGames.Events
{
    [CreateAssetMenu(fileName = nameof(EventBus), menuName = "LLMGames/EventBus")]
    [GlobalConfig("Assets/Resources/")]
    public class EventBus : CustomGlobalConfig<EventBus>
    {
        #region main


        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<Type, List<Delegate>> Subscribers { get; } = new ConcurrentDictionary<Type, List<Delegate>>();

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<Type, List<Delegate>> AsyncSubscribers { get; } = new ConcurrentDictionary<Type, List<Delegate>>();

        [ShowInInspector, DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        private ConcurrentDictionary<Type, ConcurrentQueue<SubscribeQueue>> QueuedEvents { get; } = new ConcurrentDictionary<Type, ConcurrentQueue<SubscribeQueue>>();
        [ShowInInspector] private float InitializationTimeout { get; set; } = 5f;
        [ShowInInspector] private float CheckInterval { get; set; } = 0.1f;

        [SerializeField]
        public class SubscribeQueue
        {
            public IEventArgs Event { get; }
            public List<Type> MonoList { get; }
            public SubscribeQueue(IEventArgs @event, List<Type> monoList)
            {
                Event = @event;
                MonoList = monoList;
            }
        }

        public void Subscribe<T>(Action<T> subscriber) where T : IEventArgs
        {
            Type eventType = typeof(T);
            if (!Subscribers.TryGetValue(eventType, out List<Delegate> subscriberList))
            {
                subscriberList = new List<Delegate>();
                Subscribers[eventType] = subscriberList;
            }

            subscriberList.Add(subscriber);

            ProcessQueuedEventsAsync<T>().Forget();

        }

        public void SubscribeAsync<T>(Func<T, UniTask> subscriber) where T : IEventArgs
        {
            Type eventType = typeof(T);
            if (!AsyncSubscribers.TryGetValue(eventType, out List<Delegate> subscriberList))
            {
                subscriberList = new List<Delegate>();
                AsyncSubscribers[eventType] = subscriberList;
            }

            subscriberList.Add(subscriber);

            ProcessQueuedEventsAsync<T>().Forget();
        }

        public void Unsubscribe<T>(Action<T> subscriber) where T : IEventArgs
        {
            if (Subscribers.TryGetValue(typeof(T), out List<Delegate> subscriberList))
            {
                subscriberList.Remove(subscriber);
            }
        }

        public void UnsubscribeAsync<T>(Func<T, UniTask> subscriber) where T : IEventArgs
        {
            if (AsyncSubscribers.TryGetValue(typeof(T), out List<Delegate> subscriberList))
            {
                subscriberList.RemoveAll(s => s.Target == subscriber.Target && s.Method == subscriber.Method);
            }
        }

        public void Publish<T>(T eventArgs) where T : IEventArgs
        {
            try
            {
                PublishInternal(eventArgs, false).Forget();
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in Publish: {ex.Message} {ex.StackTrace}", this);
            }
        }

        public async UniTask<bool> PublishAsync<T>(T eventArgs) where T : IEventArgs
        {
            try
            {
                return await PublishInternal(eventArgs, true);
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in PublishAsync: {ex.Message} {ex.StackTrace}", this);
                return false;
            }
        }

        private async UniTask<bool> PublishInternal<T>(T eventArgs, bool awaitAsyncSubscribers) where T : IEventArgs
        {

            bool eventHandled = await HandlePublishActionAsync(eventArgs, awaitAsyncSubscribers);

            if (!eventHandled)
            {
                await UniTask.WaitForSeconds(2);

                if (expectedEventTypes is { Count: > 0 })
                {
                    OperationResult<List<Type>> result = await TryGetSubscribers(eventArgs, expectedEventTypes);

                    if (result.IsSuccess)
                    {
                        List<Type> potentialSubscribers = result.Value;

                        if (potentialSubscribers is { Count: > 0 })
                        {
                            bool allSubscribersHandled = true;

                            foreach (Type type in potentialSubscribers)
                            {
                                bool isHandled = Subscribers.ContainsKey(type) || AsyncSubscribers.ContainsKey(type);

                                if (!isHandled)
                                {
                                    allSubscribersHandled = false;
                                    break;
                                }
                            }

                            if (!allSubscribersHandled)
                            {
                                bool success = await QueueEvent(eventArgs, potentialSubscribers);

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
                        }
                        else
                        {
                            GameLoggerScriptable.LogError($"Event of type {typeof(T).Name} was published but has no subscribers and is not part of expected event types.", this);
                            return false;
                        }
                    }
                    else
                    {
                      //  GameLoggerScriptable.LogError($"Failed to Get expected event types for {typeof(T).Name}.", this);
                        return false;
                    }
                }
                else
                {
                    GameLoggerScriptable.LogError($"Event of type {typeof(T).Name} was published with no subscribers and no expected types.", this);
                    return false;
                }
            }

            return eventHandled;
        }

        private async UniTask<bool> HandlePublishActionAsync<T>(T eventArgs, bool awaitAsyncSubscribers) where T : IEventArgs
        {
            List<Delegate> subscriberList = Subscribers.TryGetValue(typeof(T), out List<Delegate> tempList) ? tempList.ToList() : null;

            List<Delegate> asyncSubscriberList = AsyncSubscribers.TryGetValue(typeof(T), out List<Delegate> asyncTempList) ? asyncTempList.ToList() : null;

            bool eventHandled = false;

            if (subscriberList != null)
            {
                foreach (Delegate subscriber in subscriberList)
                {
                    try
                    {
                        ((Action<T>)subscriber)(eventArgs);
                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.LogError($"Error in subscriber for event type {typeof(T).Name}: {ex.Message} {ex.StackTrace}", this);
                    }
                }

                eventHandled = true;
            }

            if (asyncSubscriberList != null)
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
                        GameLoggerScriptable.LogError($"Error in async subscriber for event type {typeof(T).Name}: {ex.Message} {ex.StackTrace}", this);
                    }
                }

                eventHandled = true;
            }

            await UniTask.Yield();
            return eventHandled;
        }

        public async UniTask<OperationResult<List<Type>>> TryGetSubscribers<T>(T eventArgs, List<EventUsageInfo> eventUsageInfos)
        {
            List<Type> subscribers = new List<Type>();
            try
            {

                if (eventUsageInfos is { Count: > 0 })
                {
                    foreach (EventUsageInfo eventUsageInfo in eventUsageInfos)
                    {
                        foreach (ScriptInfo subscriber in eventUsageInfo.SubscribeMethods)
                        {
                            if (eventUsageInfo.ScriptInfo.ScriptType == typeof(T) && typeof(MonoBehaviour).IsAssignableFrom(subscriber.ScriptType))
                            {
                                subscribers.Add(subscriber.ScriptType);
                            }
                        }
                    }
                }
                await UniTask.Yield();

                return new OperationResult<List<Type>>(false, subscribers, 0, "Players not ready after maximum retries");
            }
            catch (Exception e)
            {
                GameLoggerScriptable.LogException($"TryGetSubscribers failed: {e.Message} {e.StackTrace}", this);

                return new OperationResult<List<Type>>(false, null, 0, e.Message);

            }

        }


        private async UniTask<bool> QueueEvent<T>(T eventArgs, List<Type> monoList) where T : IEventArgs
        {
            try
            {
                Type eventType = typeof(T);

                if (!QueuedEvents.TryGetValue(eventType, out ConcurrentQueue<SubscribeQueue> eventQueue))
                {
                    eventQueue = new ConcurrentQueue<SubscribeQueue>();
                    QueuedEvents[eventType] = eventQueue;
                }

                eventQueue.Enqueue(new SubscribeQueue(eventArgs, monoList));

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

            if (QueuedEvents.TryGetValue(eventType, out ConcurrentQueue<SubscribeQueue> eventQueue) && eventQueue != null)
            {
                ConcurrentQueue<SubscribeQueue> tempQueue = new();
                const int batchSize = 10;

                while (!eventQueue.IsEmpty)
                {
                    for (int i = 0; i < batchSize && eventQueue.TryDequeue(out SubscribeQueue queuedItem); i++)
                    {
                        bool allSubscribersHandled = true;

                        foreach (Type type in queuedItem.MonoList)
                        {
                            if (!Subscribers.ContainsKey(type) && !AsyncSubscribers.ContainsKey(type))
                            {
                                allSubscribersHandled = false;
                                break;
                            }
                        }

                        try
                        {
                            if (allSubscribersHandled)
                            {
                                await HandlePublishActionAsync((T)queuedItem.Event, false).Timeout(TimeSpan.FromSeconds(5));
                            }
                            else
                            {
                              
                                tempQueue.Enqueue(queuedItem);
                            }
                        }
                        catch (TimeoutException)
                        {
                            GameLoggerScriptable.LogError($"Timeout occurred while processing PublishAction for event type {eventType}", this);
                        }
                        catch (Exception ex)
                        {
                            GameLoggerScriptable.LogException($"Unexpected error during PublishAction for event type {eventType}: {ex.Message}\n{ex.StackTrace}", this);
                        }
                    }

                    await UniTask.Yield(); 
                }

                if (!tempQueue.IsEmpty)
                {
                    QueuedEvents[eventType] = tempQueue;
                }
                else
                {
                    QueuedEvents.TryRemove(eventType, out _); 
                }
            }
        }



        public void Clear()
        {
            // Ensure any ongoing event processing is safely terminated before clearing
            if (!Subscribers.IsEmpty || !AsyncSubscribers.IsEmpty || !QueuedEvents.IsEmpty)
            {
                GameLoggerScriptable.LogWarning("EventBus is being cleared. All current subscribers and queued events will be removed.", this);
            }

            Subscribers.Clear(); 
            AsyncSubscribers.Clear(); 
            QueuedEvents.Clear(); 
        }


        #endregion



        [SerializeField, ShowInInspector, FoldoutGroup("Event Settings")]
        private List<EventUsageInfo> expectedEventTypes = new List<EventUsageInfo>();

        [SerializeField, ShowInInspector, FoldoutGroup("Event Settings")]
        private List<ScriptInfo> consumerScripts = new List<ScriptInfo>();

        [FoldoutGroup("Event Settings"), FolderPath(AbsolutePath = false)]
        public string MainDirectoryName = "Assets/Scripts/OcentraAI/LLMGames/LLMGamesCommon/EventBus/";

        [ShowInInspector, FoldoutGroup("Event Settings")]
        protected List<MonoScript> EventScripts = new List<MonoScript>();

        [SerializeField, ShowInInspector, FoldoutGroup("Event Settings")]
        protected List<MonoScript> UnusedScripts = new List<MonoScript>();

        [SerializeField, ShowInInspector, FoldoutGroup("Event Settings")]
        protected List<MonoScript> NoTypeMatch = new List<MonoScript>();

        [SerializeField, ShowInInspector, FoldoutGroup("Event Settings")]
        protected List<MonoScript> IsEnumIsInterfaceIsValueType;


        [Serializable]
        public class ScriptInfo
        {
            [ShowInInspector, AssetsOnly, ReadOnly, LabelText("Script")]
            public MonoScript MonoScript;

            [HideInInspector] public Type ScriptType;

            [ShowInInspector] public string DisplayName;

            [HideInInspector] public string MonoScriptPath;

            public ScriptInfo(Type scriptType, MonoScript monoScript, string monoScriptPath)
            {
                ScriptType = scriptType;
                MonoScript = monoScript;
                MonoScriptPath = monoScriptPath;
                GetFormattedTypeName();
            }

            public ScriptInfo(Type scriptType, string monoScriptPath)
            {
                ScriptType = scriptType;
                MonoScript = null;
                MonoScriptPath = monoScriptPath;
                GetFormattedTypeName();
            }

            private void GetFormattedTypeName()
            {
                if (ScriptType is { IsGenericType: true })
                {
                    string typeName = ScriptType.Name;
                    int backtickIndex = typeName.IndexOf('`');
                    if (backtickIndex > 0)
                    {
                        typeName = typeName.Substring(0, backtickIndex);
                    }

                    DisplayName = typeName;
                }
                else
                {
                    DisplayName = ScriptType.Name;
                }
            }

        }



        [Serializable]
        public class ScriptCollection
        {
            public List<ScriptInfo> ConsumerScripts;
            public List<EventUsageInfo> EventUsageInfos;
            public ScriptCollection(List<ScriptInfo> consumerScripts, List<EventUsageInfo> eventUsageInfos)
            {
                ConsumerScripts = consumerScripts;
                EventUsageInfos = eventUsageInfos;
            }
        }


        [Serializable]
        public class EventUsageInfo
        {
            public ScriptInfo ScriptInfo;

            [HideInInspector] public List<ScriptInfo> SubscribeMethods = new List<ScriptInfo>();
            [HideInInspector] public List<ScriptInfo> UnsubscribeMethods = new List<ScriptInfo>();
            [HideInInspector] public List<ScriptInfo> PublishMethods = new List<ScriptInfo>();

            [ShowInInspector, ReadOnly] bool IsPublished => PublishMethods.Count > 0;
            [ShowInInspector, ReadOnly]
            bool IsSubscribeValid => SubscribeMethods.Count > 0 &&
                                                                 SubscribeMethods.Count == UnsubscribeMethods.Count &&
                                                                 UnsubscribeMethods.All(scriptInfo => SubscribeMethods.Any(sub => sub.MonoScript == scriptInfo.MonoScript));

            [SerializeField, ReadOnly] public string LastAnalyzed { get; set; }
            [SerializeField, ReadOnly] public int TotalUsageCount => SubscribeMethods.Count + UnsubscribeMethods.Count + PublishMethods.Count;
            public EventUsageInfo(ScriptInfo scriptInfo)
            {
                ScriptInfo = scriptInfo;
                LastAnalyzed = $"{DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()}";
            }


            [ShowInInspector, ReadOnly]
            public List<(string DisplayName, MonoScript MonoScript, string ScriptPath)> SubscribeMethodNames =>
                SubscribeMethods.Select(info => (info.DisplayName, info.MonoScript, info.MonoScriptPath)).ToList();

            [ShowInInspector, ReadOnly]
            public List<(string DisplayName, MonoScript MonoScript, string ScriptPath)> UnsubscribeMethodNames =>
                UnsubscribeMethods.Select(info => (info.DisplayName, info.MonoScript, info.MonoScriptPath)).ToList();

            [ShowInInspector, ReadOnly]
            public List<(string DisplayName, MonoScript MonoScript, string ScriptPath)> PublishMethodNames =>
                PublishMethods.Select(info => (info.DisplayName, info.MonoScript, info.MonoScriptPath)).ToList();


        }

#if UNITY_EDITOR

        [Button]
        public async UniTaskVoid CollectEventTypesAsync()
        {
            EditorUtility.SetDirty(this);

            Stopwatch overallStopwatch = Stopwatch.StartNew();

            //  GameLoggerScriptable.Log("Starting Event Type Collection...", this);

            Stopwatch scriptCollectionStopwatch = Stopwatch.StartNew();
            ScriptCollection scriptCollection = await FindAllMonoScripts(MainDirectoryName);
            scriptCollectionStopwatch.Stop();
            //  GameLoggerScriptable.Log($"FindAllMonoScripts completed in {scriptCollectionStopwatch.ElapsedMilliseconds} ms", this);


            if (scriptCollection != null)
            {
                consumerScripts = scriptCollection.ConsumerScripts;

                Stopwatch mapConsumerMethodsStopwatch = Stopwatch.StartNew();
                await MapConsumerMethodsToEventsWithRoslynAsync(scriptCollection);
                mapConsumerMethodsStopwatch.Stop();
                GameLoggerScriptable.Log($"Collect EventTypes completed in {mapConsumerMethodsStopwatch.ElapsedMilliseconds} ms", this);


                EditorApplication.delayCall += AssetDatabase.SaveAssets;

            }
            else
            {
                overallStopwatch.Stop();
                GameLoggerScriptable.LogError($"Collect EventTypes completed  with errors in {overallStopwatch.ElapsedMilliseconds} ms", this);
            }

        }
        private async UniTask<ScriptCollection> FindAllMonoScripts(string directoryFilter)
        {
            await UniTask.SwitchToMainThread();

            List<ScriptInfo> consumerScriptsList = new List<ScriptInfo>();
            List<EventUsageInfo> eventScriptsList = new List<EventUsageInfo>();


            MonoScript[] allMonoScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            Type[] assemblyTypes = GetAssemblyTypesInDirectory(directoryFilter);

            List<(MonoScript script, string assetPath, Type scriptType, string scriptName)> scriptDataList = new List<(MonoScript, string, Type, string)>();


            foreach (MonoScript script in allMonoScripts)
            {
                if (script == null)
                {
                    continue;
                }

                string assetPath = AssetDatabase.GetAssetPath(script);
                if (!string.IsNullOrEmpty(directoryFilter) && !assetPath.StartsWith(directoryFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Type scriptType = script.GetClass();

                scriptDataList.Add((script, assetPath, scriptType, scriptType?.Name ?? script.name));

            }

            List<UniTask> tasks = new List<UniTask>();
            lock (NoTypeMatch)
            {
                NoTypeMatch = new List<MonoScript>();
            }
            lock (IsEnumIsInterfaceIsValueType)
            {
                IsEnumIsInterfaceIsValueType = new List<MonoScript>();
            }

            foreach ((MonoScript script, string assetPath, Type scriptType, string scriptName) scriptData in scriptDataList)
            {
                tasks.Add(UniTask.RunOnThreadPool(() =>
                {
                    (MonoScript script, string assetPath, Type scriptType, string scriptName) = scriptData;

                    if (scriptType == null)
                    {
                        scriptType = FindTypeByName(scriptName, assemblyTypes);
                        if (scriptType == null)
                        {
                            lock (NoTypeMatch)
                            {
                                if (!NoTypeMatch.Contains(script))
                                {
                                    NoTypeMatch.Add(script);
                                }
                            }
                            return;
                        }
                    }

                    if (scriptType.IsEnum || scriptType.IsInterface || scriptType.IsValueType)
                    {
                        lock (IsEnumIsInterfaceIsValueType)
                        {
                            if (!IsEnumIsInterfaceIsValueType.Contains(script))
                            {
                                IsEnumIsInterfaceIsValueType.Add(script);
                            }

                        }
                        return;
                    }

                    ScriptInfo scriptInfo = new ScriptInfo(scriptType, script, assetPath);
                    bool isEventArgumentClass = typeof(IEventArgs).IsAssignableFrom(scriptType) && scriptType != typeof(IEventArgs) && !scriptType.IsAbstract;

                    if (isEventArgumentClass)
                    {
                        lock (eventScriptsList)
                        {
                            EventUsageInfo eventUsageInfo = new EventUsageInfo(scriptInfo);

                            if (!eventScriptsList.Contains(eventUsageInfo))
                            {
                                eventScriptsList.Add(eventUsageInfo);
                            }

                        }
                    }
                    else
                    {
                        lock (consumerScriptsList)
                        {
                            if (!consumerScriptsList.Contains(scriptInfo))
                            {
                                consumerScriptsList.Add(scriptInfo);
                            }

                        }
                    }
                }));
            }


            await UniTask.WhenAll(tasks);

            //GameLoggerScriptable.Log($" allMonoScripts: {allMonoScripts.Length}" +
            //                         $" NoTypeMatch: {noTypeMatch.Count}  " +
            //                         $" IsEnumIsInterfaceIsValueType: {isEnumIsInterfaceIsValueType.Count}  " +
            //                         $" consumerScriptsList: {consumerScriptsList.Count}  " +
            //                         $" eventScriptsList: {eventScriptsList.Count} " +
            //                         $" scriptDataList{scriptDataList.Count} ", this);


            return new ScriptCollection(consumerScriptsList, eventScriptsList);
        }
        public static Type[] GetAssemblyTypesInDirectory(string directoryPath)
        {
            List<Type> assemblyTypes = new List<Type>();

            string fullPath = Path.Combine(Application.dataPath, directoryPath.Substring("Assets".Length).TrimStart('/'));

            string[] asmdefFiles = Directory.GetFiles(fullPath, "*.asmdef", SearchOption.AllDirectories);

            foreach (string asmdefPath in asmdefFiles)
            {
                string asmdefName = Path.GetFileNameWithoutExtension(asmdefPath);
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        if (assembly.GetName().Name.Contains(asmdefName))
                        {
                            assemblyTypes.AddRange(assembly.GetTypes());
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        assemblyTypes.AddRange(ex.Types);
                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.Instance.LogError($"Error loading types from assembly {assembly.GetName().Name}: {ex.Message}", Instance);
                    }
                }
            }
            return assemblyTypes.ToArray();
        }
        private Type FindTypeByName(string scriptName, Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
            {
                try
                {
                    string normalizedTypeName = type.Name;
                    string normalizedFullName = type.FullName ?? type.Name;

                    int backtickIndex = normalizedTypeName.IndexOf('`');
                    if (backtickIndex > -1)
                    {
                        normalizedTypeName = normalizedTypeName.Substring(0, backtickIndex);
                    }

                    backtickIndex = normalizedFullName.IndexOf('`');
                    if (backtickIndex > -1)
                    {
                        normalizedFullName = normalizedFullName.Substring(0, backtickIndex);
                    }

                    if (normalizedTypeName.Contains(scriptName, StringComparison.OrdinalIgnoreCase) ||
                        normalizedFullName.Contains(scriptName, StringComparison.OrdinalIgnoreCase))
                    {
                        return type;
                    }

                    if (type.DeclaringType != null)
                    {
                        string nestedFullName = $"{type.DeclaringType.Name}+{type.Name}";
                        backtickIndex = nestedFullName.IndexOf('`');
                        if (backtickIndex > -1)
                        {
                            nestedFullName = nestedFullName.Substring(0, backtickIndex);
                        }
                        if (nestedFullName.Contains(scriptName, StringComparison.OrdinalIgnoreCase))
                        {
                            return type;
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogException($"Failed to process type: {type?.Name ?? "Unknown"}, Error: {ex.Message}", this);
                }
            }
            return null;
        }
        private async UniTask MapConsumerMethodsToEventsWithRoslynAsync(ScriptCollection scriptCollection)
        {
            List<UniTask> scriptTasks = new List<UniTask>();

            ConcurrentDictionary<string, EventUsageInfo> concurrentDictionary = new ConcurrentDictionary<string, EventUsageInfo>();

            foreach (EventUsageInfo eventUsageInfo in scriptCollection.EventUsageInfos)
            {
                concurrentDictionary.TryAdd(eventUsageInfo.ScriptInfo.DisplayName, eventUsageInfo);
            }


            foreach (ScriptInfo scriptInfo in scriptCollection.ConsumerScripts)
            {
                scriptTasks.Add(UniTask.RunOnThreadPool(async () =>
                {
                    try
                    {
                        string sourceContent = await UniTask.RunOnThreadPool(() => File.ReadAllText(scriptInfo.MonoScriptPath));
                        SyntaxTree syntaxTree = await UniTask.RunOnThreadPool(() => CSharpSyntaxTree.ParseText(sourceContent));
                        SyntaxNode root = await UniTask.RunOnThreadPool(() => syntaxTree.GetRoot());

                        foreach (MethodDeclarationSyntax methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                        {
                            foreach (InvocationExpressionSyntax invocation in methodDeclaration.Body?.DescendantNodes().OfType<InvocationExpressionSyntax>() ?? Enumerable.Empty<InvocationExpressionSyntax>())
                            {
                                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                                    memberAccess.Expression.ToString() == $"{nameof(EventBus)}.Instance")
                                {
                                    string methodName = memberAccess.Name.Identifier.Text;

                                    if (memberAccess.Name is GenericNameSyntax genericName)
                                    {
                                        TypeSyntax typeArgument = genericName.TypeArgumentList.Arguments.FirstOrDefault();
                                        if (typeArgument != null)
                                        {
                                            string key = typeArgument.ToString();

                                            if (!concurrentDictionary.TryGetValue(key, out EventUsageInfo matchingEvent))
                                            {
                                                // If failed, retry with base name (without generic parameters)
                                                string baseKey = key.Split('<')[0].Trim();
                                                concurrentDictionary.TryGetValue(baseKey, out matchingEvent);
                                            }

                                            if (matchingEvent != null)
                                            {
                                                lock (matchingEvent)
                                                {
                                                    if ((methodName == nameof(Subscribe) || methodName == nameof(SubscribeAsync)) &&
                                                        !matchingEvent.SubscribeMethods.Contains(scriptInfo))
                                                    {
                                                        matchingEvent.SubscribeMethods.Add(scriptInfo);
                                                    }

                                                    else if ((methodName == nameof(Publish) || methodName == nameof(PublishAsync)) &&
                                                           !matchingEvent.PublishMethods.Contains(scriptInfo))
                                                    {
                                                        matchingEvent.PublishMethods.Add(scriptInfo);
                                                    }

                                                    else if ((methodName == nameof(Unsubscribe) || methodName == nameof(UnsubscribeAsync)) &&
                                                             !matchingEvent.UnsubscribeMethods.Contains(scriptInfo))
                                                    {
                                                        matchingEvent.UnsubscribeMethods.Add(scriptInfo);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                                        {
                                            if (methodName == nameof(Publish) || methodName == nameof(PublishAsync))
                                            {
                                                if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                                                {
                                                    string key = objectCreation.Type.ToString();

                                                    if (concurrentDictionary.TryGetValue(key, out EventUsageInfo matchingEvent))
                                                    {
                                                        lock (matchingEvent)
                                                        {
                                                            if (!matchingEvent.PublishMethods.Contains(scriptInfo))
                                                            {
                                                                matchingEvent.PublishMethods.Add(scriptInfo);
                                                            }
                                                        }

                                                    }

                                                }
                                                else if (argument.Expression is IdentifierNameSyntax identifierName)
                                                {
                                                    string argumentName = identifierName.Identifier.Text;

                                                    foreach (VariableDeclarationSyntax variable in methodDeclaration.Body.DescendantNodes().OfType<VariableDeclarationSyntax>())
                                                    {
                                                        foreach (VariableDeclaratorSyntax variableDeclarator in variable.Variables)
                                                        {
                                                            if (variableDeclarator.Identifier.Text == argumentName && variable.Type is IdentifierNameSyntax type)
                                                            {
                                                                string key = type.Identifier.Text;

                                                                if (concurrentDictionary.TryGetValue(key, out EventUsageInfo matchingEvent))
                                                                {
                                                                    lock (matchingEvent)
                                                                    {
                                                                        if (!matchingEvent.PublishMethods.Contains(scriptInfo))
                                                                        {
                                                                            matchingEvent.PublishMethods.Add(scriptInfo);
                                                                        }
                                                                    }


                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (methodName == nameof(Subscribe) || methodName == nameof(SubscribeAsync))
                                            {
                                                if (argument.Expression is IdentifierNameSyntax identifierName)
                                                {
                                                    string argumentName = identifierName.Identifier.Text;

                                                    foreach (VariableDeclarationSyntax variable in methodDeclaration.Body.DescendantNodes().OfType<VariableDeclarationSyntax>())
                                                    {
                                                        foreach (VariableDeclaratorSyntax variableDeclarator in variable.Variables)
                                                        {
                                                            if (variableDeclarator.Identifier.Text == argumentName && variable.Type is IdentifierNameSyntax type)
                                                            {
                                                                string key = type.Identifier.Text;

                                                                if (concurrentDictionary.TryGetValue(key, out EventUsageInfo matchingEvent))
                                                                {
                                                                    lock (matchingEvent)
                                                                    {
                                                                        if (!matchingEvent.SubscribeMethods.Contains(scriptInfo))
                                                                        {
                                                                            matchingEvent.SubscribeMethods.Add(scriptInfo);
                                                                        }
                                                                    }

                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (methodName == nameof(Unsubscribe) || methodName == nameof(UnsubscribeAsync))
                                            {
                                                if (argument.Expression is IdentifierNameSyntax identifierName)
                                                {
                                                    string argumentName = identifierName.Identifier.Text;

                                                    foreach (VariableDeclarationSyntax variable in methodDeclaration.Body.DescendantNodes().OfType<VariableDeclarationSyntax>())
                                                    {
                                                        foreach (VariableDeclaratorSyntax variableDeclarator in variable.Variables)
                                                        {
                                                            if (variableDeclarator.Identifier.Text == argumentName && variable.Type is IdentifierNameSyntax type)
                                                            {
                                                                string key = type.Identifier.Text;

                                                                if (concurrentDictionary.TryGetValue(key, out EventUsageInfo matchingEvent))
                                                                {
                                                                    lock (matchingEvent)
                                                                    {
                                                                        if (!matchingEvent.UnsubscribeMethods.Contains(scriptInfo))
                                                                        {
                                                                            matchingEvent.UnsubscribeMethods.Add(scriptInfo);
                                                                        }
                                                                    }

                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.LogError($"Error analyzing script {scriptInfo.MonoScriptPath}: {ex.Message}", this);
                    }
                }));
            }

            await UniTask.WhenAll(scriptTasks);

            expectedEventTypes = new List<EventUsageInfo>();
            EventScripts = new List<MonoScript>();
            UnusedScripts = new List<MonoScript>();

            List<KeyValuePair<string, EventUsageInfo>> sortedEntries = concurrentDictionary.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase).ToList();


            foreach ((string _, EventUsageInfo eventUsageInfo) in sortedEntries)
            {
                MonoScript monoScript = eventUsageInfo.ScriptInfo.MonoScript;

                if (eventUsageInfo.TotalUsageCount > 0)
                {

                    if (!expectedEventTypes.Contains(eventUsageInfo))
                    {
                        expectedEventTypes.Add(eventUsageInfo);
                    }

                    if (!EventScripts.Contains(monoScript))
                    {
                        EventScripts.Add(monoScript);
                    }
                }
                else
                {
                    UnusedScripts.Add(monoScript);
                }

            }

            //GameLoggerScriptable.Log($" scriptCollection.EventUsageInfos: {scriptCollection.EventUsageInfos.Count}" +
            //                         $" concurrentDictionary: {concurrentDictionary.Count}  " +
            //                         $" sortedEntries: {sortedEntries.Count} " +
            //                         $" expectedEventTypes{expectedEventTypes.Count} ", this);

            foreach (EventUsageInfo eventUsageInfo in scriptCollection.EventUsageInfos)
            {
                bool found = false;

                foreach ((string _, EventUsageInfo processedEventInfo) in sortedEntries)
                {
                    if (processedEventInfo.ScriptInfo.MonoScript == eventUsageInfo.ScriptInfo.MonoScript)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    UnusedScripts.Add(eventUsageInfo.ScriptInfo.MonoScript);
                }
            }

            await UniTask.Yield();
        }

#endif

    }

}
