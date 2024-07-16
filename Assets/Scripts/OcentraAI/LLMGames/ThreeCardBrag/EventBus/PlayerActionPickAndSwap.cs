using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerActionPickAndSwap : EventArgs
    {
        public  Card PickCard { get; }
        public Card SwapCard { get; }
        public Type CurrentPlayerType { get; }
        public PlayerActionPickAndSwap(Type currentPlayerType, Card pickCard, Card swapCard)
        {
            CurrentPlayerType = currentPlayerType;
            PickCard = pickCard;
            SwapCard = swapCard;
        }
    }
}