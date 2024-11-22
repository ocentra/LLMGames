using System;

namespace OcentraAI.LLMGames.Events
{
    public class TimerUpdateEvent : EventArgsBase
    {
        public float RemainingSeconds { get; }

        public TimerUpdateEvent(float remainingSeconds)
        {
            RemainingSeconds = remainingSeconds;
        }
    }
}