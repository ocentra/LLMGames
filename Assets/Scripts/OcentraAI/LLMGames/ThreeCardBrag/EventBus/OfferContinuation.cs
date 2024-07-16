using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class OfferContinuation : EventArgs
    {
        public float Delay { get; }

        public OfferContinuation(float delay)
        {
            Delay = delay;
        }
    }
}