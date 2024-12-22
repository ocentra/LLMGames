using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class RequestComputerPlayersDataEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IReadOnlyList<IComputerPlayerData> player)> PlayerDataSource { get; }

        public RequestComputerPlayersDataEvent(UniTaskCompletionSource<(bool success, IReadOnlyList<IComputerPlayerData> player)> playerDataSource)
        {
            PlayerDataSource = playerDataSource;

        }
    }
}