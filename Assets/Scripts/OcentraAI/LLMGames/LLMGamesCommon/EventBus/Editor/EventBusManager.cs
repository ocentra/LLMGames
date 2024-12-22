using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.Events.Editor
{
    public class EventBusManager : OdinMenuEditorWindow
    {

        private Color backgroundColor;

        private const string MainDirectoryPrefKey = "EventBusManager.MainDirectoryName";


        private string MainDirectoryName
        {
            get => EditorPrefs.GetString(MainDirectoryPrefKey, "Assets/Scripts/OcentraAI/");
            set => EditorPrefs.SetString(MainDirectoryPrefKey, value);
        }

        private EventBus EventBus => EventBus.Instance;

        public UsageInfo UsageInfo
        {
            get => EventBus.UsageInfo;
            set => EventBus.UsageInfo = value;
        }



        public List<string> AssemblyFiles
        {
            get => EventBus.AssemblyFiles;
            set => EventBus.AssemblyFiles = value;
        }

        public List<ScriptInfo> AllScripts
        {
            get => EventBus.AllScripts;
            set => EventBus.AllScripts = value;
        }

        public List<ScriptInfo> EventMonoScript
        {
            get => EventBus.EventMonoScript;
            set => EventBus.EventMonoScript = value;
        }

        private Vector2 scrollPosition, scrollPositionCode;
        private float dividerHeight = 300f;
        private const float DividerHandleSize = 5f;
        private Object lastSelectedWrapper;

        [MenuItem("Tools/Event Bus Manager")]
        private static void OpenWindow()
        {
            EventBusManager window = GetWindow<EventBusManager>("Event Bus Manager");
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            backgroundColor = EditorGUIUtility.isProSkin
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

            string[] asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { MainDirectoryName });

            foreach (string guid in asmdefGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssemblyFiles.Contains(path))
                {
                    AssemblyFiles.Add(path);
                }

            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    if (assembly.GetName().Name.Contains("LLMGames.Events"))
                    {
                        Type[] eventAssemblyTypes = assembly.GetTypes();

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

                    foreach (string path in AssemblyFiles)
                    {
                        string nameWithoutExtension = Path.GetFileNameWithoutExtension(path);

                        if (assembly.GetName().Name.Equals(nameWithoutExtension, StringComparison.OrdinalIgnoreCase))
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
            GUI.color = backgroundColor;
            EditorGUI.DrawRect(position, backgroundColor);
            GUI.color = originalColor;

            base.DrawEditors();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree menuTree = new OdinMenuTree(true)
            {
                Config = { DrawSearchToolbar = true },
                DefaultMenuStyle = new OdinMenuStyle
                {
                    Height = 23,
                    IconSize = 20,
                    IconOffset = 0,
                    IndentAmount = 15,
                    BorderPadding = 0,
                    AlignTriangleLeft = true,
                    TriangleSize = 16,
                    TrianglePadding = 0
                }
            };

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
                        string folderPath = Path.GetDirectoryName(scriptPath)?.Replace("\\", "/");
                        if (folderPath != null)
                        {
                            string relativeFolderPath = folderPath.Replace("Assets/Scripts/OcentraAI/LLMGames/Events", "Events");

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

                                    OdinMenuItem folderItem = new OdinMenuItem(menuTree, $"{folderParts[i]} [0]", null)
                                    {
                                        Icon = EditorGUIUtility.IconContent("Folder Icon").image
                                    };

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
                                    Icon = EditorGUIUtility.IconContent("cs Script Icon").image
                                };
                                targetFolder.ChildMenuItems.Add(scriptItem);

                                targetFolder.Name = $"{Path.GetFileName(relativeFolderPath)} [{targetFolder.ChildMenuItems.Count}]";
                            }
                        }
                    }
                }
            }


            return menuTree;
        }

        private void DisplayAssemblyFiles(OdinMenuTree menuTree)
        {
            if (AssemblyFiles is { Count: > 0 })
            {
                OdinMenuItem assembliesFolder = new OdinMenuItem(menuTree, $"Assemblies [{AssemblyFiles.Count}]", null)
                {
                    Icon = EditorGUIUtility.IconContent("Folder Icon").image
                };
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
                OdinMenuItem allScript = new OdinMenuItem(menuTree, $"AllScript [{AllScripts.Count}]", null)
                {
                    Icon = EditorGUIUtility.IconContent("Folder Icon").image
                };
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

                if (lastSelectedWrapper != script)
                {
                    lastSelectedWrapper = script;
                    EditorGUIUtility.PingObject(script);
                }

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(dividerHeight));
                {
                    DrawSectionHeader("Event Usage");

                    EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                    {
                        // Publishers Column
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

                        // Subscribers Column
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
                if (selected?.Value is Object pingableObject)
                {

                    if (lastSelectedWrapper != pingableObject)
                    {
                        lastSelectedWrapper = pingableObject;
                        EditorGUIUtility.PingObject(pingableObject);
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

                    if (GUILayout.Button("...", GUILayout.Width(30)))
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
                progressText = "No scripts found to process.";
                currentProgress = 1f;
                Repaint();
                return;
            }

            int totalScripts = EventMonoScript.Count;
            int currentScriptIndex = 0;

            List<UniTask> eventTasks = new List<UniTask>();

            foreach (ScriptInfo info in EventMonoScript)
            {
                eventTasks.Add(UniTask.RunOnThreadPool(async () =>
                {
                    MonoScript eventScript = info.MonoScript;
                    Type eventType = info.ScriptType;

                    if (eventType == null) return;

                    string eventTypeName = info.DisplayName;

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

                                    bool isPublisher = sourceCode.Contains($"new {eventTypeName}");
                                    bool isSubscriber = sourceCode.Contains($"<{eventTypeName}>");

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
                                catch (Exception)
                                {
                                }
                            }));
                        }

                        await UniTask.WhenAll(batchTasks);
                    }

                    UsageInfo.Subscribers.TryAdd(eventScript, subscribers);
                    UsageInfo.Publishers.TryAdd(eventScript, publishers);
                }));

                currentScriptIndex++;
                currentProgress = (float)currentScriptIndex / totalScripts;
                progressText = $"Processing {currentScriptIndex}/{totalScripts} event scripts...";

                await UniTask.Yield();
                Repaint();
            }

            await UniTask.WhenAll(eventTasks);

            progressText = "Completed!";
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

        private float currentProgress = 0f;
        private string progressText = string.Empty;
        private bool isRunning = false;

        private void DrawSectionHeaderWithRefresh(string headerTitle)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0, 0, 0.8f, 1f);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(headerTitle, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

            GUILayout.BeginVertical(GUILayout.Width(300));
            {
                if (isRunning)
                {
                    GUILayout.Label(progressText, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                    Rect progressRect = GUILayoutUtility.GetRect(100, 20, GUILayout.ExpandWidth(true));
                    EditorGUI.ProgressBar(progressRect, currentProgress, progressText);
                }
                else
                {
                    GUILayout.Label("No ongoing process", EditorStyles.centeredGreyMiniLabel);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Update Usages", GUILayout.ExpandWidth(false));

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), GUILayout.Width(40)))
                {
                    Initialize();
                }

            }
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
        }

        private async UniTaskVoid HandleProgress(string taskName, Func<UniTask> task)
        {
            isRunning = true;
            currentProgress = 0f;
            progressText = $"{taskName}...";

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
                progressText = "Completed!";
                currentProgress = 1f;
                await UniTask.Delay(1000);
                progressText = string.Empty;
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