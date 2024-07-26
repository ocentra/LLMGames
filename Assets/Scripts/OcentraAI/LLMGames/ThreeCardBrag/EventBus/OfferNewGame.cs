using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class OfferNewGame : EventArgs
    {
        public float Delay { get; }

        public GameManager GameManager { get; }
        public OfferNewGame(GameManager gameManager,float delay)
        {
            Delay = delay;
            GameManager = gameManager;
        }
    }
}