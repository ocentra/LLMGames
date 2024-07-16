using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class OfferNewGame : EventArgs
    {
        public float Delay { get; }

        public OfferNewGame(float delay)
        {
            Delay = delay;
        }
    }
}