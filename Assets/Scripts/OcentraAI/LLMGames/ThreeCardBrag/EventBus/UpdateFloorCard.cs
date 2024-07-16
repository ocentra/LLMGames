using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateFloorCard : EventArgs
    {
        public Card Card { get; }
        public UpdateFloorCard()
        {

        }

        public UpdateFloorCard(Card card)
        {
            Card = card;
        }
    }
}