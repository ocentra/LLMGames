
using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public class RegisterHumanPlayerEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IHumanPlayerData player)> PlayerDataSource { get; }
        public IHumanPlayerData HumanPlayerData { get; }
        public RegisterHumanPlayerEvent(IHumanPlayerData humanPlayerData, UniTaskCompletionSource<(bool success, IHumanPlayerData player)> playerDataSource)
        {
            HumanPlayerData = humanPlayerData;
            PlayerDataSource = playerDataSource;
        }
    }
}