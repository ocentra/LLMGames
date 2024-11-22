using System;

namespace OcentraAI.LLMGames.Events
{
    public class UIMessageEvent : EventArgsBase
    {
        public UIMessageEvent(string message, float delay)
        {
            Message = message;
            Delay = delay;
        }

        public string Message { get; }
        public float Delay { get; }
    }
}