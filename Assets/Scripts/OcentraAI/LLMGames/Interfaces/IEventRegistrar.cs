using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public interface IEventRegistrar
    {
        public void Subscribe<T>(Action<T> handler) where T : IEventArgs;
        public void Subscribe<T>(Func<T, UniTask> handler) where T : IEventArgs;
        public void UnsubscribeAll();

    }
}