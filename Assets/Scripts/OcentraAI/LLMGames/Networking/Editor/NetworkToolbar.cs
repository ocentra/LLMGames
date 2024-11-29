#if UNITY_EDITOR
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using ParrelSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


[InitializeOnLoad]
public static class NetworkToolbar
{
    public static int MaxNumberOfPlayers = 10;

    private static string CurrentSceneName { get; set; }
    private static bool playerDropdownOpen = false;
    private static bool aiDropdownOpen = false;
    private static bool sceneDropdownOpen = false;
    private static bool isNewSceneLoaded  = false;
    private static int lastHumanPlayers = 1;
    private static int lastAIPlayers = 0;
    private static int maxAI = 9;
    private static int maxHumans = 9;
    private static bool syncEnabled = true;
    private static EditorData EditorData => AutoNetworkBootstrap.Instance != null ? AutoNetworkBootstrap.Instance.EditorData : new EditorData();
    static NetworkToolbar()
    {
        if (Application.isPlaying) { return; }

        EditorApplication.update -= UpdateToolbar;
        EditorApplication.update += UpdateToolbar;


        if (AutoNetworkBootstrap.Instance != null)
        {


            if (AutoNetworkBootstrap.Instance != null)
            {
                EditorApplication.playModeStateChanged += AutoNetworkBootstrap.Instance.OnPlayModeStateChanged;
            }

            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                EditorApplication.playModeStateChanged -= AutoNetworkBootstrap.Instance.OnPlayModeStateChanged;
            };

        }



        ToolBarExt.ToolbarGUI.Add(OnToolbarGUI);

    }
    
    private static void UpdateToolbar()
    {
        if (Application.isPlaying) { return; }

        bool changed = false;

        int currentHumanPlayers = EditorData.HumanPlayersCount;
        int currentAIPlayers = EditorData.AIPlayersCount;
        bool wasSynced = EditorData.SyncEnabled;
        bool newSceneLoaded = EditorData.IsNewSceneLoaded;

        if (newSceneLoaded != NetworkToolbar.isNewSceneLoaded)
        {
            isNewSceneLoaded = newSceneLoaded;
            EditorData.IsNewSceneLoaded = isNewSceneLoaded;
            changed = true;
        }

        if (wasSynced != syncEnabled)
        {
            syncEnabled = wasSynced;
            EditorData.SyncEnabled = syncEnabled;
            changed = true;
        }

        if (currentHumanPlayers != lastHumanPlayers)
        {
            lastHumanPlayers = currentHumanPlayers;
            maxAI = MaxNumberOfPlayers - currentHumanPlayers;
            EditorData.AIPlayersCount = Mathf.Clamp(EditorData.AIPlayersCount, 0, maxAI);
            changed = true;
        }
        else if (currentAIPlayers != lastAIPlayers)
        {
            lastAIPlayers = currentAIPlayers;
            maxHumans = MaxNumberOfPlayers - currentAIPlayers;
            EditorData.HumanPlayersCount = Mathf.Clamp(EditorData.HumanPlayersCount, 1, maxHumans);
            changed = true;
        }

        if (changed)
        {
            if (AutoNetworkBootstrap.Instance != null)
            {

                AutoNetworkBootstrap.Instance.Initialize(EditorData);
            }

            EditorData.IsNewSceneLoaded = false;
        }

        string currentScenePath = SceneManager.GetActiveScene().path;
        bool isNullOrEmpty = string.IsNullOrEmpty(EditorData.ScenePath);

        if (ClonesManager.IsClone())
        {

            if (!isNullOrEmpty && EditorData.ScenePath != currentScenePath)
            {
                OpenScene(EditorData.ScenePath);
            }
        }
    }
    private static void OnToolbarGUI()
    {
        if (Application.isPlaying) { return; }

        GUIStyle dropdownButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            border = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(8, 20, 4, 4),
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            normal = {
            background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f, 1f)),
            textColor = Color.white
        },
        };

        GUIStyle arrowStyle = new GUIStyle(dropdownButtonStyle)
        {
            padding = new RectOffset(0, 2, 0, 0),
            fontSize = 8,
            alignment = TextAnchor.MiddleRight
        };

        // Check if we're in the main scene
        bool isMainScene = CurrentSceneName == AutoNetworkBootstrap.Instance?.MainLoginScene;

        GUILayout.FlexibleSpace();

        if (ClonesManager.IsClone())
        {
            GUILayout.BeginHorizontal();

            if (!isMainScene)
            {
                GUILayout.Label($"Players: {EditorData.HumanPlayersCount}", dropdownButtonStyle);
                GUILayout.Space(2);
                GUILayout.Label($"AI: {EditorData.AIPlayersCount}", dropdownButtonStyle);
            }

            GUILayout.EndHorizontal();
            return;
        }

        dropdownButtonStyle.hover = new GUIStyleState
        {
            background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.5f, 1f)),
            textColor = Color.white
        };

        dropdownButtonStyle.focused = new GUIStyleState
        {
            background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.4f, 1f)),
            textColor = Color.white
        };

        GUILayout.FlexibleSpace();

        List<(string, Action)> playersItems = new List<(string, Action)>();
        List<(string, Action)> aiItems = new List<(string, Action)>();
        List<(string, Action)> sceneItems = new List<(string, Action)>();

        GUILayout.BeginHorizontal(dropdownButtonStyle);

        EditorData.SyncEnabled = GUILayout.Toggle(EditorData.SyncEnabled, " Sync", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUILayout.EndHorizontal();

        if (!isMainScene)
        {
            // Draw Players Dropdown
            Rect playerButtonRect = GUILayoutUtility.GetRect(new GUIContent($"Players {EditorData.HumanPlayersCount}"), dropdownButtonStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (GUI.Button(playerButtonRect, $"Players {EditorData.HumanPlayersCount}", dropdownButtonStyle))
            {
                if (playerDropdownOpen)
                {
                    CustomDropdownWindow.CurrentWindow?.Close();
                    playerDropdownOpen = false;
                }
                else
                {
                    for (int i = 1; i <= maxHumans; i++)
                    {
                        int playerCount = i;
                        playersItems.Add(($"{i} Player", () =>
                        {
                            EditorData.HumanPlayersCount = playerCount;
                            playerDropdownOpen = false;
                        }
                        ));
                    }
                    CustomDropdownWindow.Show(playerButtonRect, playersItems);
                    playerDropdownOpen = true;
                    sceneDropdownOpen = false;
                    aiDropdownOpen = false;
                }
            }
            GUI.Label(new Rect(playerButtonRect.x + playerButtonRect.width - 15, playerButtonRect.y, 15, playerButtonRect.height), "▼", arrowStyle);

            GUILayout.Space(2);

            // Draw AI Dropdown
            Rect aiButtonRect = GUILayoutUtility.GetRect(new GUIContent($"AI {EditorData.AIPlayersCount}"), dropdownButtonStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (GUI.Button(aiButtonRect, $"AI {EditorData.AIPlayersCount}", dropdownButtonStyle))
            {
                if (aiDropdownOpen)
                {
                    CustomDropdownWindow.CurrentWindow?.Close();
                    aiDropdownOpen = false;
                }
                else
                {
                    for (int i = 0; i <= maxAI; i++)
                    {
                        int aiCount = i;
                        aiItems.Add(($"{i} AI", () =>
                        {
                            EditorData.AIPlayersCount = aiCount;
                            aiDropdownOpen = false;
                        }
                        ));
                    }
                    CustomDropdownWindow.Show(aiButtonRect, aiItems);
                    aiDropdownOpen = true;
                    sceneDropdownOpen = false;
                    playerDropdownOpen = false;
                }
            }
            GUI.Label(new Rect(aiButtonRect.x + aiButtonRect.width - 15, aiButtonRect.y, 15, aiButtonRect.height), "▼", arrowStyle);

            GUILayout.Space(2);
        }

        // Scene Dropdown (always visible)
        bool isInBuildSettings = CheckCurrentSceneInBuildSettings(out string currentScenePath);
        string sceneLabel = isInBuildSettings ? $"Scene: {CurrentSceneName}" : "Scene: +";
        Rect sceneButtonRect = GUILayoutUtility.GetRect(new GUIContent(sceneLabel), dropdownButtonStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (GUI.Button(sceneButtonRect, sceneLabel, dropdownButtonStyle))
        {
            if (sceneDropdownOpen)
            {
                CustomDropdownWindow.CurrentWindow?.Close();
                sceneDropdownOpen = false;
            }
            else
            {
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    if (sceneName != CurrentSceneName)
                    {
                        sceneItems.Add((sceneName, () =>
                        {
                            OpenScene(scenePath);
                            sceneDropdownOpen = false;
                        }
                        ));
                    }
                }

                if (!isInBuildSettings)
                {
                    sceneItems.Add(("Add Scene", () =>
                    {
                        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
                        scenes.Add(new EditorBuildSettingsScene(currentScenePath, true));
                        EditorBuildSettings.scenes = scenes.ToArray();

                        sceneDropdownOpen = false;
                    }
                    ));
                }

                CustomDropdownWindow.Show(sceneButtonRect, sceneItems);
                sceneDropdownOpen = true;
                playerDropdownOpen = false;
                aiDropdownOpen = false;
            }
        }
        GUI.Label(new Rect(sceneButtonRect.x + sceneButtonRect.width - 15, sceneButtonRect.y, 15, sceneButtonRect.height), "▼", arrowStyle);
    }
    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
    private static void OpenScene(string scenePath)
    {
        if (Application.isPlaying) { return; }

        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError($"Scene at path '{scenePath}' does not exist.");
            return;
        }

        if (!ClonesManager.IsClone())
        {
            bool saveCurrentModifiedScenesIfUserWantsTo = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        }

        EditorData.ScenePath = scenePath;
        EditorSceneManager.OpenScene(scenePath);
        CurrentSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        EditorData.IsNewSceneLoaded = true;
        Debug.Log($"Scene '{CurrentSceneName}' opened successfully.");


    }
    private static bool CheckCurrentSceneInBuildSettings(out string scenePath)
    {
        if (Application.isPlaying)
        {
            scenePath = "";
            return false;
        }

        scenePath = SceneManager.GetActiveScene().path;

        if (string.IsNullOrEmpty(scenePath))
        {
            return false;
        }

        bool isSceneInBuildSettings = false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneUtility.GetScenePathByBuildIndex(i) == scenePath)
            {
                isSceneInBuildSettings = true;
                CurrentSceneName = SceneManager.GetActiveScene().name;
                break;
            }
        }

        return isSceneInBuildSettings;
    }

}

#endif
