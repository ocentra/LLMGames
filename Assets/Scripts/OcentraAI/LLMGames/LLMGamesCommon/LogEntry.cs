using System;

namespace OcentraAI.LLMGames.Utilities
{
    [Serializable]
    public class LogEntry
    {

        public string Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string CloneId { get; private set; }
        public string PlayerName { get; set; }
        public LogEntry(string timestamp, string level, string message, string playerName)
        {
            Timestamp = timestamp;
            Level = level;
            Message = message;
            PlayerName = playerName;
#if UNITY_EDITOR
            CloneId = ParrelSync.ClonesManager.GetArgument();

#endif

        }
    }
}