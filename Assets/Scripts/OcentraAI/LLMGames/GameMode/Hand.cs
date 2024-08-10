using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class Hand
    {
        [OdinSerialize, ShowInInspector]
        public Card[] Cards { get; private set; }

        public Hand()
        {
            
        }

        public Hand(IOrderedEnumerable<Card> cards)
        {
            Cards = cards.ToArray();
        }

        public Hand(Card[] cards)
        {
            Cards = cards;
        }

        public Hand(List<Card> cards)
        {
            Cards = cards.ToArray();
        }

        public void SetCards(Card[] newCards)
        {
            if (newCards == null || newCards.Length == 0)
            {
                return;
            }

            Cards = newCards;
        }



    }
}
