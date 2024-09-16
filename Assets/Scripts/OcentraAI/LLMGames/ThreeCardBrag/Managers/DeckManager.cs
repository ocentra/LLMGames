using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public class DeckManager : ManagerBase<DeckManager>
    {
        [ShowInInspector, ReadOnly] public List<Card> DeckCards { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public List<Card> FloorCards { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public Card BackCard => Deck.Instance.BackCard;
        [ShowInInspector, ReadOnly] public int TotalCards => Deck.Instance.CardTemplates.Count;
        [ShowInInspector, ReadOnly] public int RemainingCards => DeckCards.Count;
        [ShowInInspector, ReadOnly] public int FloorCardsCount => FloorCards.Count;
        [ShowInInspector, ReadOnly] public Card FloorCard { get; set; }
        [ShowInInspector, ReadOnly] public Card SwapCard { get; set; }
        [ShowInInspector, ReadOnly] public Dictionary<string, Card> WildCards { get; private set; } = new Dictionary<string, Card>();
        [ShowInInspector, ReadOnly] private Queue<Card> LastDrawnWildCards { get; set; } = new Queue<Card>();

        private TurnManager TurnManager => TurnManager.Instance;
        private GameManager GameManager => GameManager.Instance;
        private GameMode GameMode => GameManager.GameMode;

        protected override void Awake()
        {
            base.Awake();
            DeckCards = new List<Card>(Deck.Instance.CardTemplates);

        }

        public void OnSetFloorCard(SetFloorCard e)
        {
            if (e.SwapCard != null)
            {
                OnSetFloorCardList(e.SwapCard);
                FloorCard = null;
                EventBus.Publish(new UpdateFloorCard(null));
            }
            else
            {
                if (FloorCard != null)
                {
                    OnSetFloorCardList(FloorCard);
                }
                FloorCard = DrawCard();
                EventBus.Publish(new UpdateFloorCard(FloorCard));
            }
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

        public void OnSetFloorCardList(Card floorCard)
        {
            if (!FloorCards.Contains(floorCard))
            {
                FloorCards.Add(floorCard);
                EventBus.Publish(new UpdateFloorCardList(floorCard));
            }
        }

        public void SetRandomWildCards()
        {
            List<Card> cards = new List<Card>(Deck.Instance.CardTemplates);

            if (cards.Count == 0) return;

            WildCards.Clear();

            HashSet<Card> selectedCards = new HashSet<Card>();

            // Select Trump Card
            Card trumpCard = null;

#if UNITY_EDITOR
            // Editor-specific DevModeManager logic
            if (GameSettings.Instance.DevModeEnabled)
            {
                Card devTrumpCard = DevModeManager.Instance.TrumpDevCard;
                if (devTrumpCard != null)
                {
                    trumpCard = cards.FirstOrDefault(c => c.Suit == devTrumpCard.Suit && c.Rank == devTrumpCard.Rank);
                    if (trumpCard != null)
                    {
                        WildCards["TrumpCard"] = trumpCard;
                    }
                }
            }
            else
#endif
            {
                // Randomly select a Trump Card
                bool validCardFound = false;
                while (!validCardFound)
                {
                    int randomIndex = Random.Range(0, cards.Count);
                    trumpCard = cards[randomIndex];

                    if (!selectedCards.Contains(trumpCard) && !LastDrawnWildCards.Contains(trumpCard))
                    {
                        validCardFound = true;
                        selectedCards.Add(trumpCard);
                        WildCards["TrumpCard"] = trumpCard;
                    }
                }
            }

            // Select Magic Cards
            List<Card> magicCards = new List<Card>();
            while (magicCards.Count < 4)
            {
                int randomIndex = Random.Range(0, cards.Count);
                Card magicCard = cards[randomIndex];

                if (!selectedCards.Contains(magicCard) && !LastDrawnWildCards.Contains(magicCard))
                {
                    magicCards.Add(magicCard);
                    selectedCards.Add(magicCard);
                }
            }

            WildCards["MagicCard0"] = magicCards[0];
            WildCards["MagicCard1"] = magicCards[1];
            WildCards["MagicCard2"] = magicCards[2];
            WildCards["MagicCard3"] = magicCards[3];

            // Update the queue with the new wild cards
            LastDrawnWildCards.Enqueue(trumpCard);
            LastDrawnWildCards.Enqueue(magicCards[0]);
            LastDrawnWildCards.Enqueue(magicCards[1]);
            LastDrawnWildCards.Enqueue(magicCards[2]);
            LastDrawnWildCards.Enqueue(magicCards[3]);

            // Ensure the queue only retains the last 10 wild cards
            while (LastDrawnWildCards.Count > 10)
            {
                LastDrawnWildCards.Dequeue();
            }

            // Publish event with new wild cards
            EventBus.Publish(new UpdateWildCards(WildCards, GameMode));
        }




        public void ResetForNewGame()
        {
            ResetForNewRound();
            LastDrawnWildCards = new Queue<Card>();
        }

        public void ResetForNewRound()
        {
            FloorCards.Clear();
            Shuffle();

            SetRandomWildCards();
            FloorCard = null;
            EventBus.Publish(new UpdateFloorCard(null));
            EventBus.Publish(new UpdateFloorCardList(null, true));
        }

        private void Log(string message)
        {
            GameLogger.Log($"{nameof(DeckManager)} {message}");
        }

        private void LogError(string message)
        {
            GameLogger.LogError($"{nameof(DeckManager)} {message}");
        }

        public void RemoveCardsFromDeck(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                DeckCards.RemoveAll(c => c.Id == card.Id);
            }
        }
    }
}
