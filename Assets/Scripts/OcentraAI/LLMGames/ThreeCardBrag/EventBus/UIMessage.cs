using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UIMessage : EventArgs
    {
        public string Message { get; }
        public float Delay { get; }

        public UIMessage(string message, float delay)
        {
            Message = message;
            Delay = delay;
        }
    }
}