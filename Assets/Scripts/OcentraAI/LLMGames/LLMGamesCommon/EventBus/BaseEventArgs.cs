using System;

namespace OcentraAI.LLMGames.Events
{
    public abstract class EventArgsBase : EventArgs, IEventArgs
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        protected EventArgsBase()
        {

        }
        
    }
}