using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class RequestHumanPlayersDataEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IReadOnlyList<IHumanPlayerData> player)> PlayerDataSource { get; }

        public RequestHumanPlayersDataEvent(UniTaskCompletionSource<(bool success, IReadOnlyList<IHumanPlayerData> player)> playerDataSource)
        {
            PlayerDataSource = playerDataSource;

        }
    }
}