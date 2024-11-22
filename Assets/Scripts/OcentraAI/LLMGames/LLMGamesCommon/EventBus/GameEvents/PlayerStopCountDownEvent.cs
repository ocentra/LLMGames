
using System;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerStopCountDownEvent <T>: EventArgsBase
    {
        public T CurrentLLMPlayer { get; }

        public PlayerStopCountDownEvent(T currentLLMPlayer)
        {
            CurrentLLMPlayer = currentLLMPlayer;
        }


    }
}