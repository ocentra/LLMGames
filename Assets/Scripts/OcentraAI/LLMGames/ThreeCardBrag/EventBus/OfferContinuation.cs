using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class OfferContinuation : EventArgs
    {
        public float Delay { get; }
        public GameManager GameManager { get; }

        public OfferContinuation(GameManager gameManager, float delay)
        {
            Delay = delay;
            GameManager = gameManager;
        }
    }
}