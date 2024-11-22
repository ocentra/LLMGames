using System;
using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateLobbyPlayerListEvent : EventArgsBase
    {
        public Lobby Lobby { get; }
        public bool IsHost { get; }
        public UpdateLobbyPlayerListEvent(Lobby lobby, bool isHost )
        {
            Lobby = lobby;
            IsHost = isHost;
        }

    }
}