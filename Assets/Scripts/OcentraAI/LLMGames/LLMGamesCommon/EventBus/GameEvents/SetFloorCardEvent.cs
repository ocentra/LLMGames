
using System;

namespace OcentraAI.LLMGames.Events
{
    public class SetFloorCardEvent<T> : EventArgsBase
    {
        public SetFloorCardEvent()
        {
        }

        public SetFloorCardEvent(T swapCard)
        {
            SwapCard = swapCard;
        }

        public T SwapCard { get; }
    }
}