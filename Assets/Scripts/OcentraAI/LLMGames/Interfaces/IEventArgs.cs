using System;

namespace OcentraAI.LLMGames.Events
{
    public interface IEventArgs
    {
        DateTime Timestamp { get; }
        public Guid UniqueIdentifier { get; }
        public bool IsRePublishable { get; set; }
        public void Dispose();
    }
}