using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerActionEvent : EventArgs
    {
        public PlayerAction Action { get; }
        public Type CurrentPlayerType { get; }
        public PlayerActionEvent(Type currentPlayerType, PlayerAction action)
        {
            CurrentPlayerType = currentPlayerType;
            Action = action;
        }
    }
}