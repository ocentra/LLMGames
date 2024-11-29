
using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RegisterPlayerEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IPlayerData player)> PlayerDataSource { get; }
        public IPlayerData PlayerData { get; }
        public RegisterPlayerEvent(IPlayerData playerData, UniTaskCompletionSource<(bool success, IPlayerData player)> playerDataSource)
        {
            PlayerData = playerData;
            PlayerDataSource = playerDataSource;
        }
    }
}