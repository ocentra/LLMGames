
using System;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerActionPickAndSwapEvent<T> : EventArgsBase
    {

        public T FloorCard { get; }
        public T SwapCard { get; }
        public Type CurrentPlayerType { get; }

        public PlayerActionPickAndSwapEvent(Type currentPlayerType, T floorCard, T swapCard)
        {
            CurrentPlayerType = currentPlayerType;
            FloorCard = floorCard;
            SwapCard = swapCard;
        }

    }
}