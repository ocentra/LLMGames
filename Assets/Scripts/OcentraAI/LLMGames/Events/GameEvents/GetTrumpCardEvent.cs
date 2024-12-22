using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class GetTrumpCardEvent<T> : EventArgsBase
    {
        public UniTaskCompletionSource<T> CompletionSource { get; }

        public GetTrumpCardEvent()
        {
            CompletionSource = new UniTaskCompletionSource<T>();
        }

        public void SetCard(T card)
        {
            CompletionSource.TrySetResult(card);
        }

        public UniTask<T> WaitForCard()
        {
            return CompletionSource.Task;
        }
    }
}