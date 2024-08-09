using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateFloorCard : EventArgs
    {
        public Card Card { get; }
        
        public UpdateFloorCard(Card card)
        {
            Card = card;
        }
    }

    public class UpdateWildCards : EventArgs
    {
        public Dictionary<string, Card> WildCards { get; }
        public GameMode GameMode { get; }

        public UpdateWildCards(Dictionary<string, Card> wildCards,GameMode gameMode)
        {
            WildCards = wildCards;
            GameMode = gameMode;
        }
    }


    public class UpdateWildCardsHighlight : EventArgs
    {
        public Dictionary<string, Card> WildCardsInHand { get; }
        public bool IsHighlighted { get; }

        public UpdateWildCardsHighlight(Dictionary<string, Card> wildCardsInHand, bool isHighlighted = false)
        {
            WildCardsInHand = wildCardsInHand;
            IsHighlighted = isHighlighted;
        }
    }


}