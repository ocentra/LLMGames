using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
#if true
using UnityEditor;
#endif
using UnityEngine;
using static System.String;
using Object = UnityEngine.Object;


namespace OcentraAI.LLMGames.Utilities
{
    [CreateAssetMenu(fileName = nameof(GameLoggerScriptable), menuName = "LLMGames/GameLoggerScriptable")]
    [GlobalConfig("Assets/Resources/")]
    public class GameLoggerScriptable : CustomGlobalConfig<GameLoggerScriptable>
    {
        [Header("Global Logging Settings")]
        [SerializeField, HideInInspector] private int logFrequencyInSeconds = 10;
        [SerializeField, HideInInspector] private bool logToFile = false;
        [SerializeField, HideInInspector] private bool logStackTrace = true;
        [SerializeField, HideInInspector] private bool loggingEnabled = true;
        [SerializeField, HideInInspector] private bool logWarnings = true;
        [SerializeField, HideInInspector] private bool logErrors = true;
        [SerializeField, HideInInspector] private bool logException = true;

        [ShowInInspector]
        private bool ToFile
        {
            get => logToFile;
            set
            {
                logToFile = value;
                OnValueChanged(nameof(ToFile), value);
            }
        }

        [ShowInInspector]
        private bool UseStackTrace
        {
            get => logStackTrace;
            set
            {
                logStackTrace = value;
                OnValueChanged(nameof(UseStackTrace), value);
            }
        }

        [ShowInInspector]
        private bool IsLoggingEnabled
        {
            get => loggingEnabled;
            set
            {
                loggingEnabled = value;
                OnValueChanged(nameof(IsLoggingEnabled), value);
            }
        }

        [ShowInInspector]
        private bool LogWarningsEnabled
        {
            get => logWarnings;
            set
            {
                logWarnings = value;
                OnValueChanged(nameof(LogWarningsEnabled), value);
            }
        }

        [ShowInInspector]
        private bool LogErrorsEnabled
        {
            get => logErrors;
            set
            {
                logErrors = value;
                OnValueChanged(nameof(LogErrorsEnabled), value);
            }
        }

        [ShowInInspector]
        private bool LogExceptionEnabled
        {
            get => logException;
            set
            {
                logException = value;
                OnValueChanged(nameof(LogExceptionEnabled), value);
            }
        }



        [ShowInInspector, ReadOnly] private string PlayerName { get; set; }


        private Queue<LogEntry> LogQueue { get; set; } = new Queue<LogEntry>();
        private float LastFlushTime { get; set; }

        private readonly object logQueueLock = new object();
        private readonly object logFilePathWriteLock = new object();

        [Sirenix.OdinInspector.FilePath(AbsolutePath = true, Extensions = "json"), ValidateInput(nameof(IsValidJsonFile), "Selected file is not a valid JSON file.")]
        public string LogFilePath = Application.isEditor ? Path.Combine(Application.dataPath, "Resources", "LogFile.json") : Path.Combine(Application.persistentDataPath, "LogFile.json");

        private bool IsValidJsonFile(string path)
        {
            if (IsNullOrEmpty(path) || !File.Exists(path) || Path.GetExtension(path).ToLower() != ".json")
            {
                return false;
            }
            return true;
        }

        private void OnValueChanged(string propertyName, object newValue)
        {

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            Debug.Log($"{propertyName} changed to {newValue}");
            SaveChanges();
#endif
        }

        void Awake()
        {
            PlayerName = null;
            LogQueue = new Queue<LogEntry>();
            LastFlushTime = 0;
        }


        public void Log(string message, Object context, bool toEditor = default, bool toFile = default, bool useStackTrace = true, [CallerMemberName] string method = "", [CallerLineNumber] int sourceLineNumber = 0)
        {

            string className = context.GetType().Name;
            string fullMessage = $"{className}.{method}.{sourceLineNumber} : {message}";

            toEditor = toEditor == default ? IsLoggingEnabled : Application.isEditor;
            toFile = toFile == default ? ToFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;
            useStackTrace = useStackTrace == default ? UseStackTrace : Application.isEditor;

            if (toEditor)
            {
                Debug.LogFormat(LogType.Log, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
            }

            if (toFile)
            {
                AddToLogQueue(fullMessage);
            }
        }

        public void LogWarning(string message, Object context, bool toEditor = default, bool toFile = default, bool useStackTrace = true, [CallerMemberName] string method = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string className = context.GetType().Name;
            string fullMessage = $"[Warning]{className}.{method}.{sourceLineNumber} : {message}";

            toEditor = toEditor == default ? LogWarningsEnabled : Application.isEditor;
            toFile = toFile == default ? ToFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;

            useStackTrace = useStackTrace == default ? UseStackTrace : Application.isEditor;

            if (toEditor)
            {
                Debug.LogFormat(LogType.Warning, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
            }

            if (toFile)
            {
                AddToLogQueue(fullMessage);
            }
        }

        public void LogError(string message, Object context, bool toEditor = default, bool toFile = default, bool useStackTrace = true, [CallerMemberName] string method = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string className = context.GetType().Name;
            string fullMessage = $"[Error]{className}.{method}.{sourceLineNumber} : {message}";

            toEditor = toEditor == default ? LogErrorsEnabled : Application.isEditor;
            toFile = toFile == default ? ToFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;
            useStackTrace = useStackTrace == default ? UseStackTrace : Application.isEditor;
            if (toEditor)
            {
                Debug.LogFormat(LogType.Error, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
            }

            if (toFile)
            {
                AddToLogQueue(fullMessage);
            }
        }

        public void LogException(string message, Object context, bool toEditor = default, bool toFile = default, bool useStackTrace = true, [CallerMemberName] string method = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string className = context.GetType().Name;
            string fullMessage = $"[Exception]{className}.{method}.{sourceLineNumber} : {message}";

            toEditor = toEditor == default ? LogExceptionEnabled : Application.isEditor;
            toFile = toFile == default ? ToFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;
            useStackTrace = useStackTrace == default ? UseStackTrace : Application.isEditor;

            if (toEditor)
            {
                Debug.LogFormat(LogType.Exception, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
            }

            if (toFile)
            {
                AddToLogQueue(fullMessage);
            }
        }


        private async void AddToLogQueue(string message, string level = "Info")
        {
            if (IsNullOrEmpty(PlayerName) && Application.isPlaying)
            {
                UniTaskCompletionSource<OperationResult<IPlayerData>> uniTaskCompletionSource = new UniTaskCompletionSource<OperationResult<IPlayerData>>();
                bool resultOperation = await EventBus.Instance.PublishAsync(new GetLocalPlayerEvent(uniTaskCompletionSource));

                if (resultOperation)
                {
                    OperationResult<IPlayerData> result = null;
                    try
                    {
                        result = await uniTaskCompletionSource.Task;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to retrieve local player data: {ex.Message}");
                    }

                    if (result is { IsSuccess: true })
                    {
                        PlayerName = result.Value.PlayerName.Value.Value;
                    }
                }

            }


            lock (logQueueLock)
            {
                LogEntry logEntry = new LogEntry(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level, message, PlayerName);
                LogQueue.Enqueue(logEntry);
            }

            if (Time.realtimeSinceStartup - LastFlushTime > logFrequencyInSeconds)
            {
                await FlushLogToFile();
                LastFlushTime = Time.realtimeSinceStartup;
            }
        }

        private readonly SemaphoreSlim fileWriteSemaphore = new SemaphoreSlim(1, 1);

        public async UniTask<bool> FlushLogToFile()
        {
            List<LogEntry> entriesToWrite = new List<LogEntry>();

            // Safely dequeue log entries
            lock (logQueueLock)
            {
                while (LogQueue.Count > 0)
                {
                    entriesToWrite.Add(LogQueue.Dequeue());
                }
            }

            // Use SemaphoreSlim for asynchronous locking
            await fileWriteSemaphore.WaitAsync().AsUniTask();
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, append: true))
                {
                    foreach (LogEntry entry in entriesToWrite)
                    {
                        string jsonEntry = JsonConvert.SerializeObject(entry, Formatting.Indented);
                        await writer.WriteLineAsync(jsonEntry).AsUniTask();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
                return false;
            }
            finally
            {
                fileWriteSemaphore.Release(); // Ensure the semaphore is released
            }
        }




        [Button]
        public void ClearLogFile()
        {
            if (IsNullOrEmpty(LogFilePath) || !File.Exists(LogFilePath))
            {
                Debug.LogWarning("Log file path is invalid or the file does not exist.");
                return;
            }

            try
            {
                lock (logFilePathWriteLock)
                {
                    using (StreamWriter writer = new StreamWriter(LogFilePath, false))
                    {
                        writer.Write(Empty);
                    }
                }

                Debug.Log("Log file cleared successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear the log file: {ex.Message}");
            }
        }

        protected override async UniTask<bool> ApplicationWantsToQuit()
        {
            await base.ApplicationWantsToQuit();

            if (ToFile)
            {
                await FlushLogToFile();
            }

            return true;
        }


    }
}