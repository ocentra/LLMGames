using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RequestScoreManagerDetailsEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, int pot,int currentBet)> ScoreManagerDetails { get; }

        public RequestScoreManagerDetailsEvent(UniTaskCompletionSource<(bool success, int pot, int currentBet)> scoreManagerDetails)
        {
            ScoreManagerDetails = scoreManagerDetails;

        }
    }
}