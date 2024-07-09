using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{

    public class DeckManager
    {
        [ShowInInspector]
        public List<Card> DeckCards { get; private set; } = new List<Card>();

        [ShowInInspector]
        public List<Card> FloorCards { get; private set; } = new List<Card>();

        [ShowInInspector]
        public Card BackCard => Deck.Instance.BackCard;

        [ShowInInspector]
        public int TotalCards => Deck.Instance.CardTemplates.Count;
        [ShowInInspector]
        public int RemainingCards => DeckCards.Count;

        [ShowInInspector]
        public int FloorCardsCount => FloorCards.Count;

        [ShowInInspector]
        public Card FloorCard { get; private set; }

        [ShowInInspector]
        public Card SwapCard { get; private set; }

        [ShowInInspector]
        public Card TrumpCard { get; private set; }

        [ShowInInspector]
        private Queue<Card> LastDrawnTrumpCards { get; set; } = new Queue<Card>();
        public DeckManager()
        {
            InitializeDeck();
        }



        public void SetFloorCard(Card card)
        {
            FloorCard = card;
        }

        public void InitializeDeck()
        {
            DeckCards = new List<Card>(Deck.Instance.CardTemplates);
            FloorCards.Clear();
            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = 0; i < DeckCards.Count; i++)
            {
                Card temp = DeckCards[i];
                int randomIndex = Random.Range(i, DeckCards.Count);
                DeckCards[i] = DeckCards[randomIndex];
                DeckCards[randomIndex] = temp;
            }
        }

        public Card DrawCard()
        {
            if (DeckCards.Count == 0) return null;
            Shuffle();
            Card card = DeckCards[0];
            DeckCards.RemoveAt(0);
            return card;
        }

        public void AddToFloorCardList(Card card)
        {
            if (!FloorCards.Contains(card))
            {
                FloorCards.Add(card);
                GameManager.Instance.UIController.UpdateFloorCards(card);

            }

        }

        public void SetRandomTrumpCard()
        {
            List<Card> cards = new List<Card>(Deck.Instance.CardTemplates);

            if (cards.Count == 0) return;

            Card trumpCard = null;
            bool validCardFound = false;
            while (!validCardFound)
            {
                int randomIndex = Random.Range(0, cards.Count);
                trumpCard = cards[randomIndex];

                if (!LastDrawnTrumpCards.Contains(trumpCard))
                {
                    validCardFound = true;
                    TrumpCard = trumpCard;
                }
            }

            LastDrawnTrumpCards.Enqueue(trumpCard);

            if (LastDrawnTrumpCards.Count > 10)
            {
                LastDrawnTrumpCards.Dequeue();
            }
        }


        public void ResetForNewGame()
        {
            Reset();
            LastDrawnTrumpCards = new Queue<Card>();

        }

        public void Reset()
        {
            InitializeDeck();

        }

        public void SetSwapCard(Card card)
        {
            SwapCard = card;
        }
    }
}