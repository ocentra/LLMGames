using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateFloorCardList : EventArgs
    {
        public Card Card { get; }

        public bool Reset { get; }
        public UpdateFloorCardList(Card card, bool reset = false)
        {
            Card = card;
            Reset = reset;
        }
    }
}