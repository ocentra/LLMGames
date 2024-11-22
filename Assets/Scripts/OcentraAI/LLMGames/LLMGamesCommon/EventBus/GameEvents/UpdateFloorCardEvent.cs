
using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateFloorCardEvent<T> : EventArgsBase
    {
        public T Card { get; }
        public UpdateFloorCardEvent(T card)
        {
            Card = card;
        }


    }
}