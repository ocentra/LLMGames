using OcentraAI.LLMGames.Scriptable;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class SetFloorCard : EventArgs
    {
        public Card SwapCard { get; }
        public SetFloorCard()
        {
            
        }

        public SetFloorCard(Card swapCard)
        {
            SwapCard = swapCard;
        }
    }


}