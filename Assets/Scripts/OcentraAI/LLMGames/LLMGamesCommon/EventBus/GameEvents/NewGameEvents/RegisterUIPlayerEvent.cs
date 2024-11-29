using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class RegisterUIPlayerEvent : EventArgsBase
    {
        public ICollection<IPlayerBase> Players { get; }

        public RegisterUIPlayerEvent(ICollection<IPlayerBase> players)
        {
            Players = players;
        }
    }
}