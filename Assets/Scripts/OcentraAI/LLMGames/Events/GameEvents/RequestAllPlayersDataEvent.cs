using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class RequestAllPlayersDataEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IReadOnlyList<IPlayerBase> player)> PlayerDataSource { get; }

        public RequestAllPlayersDataEvent(UniTaskCompletionSource<(bool success, IReadOnlyList<IPlayerBase> player)> playerDataSource)
        {
            PlayerDataSource = playerDataSource;

        }
    }
}