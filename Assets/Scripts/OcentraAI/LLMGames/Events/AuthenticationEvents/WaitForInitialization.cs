using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class WaitForInitializationEvent<T> : EventArgsBase where T : IManager
    {
        public UniTaskCompletionSource<bool> CompletionSource { get; }
        public WaitForInitializationEvent(UniTaskCompletionSource<bool> completionSource)
        {
            CompletionSource = completionSource;
        }
    }
}