using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class RequestActivePlayersEvent : EventArgsBase
    {
        public UniTaskCompletionSource<(bool success, IReadOnlyList<IPlayerBase> player)> PlayerDataSource { get; }

        public RequestActivePlayersEvent(UniTaskCompletionSource<(bool success, IReadOnlyList<IPlayerBase> player)> playerDataSource)
        {
            PlayerDataSource = playerDataSource;

        }
    }
}