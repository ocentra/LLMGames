using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using static System.String;

public static class GameLogger
{
    private static string logFilePath;
    private static bool isInitialized = false;

    public static void Initialize()
    {
        if (isInitialized) return;

        string fileName = "GameLog.txt";
        string logDirectory = Empty;

#if UNITY_EDITOR
        string assetsPath = Application.dataPath;
        logDirectory = Path.Combine(assetsPath, "Logs");
        logFilePath = Path.Combine(logDirectory, fileName);

        // Refresh AssetDatabase on Play mode state change
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
        logDirectory = Application.persistentDataPath;
        logFilePath = Path.Combine(logDirectory, fileName);
#endif

        // Ensure the directory exists
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Create or overwrite the log file
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, false))
            {
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log file created");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create log file: {ex.Message}");
        }

        isInitialized = true;
    }

    public static void Log(string message, string method = "")
    {
        if (!isInitialized) Initialize();

        string formattedMessage = IsNullOrEmpty(method)
            ? $"{message} "
            : $"{message} in method {method}";

        try
        {
            if (GameSettings.Instance.UnityLogError && message.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError(formattedMessage);
            }
            else if (GameSettings.Instance.UnityLogWarning && message.StartsWith("WARNING:", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning(formattedMessage);
            }
            else if (GameSettings.Instance.UnityLog)
            {
                Debug.Log(formattedMessage);
            }

            if (GameSettings.Instance.FileLog)
            {
                string fileLogMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {formattedMessage}";
                using StreamWriter writer = File.AppendText(logFilePath);
                writer.WriteLine(fileLogMessage);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write to log file: {ex.Message}");
        }
    }

    public static void LogError(string message, string method = "", bool fileLog = true, bool unityLog = true)
    {
        Log($"ERROR: {message}", method);
    }

    public static void LogWarning(string message, string method = "", bool fileLog = true, bool unityLog = true)
    {
        Log($"WARNING: {message}", method);
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.ExitingPlayMode)
        {
            AssetDatabase.Refresh();
        }
    }
#endif
}
