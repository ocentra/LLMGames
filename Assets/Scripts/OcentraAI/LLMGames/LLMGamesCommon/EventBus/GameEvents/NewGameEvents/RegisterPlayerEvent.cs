
using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RegisterPlayerEvent : EventArgsBase
    {
        public UniTaskCompletionSource<IPlayerData> PlayerDataSource { get; }
        public IPlayerData PlayerData { get; }
        public RegisterPlayerEvent(IPlayerData playerData, UniTaskCompletionSource<IPlayerData> playerDataSource)
        {
            PlayerData = playerData;
            PlayerDataSource = playerDataSource;
        }
    }
}