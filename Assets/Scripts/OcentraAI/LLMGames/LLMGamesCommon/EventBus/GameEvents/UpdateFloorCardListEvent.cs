using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateFloorCardListEvent<T> : EventArgsBase
    {
        public List<T> FloorCards { get; }

        public bool Reset { get; }

        public UpdateFloorCardListEvent(List<T> floorCards, bool reset = false)
        {
            FloorCards = floorCards;
            Reset = reset;
        }

 
    }
}