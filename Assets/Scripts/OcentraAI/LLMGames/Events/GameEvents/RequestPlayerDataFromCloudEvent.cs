using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;

namespace OcentraAI.LLMGames.Events
{
    public class RequestPlayerDataFromCloudEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IAuthPlayerData playerData)> PlayerDataSource { get; }
        public string PlayerId { get; }
        public RequestPlayerDataFromCloudEvent(UniTaskCompletionSource<(bool success, IAuthPlayerData playerData)> playerDataSource, string playerId)
        {
            PlayerDataSource = playerDataSource;
            PlayerId = playerId;
        }
    }
}