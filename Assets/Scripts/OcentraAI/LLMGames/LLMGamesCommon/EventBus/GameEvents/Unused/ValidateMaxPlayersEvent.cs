using Cysharp.Threading.Tasks;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class ValidateMaxPlayersEvent : EventArgsBase
    {
        public UniTaskCompletionSource<int> MaxPlayersSetSource { get; }
        public int MaxPlayers { get; }
        public ValidateMaxPlayersEvent(UniTaskCompletionSource<int> maxPlayersSetSource, int maxPlayers)
        {
            MaxPlayersSetSource = maxPlayersSetSource;
            MaxPlayers = maxPlayers;
        }
    }
}