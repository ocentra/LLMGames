using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RequestRemainingCardsCountEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, int cards)> RemainingCards { get; }

        public RequestRemainingCardsCountEvent(UniTaskCompletionSource<(bool success, int cards)> remainingCards)
        {
            RemainingCards = remainingCards;

        }
    }
}