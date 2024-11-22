using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateLobbyListEvent : EventArgsBase
    {
        public List<Lobby> Lobbies { get; }
        public UpdateLobbyListEvent(List<Lobby> lobbies)
        {
            Lobbies = lobbies;

        }

    }
}