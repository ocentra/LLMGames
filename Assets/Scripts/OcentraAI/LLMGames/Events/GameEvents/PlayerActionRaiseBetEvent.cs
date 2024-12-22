using System;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerActionRaiseBetEvent : EventArgsBase
    {
        public string Amount;

        public PlayerActionRaiseBetEvent(Type currentPlayerType, string amount)
        {
            CurrentPlayerType = currentPlayerType;
            Amount = amount;
        }

        public Type CurrentPlayerType { get; }
    }
}