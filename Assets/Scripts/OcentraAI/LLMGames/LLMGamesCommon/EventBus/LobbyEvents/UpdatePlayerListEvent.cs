using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace OcentraAI.LLMGames.Events
{
    public class UpdatePlayerListEvent : EventArgsBase
    {
        public List<Player> PlayerList { get; }
        public UpdatePlayerListEvent(List<Player> players)
        {
           
            PlayerList = players;
        }
    }
}