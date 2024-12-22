using System;

namespace OcentraAI.LLMGames.Events
{
    public abstract class EventArgsBase : EventArgs, IEventArgs, IDisposable
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public Guid UniqueIdentifier { get; } = Guid.NewGuid();
        public bool IsRePublishable { get; set; } = false;

        private bool disposed;

        protected EventArgsBase() { }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from being called.
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
            }
        }

        ~EventArgsBase()
        {
            Dispose(false);
        }

        public override string ToString()
        {
            return $"{GetType().Name} [Timestamp: {Timestamp}, UniqueIdentifier: {UniqueIdentifier}, IsRePublishable: {IsRePublishable}]";
        }
    }
}