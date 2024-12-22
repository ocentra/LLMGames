using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RequestPlayerHandDetailEvent : EventArgsBase
    {
        public ulong PlayerId;
        public UniTaskCompletionSource<(bool success, string hand)> PlayerHandDetails { get; }

        public RequestPlayerHandDetailEvent( ulong playerID, UniTaskCompletionSource<(bool success, string card)> playerHandDetails)
        {
            PlayerHandDetails = playerHandDetails;
            PlayerId = playerID;
        }
    }
}