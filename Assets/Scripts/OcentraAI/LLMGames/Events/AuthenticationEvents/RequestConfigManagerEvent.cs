using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RequestConfigManagerEvent<T> : EventArgsBase where T : IMonoBehaviourBase
    {
        public UniTaskCompletionSource<IConfigManager> CompletionSource { get; }
        public RequestConfigManagerEvent(UniTaskCompletionSource<IConfigManager> completionSource)
        {
            CompletionSource = completionSource;
        }
    }
}