using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdatePlayerHandDisplayEvent<T> : EventArgsBase
    {    
        public T Player { get; }
        public bool IsRoundEnd { get; set; }
        public UpdatePlayerHandDisplayEvent(T player, bool isRoundEnd = false)
        {
            Player = player;
            IsRoundEnd = isRoundEnd;
        }


    }
}