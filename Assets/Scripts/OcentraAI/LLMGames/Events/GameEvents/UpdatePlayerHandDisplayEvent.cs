using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdatePlayerHandDisplayEvent : EventArgsBase
    {    
        public IPlayerBase Player { get; }
        public bool IsRoundEnd { get; set; }
      
        public UpdatePlayerHandDisplayEvent(IPlayerBase player, bool isRoundEnd = false)
        {
            Player = player;
          
            IsRoundEnd = isRoundEnd;
        }


    }
}