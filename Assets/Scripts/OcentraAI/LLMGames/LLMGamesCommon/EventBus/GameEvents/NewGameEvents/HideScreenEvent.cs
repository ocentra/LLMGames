using System;

namespace OcentraAI.LLMGames.Events
{
    public class HideScreenEvent : EventArgsBase
    {
        public string ScreenToHide { get; }
        public HideScreenEvent(string screenToHide)
        {
            ScreenToHide = screenToHide;
        }
    }
}