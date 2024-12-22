using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class RegisterPlayerListEvent : EventArgsBase
    {
        public ICollection<IPlayerBase> Players { get; }
        public RegisterPlayerListEvent(ICollection<IPlayerBase> players)
        {
            Players = players;
          
        }
    }
}