using System.Threading;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateLobbyEvent : EventArgsBase
    {
        public string SearchLobbyName { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public UpdateLobbyEvent(CancellationTokenSource cancellationTokenSource, string searchLobbyName)
        {
            CancellationTokenSource = cancellationTokenSource;
            SearchLobbyName = searchLobbyName;
        }

    }
}