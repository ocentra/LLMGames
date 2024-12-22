using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public interface ILeaderboard
    {
        Dictionary<ulong, ILeaderboardEntry> Entries { get; }
        public bool HasClearWinner();
    }
}