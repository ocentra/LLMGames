using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateUIInteractabilityEvent : EventArgsBase
    {
        public bool IsInteractable { get; }

        public UpdateUIInteractabilityEvent(bool isInteractable)
        {
            IsInteractable = isInteractable;
        }
    }
}