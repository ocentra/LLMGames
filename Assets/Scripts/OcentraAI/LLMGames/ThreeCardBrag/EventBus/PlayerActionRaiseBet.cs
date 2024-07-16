using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerActionRaiseBet : EventArgs
    {
        public string Amount;
        public Type CurrentPlayerType { get; }
        public PlayerActionRaiseBet(Type currentPlayerType, string amount)
        {
            CurrentPlayerType = currentPlayerType;
            Amount = amount;
        }
    }
}