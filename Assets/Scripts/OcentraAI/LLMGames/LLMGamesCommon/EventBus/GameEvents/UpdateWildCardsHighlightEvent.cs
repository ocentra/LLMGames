
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateWildCardsHighlightEvent<T> : EventArgsBase
    {

        public Dictionary<string, T> WildCardsInHand { get; }
        public bool IsHighlighted { get; }
        public UpdateWildCardsHighlightEvent(Dictionary<string, T> wildCardsInHand, bool isHighlighted = false)
        {
            WildCardsInHand = wildCardsInHand;
            IsHighlighted = isHighlighted;
        }


    }
}