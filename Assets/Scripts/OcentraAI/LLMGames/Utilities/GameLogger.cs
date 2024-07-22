using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OcentraAI.LLMGames.Utilities
{
    public static class GameLogger
    {
        private static string logFilePath;
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;

            string fileName = "GameLog.txt";

#if UNITY_EDITOR
            string assetsPath = Application.dataPath;
            Directory.CreateDirectory(Path.Combine(assetsPath, "Logs"));
            logFilePath = Path.Combine(assetsPath, "Logs", fileName);

            // Refresh AssetDatabase on Play mode state change
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            logFilePath = Path.Combine(Application.persistentDataPath, fileName);
#endif

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

        public static void Log(string message)
        {
            if (!isInitialized) Initialize();

            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine(formattedMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }

        public static void LogError(string message)
        {
            Log($"ERROR: {message}");
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                AssetDatabase.Refresh();
            }
        }
#endif
    }
}
