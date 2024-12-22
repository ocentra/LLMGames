
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateWildCardsEvent<T> : EventArgsBase
    {
        public Dictionary<PlayerDecision, T> WildCards { get; }
        public UpdateWildCardsEvent(Dictionary<PlayerDecision, T> wildCards)
        {
            WildCards = wildCards;

        }
    }
}