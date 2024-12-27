using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class WaitForInitializationEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>> CompletionSource { get; }
        public Type SourceType { get; }
        public Type TargetType { get; }
        public int Priority { get; }
        protected WaitForInitializationEvent() { }
        public WaitForInitializationEvent(UniTaskCompletionSource<IOperationResult<IMonoBehaviourBase>> completionSource, Type sourceType, Type targetType, int priority = 0)
        {
            CompletionSource = completionSource;
            SourceType = sourceType;
            TargetType = targetType;
            Priority = priority;
        }
    }

}