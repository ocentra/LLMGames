using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class InitializeUIPlayersEvent<T> : EventArgsBase
    {
        public InitializeUIPlayersEvent(UniTaskCompletionSource<bool> completionSource, List<T> players)
        {
            CompletionSource = completionSource;
            Players = players;
        }

        public UniTaskCompletionSource<bool> CompletionSource { get; }
        public List<T> Players { get; }
    }
}