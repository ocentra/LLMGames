using OcentraAI.LLMGames.Utilities;
using System;

namespace OcentraAI.LLMGames.Events
{
    public interface IEventArgs
    {
        DateTime Timestamp { get; }
    }
}