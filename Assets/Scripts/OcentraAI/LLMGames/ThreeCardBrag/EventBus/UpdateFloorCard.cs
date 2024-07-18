using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateFloorCard : EventArgs
    {
        public Card Card { get; }
        public bool Reset { get; }
        
        public UpdateFloorCard(Card card, bool reset = false)
        {
            Card = card;
            Reset = reset;
        }
    }
}