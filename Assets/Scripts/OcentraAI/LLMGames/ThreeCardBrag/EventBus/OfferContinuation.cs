using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class OfferContinuation : EventArgs
    {
        public float Delay { get; }
        public string Message { get; }
        public OfferContinuation(float delay, string message)
        {
            Delay = delay;
            Message = message;
        }
    }
}