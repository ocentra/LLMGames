using System;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerActionEvent<T> : EventArgsBase
    {
        public T Action { get; }
        public Type CurrentPlayerType { get; }

        public PlayerActionEvent(Type currentPlayerType, T action)
        {
            CurrentPlayerType = currentPlayerType;
            Action = action;
        }


    }
}