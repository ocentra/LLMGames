using System;

namespace OcentraAI.LLMGames.Events
{
    public class JoinLobbyEvent : EventArgsBase
    {
        public string LobbyId { get; }
        public bool IsProtectedLobby { get; }

        public JoinLobbyEvent(string lobbyId, bool isProtectedLobby = false)
        {
            LobbyId = lobbyId;
            IsProtectedLobby = isProtectedLobby;

        }

    }
}