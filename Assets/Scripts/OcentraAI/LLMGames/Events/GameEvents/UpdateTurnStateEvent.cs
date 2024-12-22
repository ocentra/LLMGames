
using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateTurnStateEvent<T> : EventArgsBase
    {
        public T CurrentLLMPlayer { get; }
        public bool IsHumanTurn { get; }
        public bool IsComputerTurn { get; }
        public UpdateTurnStateEvent(T currentLLMPlayer, bool isHumanTurn, bool isComputerTurn)
        {
            CurrentLLMPlayer = currentLLMPlayer;
            IsHumanTurn = isHumanTurn;
            IsComputerTurn = isComputerTurn;
        }
        
    }
}