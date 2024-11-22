using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerLeftLobbyEvent : EventArgsBase
    {
        public string PlayerId { get; }
        public UniTaskCompletionSource<bool> LeaveCompletionSource { get; }
        public PlayerLeftLobbyEvent(string playerId, UniTaskCompletionSource<bool> leaveCompletionSource)
        {
            PlayerId = playerId;
            LeaveCompletionSource = leaveCompletionSource;
        }
    }
}