using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class OfferNewGame : EventArgs
    {
        public float Delay { get; }
        public string Message { get; }
        public OfferNewGame(float delay, string message)
        {
            Delay = delay;
            Message = message;
        }
    }
}