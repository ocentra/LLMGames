#if UNITY_EDITOR

using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Sirenix.Utilities.Editor;

namespace OcentraAI.LLMGames.Events.Editor
{
    public class EventBusManager : OdinMenuEditorWindow
    {
        private Color BackgroundColor { get; set; }
        private string ProgressText { get; set; } = string.Empty;
        private const string MainDirectoryPrefKey = "EventBusManager.MainDirectoryName";
        private string MainDirectoryName
        {
            get => EditorPrefs.GetString(MainDirectoryPrefKey, "Assets/Scripts/OcentraAI/");
            set => EditorPrefs.SetString(MainDirectoryPrefKey, value);
        }
        private EventBus EventBus => EventBus.Instance;
        public UsageInfo UsageInfo { get => EventBus.UsageInfo; set => EventBus.UsageInfo = value; }
        public List<string> AssemblyFiles { get => EventBus.AssemblyFiles; set => EventBus.AssemblyFiles = value; }
        public List<ScriptInfo> AllScripts { get => EventBus.AllScripts; set => EventBus.AllScripts = value; }
        public List<ScriptInfo> EventMonoScript { get => EventBus.EventMonoScript; set => EventBus.EventMonoScript = value; }

        public string EventAssemblyPath { get; set; }

        private Vector2 scrollPosition, scrollPositionCode;
        private float dividerHeight = 300f;
        private const float DividerHandleSize = 5f;
        private float currentProgress = 0f;
        private bool isRunning = false;


        private Object LastSelectedWrapper { get; set; }

        [MenuItem("Tools/Event Bus Manager")]
        private static void OpenWindow()
        {
            EventBusManager window = GetWindow<EventBusManager>("Event Bus Manager");
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            BackgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SaveEventBusData();
        }

        private void SaveEventBusData()
        {
            if (EventBus == null) { return; }

            EditorUtility.SetDirty(EventBus);
            EventBus.SaveChanges();
            AssetDatabase.SaveAssets();
        }

        protected override void Initialize()
        {
            base.Initialize();
            CollectScripts();
            _ = HandleProgress("Finding Usages", FindUsagesAsync);
        }

        private void CollectScripts()
        {
            if (EventBus == null) return;

            AssemblyFiles = new List<string>();
            AllScripts = new List<ScriptInfo>();
            EventMonoScript = new List<ScriptInfo>();
            UsageInfo = new UsageInfo();

            Assembly eventAssembly = typeof(EventArgsBase).Assembly;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            HashSet<Assembly> processAssemblies = new HashSet<Assembly>();
            string[] assemblyGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { MainDirectoryName });

            foreach (string guid in assemblyGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                Assembly assembly = assemblies.FirstOrDefault(a => a.GetName().Name.Equals(nameWithoutExtension, StringComparison.OrdinalIgnoreCase));

                if (assembly != null)
                {
                    if (assembly == eventAssembly)
                    {
                        EventAssemblyPath = path;
                        continue;
                    }

                    if (!AssemblyFiles.Contains(path))
                    {
                        AssemblyFiles.Add(path);
                    }
                    processAssemblies.Add(assembly);
                }

            }

            try
            {
                Type[] eventAssemblyTypes = eventAssembly.GetTypes();

                foreach (Type type in eventAssemblyTypes)
                {
                    if (type.IsSubclassOf(typeof(EventArgsBase)) && !type.IsAbstract && type.IsClass)
                    {
                        string[] assets = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
                        foreach (string asset in assets)
                        {
                            string scriptPath = AssetDatabase.GUIDToAssetPath(asset);
                            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

                            if (script != null && script.GetClass() == type)
                            {
                                ScriptInfo scriptInfo = new ScriptInfo(type, script, scriptPath);
                                if (!EventMonoScript.Contains(scriptInfo))
                                {
                                    EventMonoScript.Add(scriptInfo);
                                }

                                break;
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogError($"Error loading types from assembly {eventAssembly.GetName().Name}: {ex.Message}");
                if (ex.LoaderExceptions != null)
                {
                    foreach (Exception loaderException in ex.LoaderExceptions)
                    {
                        Debug.LogError($"Loader Exception: {loaderException?.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error processing assembly {eventAssembly.GetName().Name}: {ex.Message}");
            }



            foreach (Assembly assembly in processAssemblies)
            {
                try
                {
                    Type[] collection = assembly.GetTypes();

                    foreach (Type type in collection)
                    {
                        if (type.IsAbstract || !type.IsClass || type.Namespace == null)
                            continue;

                        string[] assets = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
                        foreach (string asset in assets)
                        {
                            string scriptPath = AssetDatabase.GUIDToAssetPath(asset);
                            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

                            if (script != null && script.GetClass() == type)
                            {
                                ScriptInfo scriptInfo = new ScriptInfo(type, script, scriptPath);
                                if (!AllScripts.Contains(scriptInfo))
                                {
                                    AllScripts.Add(scriptInfo);
                                }

                                break;
                            }
                        }
                    }

                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogError($"Error loading types from assembly {assembly.GetName().Name}: {ex.Message}");
                    if (ex.LoaderExceptions != null)
                    {
                        foreach (Exception loaderException in ex.LoaderExceptions)
                        {
                            Debug.LogError($"Loader Exception: {loaderException?.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error processing assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }

        }

        protected override void DrawEditors()
        {
            Color originalColor = GUI.color;
            GUI.color = BackgroundColor;
            EditorGUI.DrawRect(position, BackgroundColor);
            GUI.color = originalColor;

            base.DrawEditors();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree menuTree = new OdinMenuTree(true)
            {
                Config =
                {
                    DrawSearchToolbar = true,

                },
                DefaultMenuStyle = new OdinMenuStyle
                {
                    Height = 23,
                    IconSize = 20,
                    IconOffset = 0,
                    IndentAmount = 15,
                    BorderPadding = 0,
                    AlignTriangleLeft = true,
                    TriangleSize = 16,
                    TrianglePadding = 0,

                }
            };
            
            // Intentionally disabled to draw so if we need to debug we can uncomment it to see

            // DisplayAllScript(menuTree);

            // DisplayAssemblyFiles(menuTree);

            if (EventMonoScript is { Count: > 0 })
            {
                Dictionary<string, OdinMenuItem> folderItems = new Dictionary<string, OdinMenuItem>();

                foreach (ScriptInfo eventScript in EventMonoScript)
                {
                    string scriptPath = eventScript.MonoScriptPath;

                    if (!string.IsNullOrEmpty(scriptPath))
                    {
                        string directoryName = Path.GetDirectoryName(scriptPath);
                        string folderPath = directoryName?.Replace("\\", "/");
                        if (folderPath != null)
                        {
                            string eventBaseFolder = Path.GetDirectoryName(EventAssemblyPath);
                            eventBaseFolder = eventBaseFolder?.Replace("\\", "/");

                            if (eventBaseFolder != null)
                            {
                                string eventBaseFolderName = Path.GetFileName(eventBaseFolder);
                                string relativeFolderPath = folderPath.Replace(eventBaseFolder, eventBaseFolderName);
                                string[] folderParts = relativeFolderPath.Split('/');
                                string currentPath = "";

                                for (int i = 0; i < folderParts.Length; i++)
                                {
                                    currentPath = string.Join("/", folderParts.Take(i + 1));

                                    if (!folderItems.ContainsKey(currentPath))
                                    {
                                        string parentPath = string.Join("/", folderParts.Take(i));
                                        OdinMenuItem parentFolder = null;

                                        if (!string.IsNullOrEmpty(parentPath) && folderItems.TryGetValue(parentPath, out OdinMenuItem parentItem))
                                        {
                                            parentFolder = parentItem;
                                        }
                                        Object folderObject = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
                                        string displayName = $"{folderParts[i]} [Total: {EventMonoScript.Count}]";

                                        OdinMenuItem folderItem = new OdinMenuItem(menuTree, displayName, folderObject);
                                        DrawFolder(folderItem);

                                        if (parentFolder != null)
                                        {
                                            parentFolder.ChildMenuItems.Add(folderItem);
                                        }
                                        else
                                        {
                                            menuTree.MenuItems.Add(folderItem);
                                        }

                                        folderItems[currentPath] = folderItem;
                                        
                                    }
                                }

                                if (folderItems.TryGetValue(relativeFolderPath, out OdinMenuItem targetFolder))
                                {

                                    OdinMenuItem scriptItem = new OdinMenuItem(menuTree, eventScript.DisplayName, eventScript)
                                    {

                                        OnDrawItem = (item) =>
                                        {
                                            Rect rect = item.Rect;

                                            float rectX = rect.x;


                                            float iconSize = 20;
                                            Rect iconRect = new Rect(rect.x + rect.width - iconSize - 10, rect.y, iconSize, iconSize);

                                            Texture2D defaultIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
                                            if (defaultIcon != null)
                                            {
                                                GUI.DrawTexture(iconRect, defaultIcon);
                                            }


                                            string displayName = eventScript.DisplayName;
                                            switch (eventScript.State)
                                            {
                                                case ScriptInfo.EventState.Pass:
                                                    displayName = eventScript.DisplayName;
                                                    break;
                                                case ScriptInfo.EventState.Fail:
                                                    displayName = $"{eventScript.DisplayName} [{ScriptInfo.EventState.NoSubscriber} | {ScriptInfo.EventState.NoPublisher}] ";
                                                    break;
                                                case ScriptInfo.EventState.NoSubscriber:
                                                    displayName = $"{eventScript.DisplayName} [{ScriptInfo.EventState.NoSubscriber}] "; ;
                                                    break;
                                                case ScriptInfo.EventState.NoPublisher:
                                                    displayName = $"{eventScript.DisplayName} [{ScriptInfo.EventState.NoPublisher}] "; ;
                                                    break;


                                            }

                                            item.Name = $"{displayName}";
                                            (SdfIconType Icon, Color IconColor) sdfIconTypeIcon = eventScript.GetSdfIconTypeIcon();
                                            item.SdfIcon = sdfIconTypeIcon.Icon;
                                            item.SdfIconColor = sdfIconTypeIcon.IconColor;


                                        }
                                    };

                                    targetFolder.ChildMenuItems.Add(scriptItem);
                                    targetFolder.Name = $"{Path.GetFileName(relativeFolderPath)} [{targetFolder.ChildMenuItems.Count}]";


                                }
                            }
                        }
                    }
                }
            }



            return menuTree;
        }

        private static void DrawFolder(OdinMenuItem folderItem)
        {
            folderItem.OnDrawItem = (item) =>
            {
                if (item.Toggled)
                {
                    item.SdfIcon = SdfIconType.Folder2Open;
                    item.SdfIconColor = Color.green;
                }
                else
                {
                    item.SdfIcon = SdfIconType.Folder;
                    item.SdfIconColor = Color.black;
                }
            };
        }


        private void DisplayAssemblyFiles(OdinMenuTree menuTree)
        {
            if (AssemblyFiles is { Count: > 0 })
            {
                OdinMenuItem assembliesFolder = new OdinMenuItem(menuTree, $"Assemblies [{AssemblyFiles.Count}]", null);
                DrawFolder(assembliesFolder);
                menuTree.MenuItems.Add(assembliesFolder);

                foreach (string assemblyFile in AssemblyFiles)
                {
                    AssemblyDefinitionAsset asmdef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyFile);
                    if (asmdef != null)
                    {
                        OdinMenuItem assemblyItem = new OdinMenuItem(menuTree, asmdef.name, asmdef)
                        {
                            Icon = EditorGUIUtility.IconContent("ScriptableObject Icon").image
                        };
                        assembliesFolder.ChildMenuItems.Add(assemblyItem);
                    }
                }
            }

        }

        private void DisplayAllScript(OdinMenuTree menuTree)
        {
            if (AllScripts is { Count: > 0 })
            {
                OdinMenuItem allScript = new OdinMenuItem(menuTree, $"AllScript [{AllScripts.Count}]", null);
                DrawFolder(allScript);
                menuTree.MenuItems.Add(allScript);

                foreach (ScriptInfo scriptInfo in AllScripts)
                {

                    OdinMenuItem scriptItem = new OdinMenuItem(menuTree, scriptInfo.DisplayName, scriptInfo)
                    {
                        Icon = EditorGUIUtility.IconContent("cs Script Icon").image
                    };
                    allScript.ChildMenuItems.Add(scriptItem);
                }
            }

        }

        protected override void OnBeginDrawEditors()
        {
            MainDirectory();

            OdinMenuItem selected = MenuTree.Selection.FirstOrDefault();

            if (selected?.Value is ScriptInfo { MonoScriptPath: not null } scriptInfo)
            {
                MonoScript script = scriptInfo.MonoScript;


                if (LastSelectedWrapper != script)
                {
                    LastSelectedWrapper = script;
                    EditorGUIUtility.PingObject(script);
                }
                

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(dividerHeight));
                {
                    DrawSectionHeader("Event Usage");

                    EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                        {
                            DrawColumnHeader("Publishers");

                            if (UsageInfo.Publishers != null && UsageInfo.Publishers.TryGetValue(script, out List<MonoScript> monoScripts))
                            {
                                foreach (MonoScript monoScript in monoScripts)
                                {
                                    DrawEntry(monoScript);
                                }

                            }

                        }
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(1);
                        EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(1), GUILayout.ExpandHeight(true));
                        GUILayout.Space(5);

                        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                        {
                            DrawColumnHeader("Subscribers");
                            if (UsageInfo.Subscribers != null && UsageInfo.Subscribers.TryGetValue(script, out List<MonoScript> monoScripts))
                            {
                                foreach (MonoScript monoScript in monoScripts)
                                {
                                    DrawEntry(monoScript);
                                }

                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                GUILayout.Box("", GUILayout.Height(DividerHandleSize), GUILayout.ExpandWidth(true));
                Rect dividerRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(dividerRect, MouseCursor.ResizeVertical);

                if (Event.current.type == EventType.MouseDown && dividerRect.Contains(Event.current.mousePosition))
                {
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                }

                if (Event.current.type == EventType.MouseDrag && GUIUtility.hotControl != 0)
                {
                    dividerHeight += Event.current.delta.y;
                    dividerHeight = Mathf.Clamp(dividerHeight, 100f, position.height - 150f);
                    Repaint();
                }

                if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl != 0)
                {
                    GUIUtility.hotControl = 0;
                }

                GUILayout.Space(20);
                DrawSectionHeader("Event Code");
                if (script.GetClass() != null)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Path:", AssetDatabase.GetAssetPath(script));
                }

                scrollPositionCode = EditorGUILayout.BeginScrollView(scrollPositionCode, GUILayout.ExpandHeight(true));
                {
                    string codeContent = scriptInfo.CodeContent;
                    GUIStyle codeStyle = new GUIStyle(EditorStyles.textArea)
                    {
                        wordWrap = false,
                        richText = true,
                        fontSize = 12
                    };
                    EditorGUILayout.TextArea(codeContent, codeStyle, GUILayout.ExpandHeight(true));
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                if (selected is {Value: Object pingableObject})
                {
                    if (LastSelectedWrapper != pingableObject)
                    {
                        LastSelectedWrapper = pingableObject;

                        // Check if it's a folder and ping it
                        string path = AssetDatabase.GetAssetPath(pingableObject);
                        if (AssetDatabase.IsValidFolder(path))
                        {
                            Object folder = AssetDatabase.LoadAssetAtPath<Object>(path);
                            EditorGUIUtility.PingObject(folder);
                        }
                        else
                        {
                            EditorGUIUtility.PingObject(pingableObject);
                        }
                    }

                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Selected Asset Details:", EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Asset", pingableObject, pingableObject.GetType(), false);

                    if (pingableObject is AssemblyDefinitionAsset assemblyAsset)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("Assembly Name:", assemblyAsset.name);
                        EditorGUILayout.LabelField("Path:", AssetDatabase.GetAssetPath(assemblyAsset));
                    }
                    else if (pingableObject is MonoScript script)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("Script Name:", script.name);
                        EditorGUILayout.LabelField("Path:", AssetDatabase.GetAssetPath(script));
                        EditorGUILayout.LabelField("Namespace:", script.GetClass()?.Namespace ?? "None");
                    }
                    else
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField("Asset Name:", pingableObject.name);
                        EditorGUILayout.LabelField("Path:", AssetDatabase.GetAssetPath(pingableObject));

                        // If the asset is a folder, add an extra label
                        string path = AssetDatabase.GetAssetPath(pingableObject);
                        if (AssetDatabase.IsValidFolder(path))
                        {
                            EditorGUILayout.LabelField("Type:", "Folder");
                        }
                    }
                }
                else
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Please select an event or asset to view details.", MessageType.Info);
                }

            }

        }

        private void MainDirectory()
        {

            GUILayout.Space(10);
            DrawSectionHeaderWithRefresh("Event Settings");

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Main Directory:", GUILayout.Width(100));
                    EditorGUILayout.TextField(MainDirectoryName, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button("", GUILayout.Width(30)))
                    {
                        string selectedPath = EditorUtility.OpenFolderPanel("Select Main Directory", MainDirectoryName, "");
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            if (selectedPath.StartsWith(Application.dataPath))
                            {
                                selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                            }

                            MainDirectoryName = selectedPath;
                            EditorPrefs.SetString(MainDirectoryPrefKey, MainDirectoryName);

                            Initialize();
                            Repaint();
                        }
                    }
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    EditorIcons.Folder.Draw(lastRect);
                }



                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);
        }

        private async UniTask FindUsagesAsync()
        {
            if (EventMonoScript.Count == 0 || AllScripts.Count == 0)
            {
                ProgressText = "No scripts found to process.";
                currentProgress = 1f;
                Repaint();
                return;
            }

            int totalScripts = EventMonoScript.Count;
            int currentScriptIndex = 0;

            List<UniTask> eventTasks = new List<UniTask>();

            foreach (ScriptInfo eventScriptInfo in EventMonoScript)
            {
                eventTasks.Add(UniTask.RunOnThreadPool(async () =>
                {
                    try
                    {
                        MonoScript eventScript = eventScriptInfo.MonoScript;
                        Type eventType = eventScriptInfo.ScriptType;

                        if (eventType == null) return;

                        string eventTypeName = eventScriptInfo.DisplayName;

                        List<List<ScriptInfo>> scriptBatches = new List<List<ScriptInfo>>();
                        List<ScriptInfo> currentBatch = new List<ScriptInfo>();
                        List<MonoScript> subscribers = new List<MonoScript>();
                        List<MonoScript> publishers = new List<MonoScript>();

                        foreach (ScriptInfo scriptInfo in AllScripts)
                        {
                            if (scriptInfo == null) continue;

                            currentBatch.Add(scriptInfo);
                            if (currentBatch.Count >= 20)
                            {
                                scriptBatches.Add(currentBatch);
                                currentBatch = new List<ScriptInfo>();
                            }
                        }

                        if (currentBatch.Count > 0)
                        {
                            scriptBatches.Add(currentBatch);
                        }
                        
                        foreach (List<ScriptInfo> batch in scriptBatches)
                        {
                            List<UniTask> batchTasks = new List<UniTask>();

                            foreach (ScriptInfo assemblyScript in batch)
                            {
                                batchTasks.Add(UniTask.RunOnThreadPool(() =>
                                {
                                    try
                                    {
                                        string sourceCode = assemblyScript.CodeContent;
                                        if (string.IsNullOrEmpty(sourceCode)) return;

                                        bool isPublisher = Regex.IsMatch(sourceCode, $@"new\s+{eventTypeName}(<[^>]+?>)?", RegexOptions.Multiline);
                                        bool isSubscriber = Regex.IsMatch(sourceCode, $@"Subscribe<{eventTypeName}(<[^>]+?>)?>", RegexOptions.Multiline);


                                        if (isPublisher)
                                        {
                                            lock (publishers)
                                            {
                                                publishers.Add(assemblyScript.MonoScript);
                                            }
                                        }

                                        if (isSubscriber)
                                        {
                                            lock (subscribers)
                                            {
                                                subscribers.Add(assemblyScript.MonoScript);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Error processing script: {assemblyScript.MonoScript.name}, {ex.Message}");
                                    }
                                }));
                            }

                            await UniTask.WhenAll(batchTasks);
                            
                            Repaint();
                        }

                        UsageInfo.Subscribers.TryAdd(eventScript, subscribers);
                        UsageInfo.Publishers.TryAdd(eventScript, publishers);
                        eventScriptInfo.SetEventState(UsageInfo.Subscribers, UsageInfo.Publishers);

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing event script: {eventScriptInfo.MonoScript.name}, {ex.Message}");
                    }
                }));

                currentScriptIndex++;
                currentProgress = (float)currentScriptIndex / totalScripts;
                ProgressText = $"Processing {currentScriptIndex}/{totalScripts} event scripts...";

                await UniTask.Yield();
                Repaint();
            }

            await UniTask.WhenAll(eventTasks);

            ProgressText = "Completed!";
            currentProgress = 1f;
        }
        private void DrawSectionHeader(string headerTitle)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0, 0, 0.8f, 1f);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(headerTitle, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
        }
        private void DrawSectionHeaderWithRefresh(string headerTitle)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.blue;
            float iconSize = 20;

            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));

            // Left Gear Icon
            GUILayout.BeginHorizontal(GUILayout.Width(200));
            Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.ExpandWidth(false));
            EditorIcons.SettingsCog.Draw(iconRect);
            GUILayout.Label(headerTitle, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            // Event Title and Progress Area

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
               
                if (isRunning)
                {
                    GUILayout.Label(ProgressText, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                    Rect progressRect = GUILayoutUtility.GetRect(100, 20, GUILayout.ExpandWidth(true));
                    GUI.backgroundColor = Color.green;
                    EditorGUI.ProgressBar(progressRect, currentProgress, ProgressText);
                    GUI.backgroundColor = originalColor;
                }
                else
                {
                    GUI.backgroundColor = Color.black;
                    GUILayout.Label("No ongoing process", EditorStyles.centeredGreyMiniLabel);
                    GUI.backgroundColor = originalColor;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            
            // Right "Update Usages" Section
            GUILayout.BeginHorizontal(GUILayout.Width(100));
            {
                GUI.backgroundColor = Color.cyan;
                GUILayout.Label("Update Usages", GUILayout.ExpandWidth(false));
                GUI.backgroundColor = originalColor;

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("", GUILayout.Width(20)))
                {
                    Initialize();
                }

                var lastRect = GUILayoutUtility.GetLastRect();
                EditorIcons.Refresh.Draw(lastRect);
                GUI.backgroundColor = originalColor;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
        }


        private async UniTaskVoid HandleProgress(string taskName, Func<UniTask> task)
        {
            isRunning = true;
            currentProgress = 0f;
            ProgressText = $"{taskName}...";

            try
            {
                await task.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during {taskName}: {ex.Message}");
            }
            finally
            {
                ProgressText = "Completed!";
                currentProgress = 1f;
                await UniTask.Delay(1000);
                ProgressText = string.Empty;
                currentProgress = 0f;
                isRunning = false;
            }
        }
        
        private void DrawColumnHeader(string headerTitle)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.6f, 1f, 1f);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(headerTitle, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
        }

        private void DrawEntry(MonoScript script)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0, 0.6f, 1f, 1f);
            GUILayout.BeginHorizontal("box");

            EditorGUILayout.ObjectField("", script, typeof(MonoScript), false);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
        }
        
    }
}

#endif