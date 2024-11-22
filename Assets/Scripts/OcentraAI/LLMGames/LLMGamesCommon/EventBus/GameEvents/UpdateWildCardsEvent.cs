
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateWildCardsEvent<T,TC> : EventArgsBase
    {
        public Dictionary<string, TC> WildCards { get; }
        public T GameMode { get; }
        public UpdateWildCardsEvent(Dictionary<string, TC> wildCards, T gameMode)
        {
            WildCards = wildCards;
            GameMode = gameMode;
        }


    }
}