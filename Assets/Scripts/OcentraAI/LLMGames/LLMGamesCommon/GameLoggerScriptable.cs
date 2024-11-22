using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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

        [SerializeField, HideInInspector] private bool toEditor  = true;
        [SerializeField, HideInInspector] private bool toFile  =false;
        [SerializeField, HideInInspector] private bool useStackTrace = true;
        [SerializeField, HideInInspector] private bool isLoggingEnabled = true;
        [SerializeField, HideInInspector] private bool logWarningsEnabled  = true;
        [SerializeField, HideInInspector] private bool logErrorsEnabled  = true;

        [ShowInInspector] public bool ToEditor { get=> toEditor; set => toEditor = value; } 
        [ShowInInspector] public bool ToFile { get => toFile; set => toFile = value; }
        [ShowInInspector] public bool UseStackTrace { get => useStackTrace; set => useStackTrace = value; }
        [ShowInInspector] public bool IsLoggingEnabled { get => isLoggingEnabled; set => isLoggingEnabled = value; }
        [ShowInInspector] public bool LogWarningsEnabled { get => logWarningsEnabled; set => logWarningsEnabled = value; }
        [ShowInInspector] public bool LogErrorsEnabled { get => logErrorsEnabled; set => logErrorsEnabled = value; }

        [ShowInInspector, ReadOnly] private string PlayerName { get; set; }

        public int LogFrequencyInSeconds { get; set; } = 10;
        private  Queue<LogEntry> LogQueue { get; set; } = new Queue<LogEntry>();
        private float LastFlushTime { get; set; }

        private readonly object logQueueLock = new object();
        private readonly object logFilePathWriteLock = new object();

        [FilePath(AbsolutePath = true, Extensions = "json"), ValidateInput(nameof(IsValidJsonFile), "Selected file is not a valid JSON file.")]
        public string LogFilePath  = Application.isEditor ? Path.Combine(Application.dataPath, "Resources", "LogFile.json") : Path.Combine(Application.persistentDataPath, "LogFile.json");

        private bool IsValidJsonFile(string path)
        {
            if (IsNullOrEmpty(path) || !File.Exists(path) || Path.GetExtension(path).ToLower() != ".json")
            {
                return false;
            }
            return true;
        }

        void Awake()
        {
            PlayerName = null;
            toFile = !IsNullOrEmpty(LogFilePath);
            LogQueue = new Queue<LogEntry>();
            toEditor = toEditor && Application.isEditor;
            LastFlushTime = 0;
        }
        

        public void Log(string message, Object context, bool toEditor = default, bool toFile = default, bool useStackTrace = true, [CallerMemberName] string method = "", [CallerLineNumber] int sourceLineNumber = 0)
        {

            string className = context.GetType().Name;
            string fullMessage = $"{className}.{method}.{sourceLineNumber} : {message}";

            toEditor = toEditor == default ? this.toEditor && isLoggingEnabled : Application.isEditor;
            toFile = toFile == default ? this.toFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;
            useStackTrace = useStackTrace == default ? this.useStackTrace : Application.isEditor;

            if (toEditor)
            {
                UnityEngine.Debug.LogFormat(LogType.Log, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
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

            toEditor = toEditor == default ? this.toEditor && logWarningsEnabled : Application.isEditor;

            toFile = toFile == default ? this.toFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;

            useStackTrace = useStackTrace == default ? this.useStackTrace : Application.isEditor;

            if (toEditor)
            {
                UnityEngine.Debug.LogFormat(LogType.Warning, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
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

            toEditor = toEditor == default ? this.toEditor && logErrorsEnabled : Application.isEditor;
            toFile = toFile == default ? this.toFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;
            useStackTrace = useStackTrace == default ? this.useStackTrace : Application.isEditor;
            if (toEditor)
            {
                UnityEngine.Debug.LogFormat(LogType.Error, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
            }

            if (toFile)
            {
                AddToLogQueue(fullMessage);
            }
        }

        public void LogException(string message, Object context, bool toEditor = default, bool toFile = default, bool useStackTrace = true,[CallerMemberName] string method = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            string className = context.GetType().Name;
            string fullMessage = $"[Exception]{className}.{method}.{sourceLineNumber} : {message}";

            toEditor = toEditor == default ? this.toEditor && logErrorsEnabled : Application.isEditor;
            toFile = toFile == default ? this.toFile : IsNullOrEmpty(LogFilePath) && !Application.isEditor;
            useStackTrace = useStackTrace == default ? this.useStackTrace : Application.isEditor;

            if (toEditor)
            {
                UnityEngine.Debug.LogFormat(LogType.Exception, useStackTrace ? LogOption.None : LogOption.NoStacktrace, context, "{0}.{1}. Line: {2}: {3}", className, method, sourceLineNumber, message);
            }

            if (toFile)
            {
                AddToLogQueue(fullMessage);
            }
        }
        

        private async void AddToLogQueue(string message, string level = "Info")
        {
            if (IsNullOrEmpty(PlayerName) && Application.isPlaying )
            {
                UniTaskCompletionSource<OperationResult<IPlayerData>> uniTaskCompletionSource = new UniTaskCompletionSource<OperationResult<IPlayerData>>();
                await EventBus.Instance.PublishAsync(new GetLocalPlayer(uniTaskCompletionSource));

                OperationResult<IPlayerData> result = null;
                try
                {
                    result = await uniTaskCompletionSource.Task;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to retrieve local player data: {ex.Message}");
                }

                if (result is { IsSuccess: true })
                {
                    PlayerName = result.Value.PlayerName.Value.Value;
                }
            }


            lock (logQueueLock)
            {
                LogEntry logEntry = new LogEntry(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level, message, PlayerName);
                LogQueue.Enqueue(logEntry);
            }

            if (Time.realtimeSinceStartup - LastFlushTime > LogFrequencyInSeconds)
            {
                await FlushLogToFile();
                LastFlushTime = Time.realtimeSinceStartup;
            }
        }

        public async UniTask<bool> FlushLogToFile()
        {
            try
            {
                List<LogEntry> entriesToWrite = new List<LogEntry>();

                lock (logQueueLock)
                {
                    while (LogQueue.Count > 0)
                    {
                        entriesToWrite.Add(LogQueue.Dequeue());
                    }
                }

                lock (logFilePathWriteLock)
                {
                    using StreamWriter writer = new StreamWriter(LogFilePath, append: true);
                    foreach (LogEntry entry in entriesToWrite)
                    {
                        string jsonEntry = JsonConvert.SerializeObject(entry, Formatting.Indented);
                        writer.WriteLineAsync(jsonEntry).AsUniTask();
                    }
                }
                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
                return false;
            }

            return true;
        }
        

        [Button]
        public void ClearLogFile()
        {
            if (IsNullOrEmpty(LogFilePath) || !File.Exists(LogFilePath))
            {
                UnityEngine.Debug.LogWarning("Log file path is invalid or the file does not exist.");
                return;
            }

            try
            {
                lock (logFilePathWriteLock)
                {
                    using (StreamWriter writer = new StreamWriter(LogFilePath, false))
                    {
                        writer.Write(string.Empty);
                    }
                }

                UnityEngine.Debug.Log("Log file cleared successfully.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to clear the log file: {ex.Message}");
            }
        }

        protected override async UniTask<bool> ApplicationWantsToQuit()
        {
            await base.ApplicationWantsToQuit();

            if (toFile)
            {
                await FlushLogToFile();
            }

            return true;
        }
    }
}