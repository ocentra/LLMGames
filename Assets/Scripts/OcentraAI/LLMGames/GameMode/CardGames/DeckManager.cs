using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.Manager
{
    public class DeckSingletonManager : SingletonManagerBase<DeckSingletonManager>
    {
        [ShowInInspector, ReadOnly] public List<Card> DeckCards { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public List<Card> FloorCards { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public Card BackCard => Deck.Instance.BackCard;
        [ShowInInspector, ReadOnly] public int TotalCards => Deck.Instance.CardTemplates.Count;
        [ShowInInspector, ReadOnly] public int RemainingCards => DeckCards.Count;
        [ShowInInspector, ReadOnly] public int FloorCardsCount => FloorCards.Count;
        [ShowInInspector, ReadOnly] public Card FloorCard { get; set; }
        [ShowInInspector, ReadOnly] public Card SwapCard { get; set; }

        [ShowInInspector, ReadOnly, DictionaryDrawerSettings]
        public Dictionary<string, Card> WildCards { get; private set; } = new Dictionary<string, Card>();

        [ShowInInspector, ReadOnly] private Queue<Card> LastDrawnWildCards { get; set; } = new Queue<Card>();



        protected override async UniTask InitializeAsync()
        {
            try
            {
                DeckCards = new List<Card>(Deck.Instance.CardTemplates);
                await base.InitializeAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error during DeckManager initialization: {ex.Message}", this);
            }
        }



        public void Shuffle()
        {
            try
            {
                for (int i = 0; i < DeckCards.Count; i++)
                {
                    Card temp = DeckCards[i];
                    int randomIndex = Random.Range(i, DeckCards.Count);
                    DeckCards[i] = DeckCards[randomIndex];
                    DeckCards[randomIndex] = temp;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error during Shuffle: {ex.Message}", this);
            }
        }

        public Card DrawCard()
        {
            try
            {
                if (DeckCards.Count == 0)
                {
                    return null;
                }

                Shuffle();
                Card card = DeckCards[0];
                DeckCards.RemoveAt(0);
                return card;
            }
            catch (Exception ex)
            {
                LogError($"Error during DrawCard: {ex.Message}", this);
                return null;
            }
        }

        public async UniTask OnSetFloorCardList(Card floorCard)
        {
            try
            {
                if (!FloorCards.Contains(floorCard))
                {
                    FloorCards.Add(floorCard);
                   // await EventBus.Instance.PublishAsync(new UpdateFloorCardListEvent<Card>(floorCard));
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in OnSetFloorCardList: {ex.Message}", this);
            }
        }

        public async UniTask SetRandomWildCards(GameMode gameMode)
        {
            try
            {
                List<Card> cards = new List<Card>(Deck.Instance.CardTemplates);

                if (cards.Count == 0)
                {
                    return;
                }

                WildCards.Clear();

                HashSet<Card> selectedCards = new HashSet<Card>();

                // Select Trump Card
                Card trumpCard = null;

                // Editor-specific DevModeManager logic
                if (GameSettings.Instance.DevModeEnabled && Application.isEditor)
                {
                    //todo Fix this 

                    //#if UNITY_EDITOR
                    //                    Card devTrumpCard = DevTools.DevModeManager.Instance.TrumpDevCard;
                    //                    if (devTrumpCard != null)
                    //                    {
                    //                        trumpCard = cards.FirstOrDefault(
                    //                            c => c.Suit == devTrumpCard.Suit && c.Rank == devTrumpCard.Rank);
                    //                        if (trumpCard != null)
                    //                        {
                    //                            WildCards["TrumpCard"] = trumpCard;
                    //                        }
                    //                    }

                    //#endif

                }

                else

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
                //await EventBus.Instance.PublishAsync(new UpdateWildCardsEvent<GameMode, Card>(WildCards, gameMode));
            }
            catch (Exception ex)
            {
                LogError($"Error in SetRandomWildCards: {ex.Message}", this);
            }
        }

        public async UniTask<bool> ResetForNewGame(GameMode gameMode)
        {
            try
            {
                bool roundResetSuccess = await ResetForNewRound(gameMode);

                if (roundResetSuccess)
                {
                    LastDrawnWildCards = new Queue<Card>();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewGame: {ex.Message}", this);
                return false;
            }
        }

        public async UniTask<bool> ResetForNewRound(GameMode gameMode)
        {
            try
            {
                FloorCards.Clear();
                Shuffle();

                await SetRandomWildCards(gameMode);

                FloorCard = null;
                //await EventBus.Instance.PublishAsync(new UpdateFloorCardEvent<Card>(null));
               // await EventBus.Instance.PublishAsync(new UpdateFloorCardListEvent<Card>(null, true));

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewRound: {ex.Message}", this);
                return false;
            }
        }


        public void RemoveCardsFromDeck(List<Card> cards)
        {
            try
            {
                foreach (Card card in cards)
                {
                    DeckCards.RemoveAll(c => c.Id == card.Id);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in RemoveCardsFromDeck: {ex.Message}", this);
            }
        }


    }
}