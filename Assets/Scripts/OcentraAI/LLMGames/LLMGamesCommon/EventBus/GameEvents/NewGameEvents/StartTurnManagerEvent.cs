using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class StartTurnManagerEvent : EventArgsBase
    {
        public IPlayerManager PlayerManager { get; }
        public StartTurnManagerEvent(IPlayerManager playerManager)
        {
           
            PlayerManager = playerManager;
        }
    }
}