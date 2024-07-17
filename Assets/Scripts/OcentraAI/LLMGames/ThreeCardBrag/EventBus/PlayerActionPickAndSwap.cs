using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerActionPickAndSwap : EventArgs
    {
        public  Card FloorCard { get; }
        public Card SwapCard { get; }
        public Type CurrentPlayerType { get; }
        public PlayerActionPickAndSwap(Type currentPlayerType, Card floorCard, Card swapCard)
        {
            CurrentPlayerType = currentPlayerType;
            FloorCard = floorCard;
            SwapCard = swapCard;
        }
    }
}