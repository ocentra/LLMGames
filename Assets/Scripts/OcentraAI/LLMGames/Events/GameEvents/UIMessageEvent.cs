using System;

namespace OcentraAI.LLMGames.Events
{
    public class UIMessageEvent : EventArgsBase
    {
        public string Message { get; }
        public float Delay { get; }
        public string ButtonName { get; }
        public UIMessageEvent(string buttonName, string message, float delay)
        {
            Message = message;
            Delay = delay;
            ButtonName = buttonName;
        }


    }
}