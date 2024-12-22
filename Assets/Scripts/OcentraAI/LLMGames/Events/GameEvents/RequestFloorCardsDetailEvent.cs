using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RequestFloorCardsDetailEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, string card)> FloorCardDetails { get; }

        public RequestFloorCardsDetailEvent(UniTaskCompletionSource<(bool success, string card)> floorCardDetails)
        {
            FloorCardDetails = floorCardDetails;
        }
    }
}