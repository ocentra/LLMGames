using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateFloorCardListEvent<T> : EventArgsBase
    {
        public T Card { get; }

        public bool Reset { get; }

        public UpdateFloorCardListEvent(T card, bool reset = false)
        {
            Card = card;
            Reset = reset;
        }

 
    }
}