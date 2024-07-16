using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateFloorCardList : EventArgs
    {
        public Card Card { get; }


        public UpdateFloorCardList(Card card)
        {
            Card = card;
        }
    }
}