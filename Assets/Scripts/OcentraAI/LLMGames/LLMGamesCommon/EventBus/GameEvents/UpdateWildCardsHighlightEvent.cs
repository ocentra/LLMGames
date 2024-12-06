
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateWildCardsHighlightEvent : EventArgsBase
    {

        public int WildCardsInHandId { get; } = -1;
        public UpdateWildCardsHighlightEvent( int isHighlighted )
        {
            WildCardsInHandId = WildCardsInHandId;
            
        }


    }
}