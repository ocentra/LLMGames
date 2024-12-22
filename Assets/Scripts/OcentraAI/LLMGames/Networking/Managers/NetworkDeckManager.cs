using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace OcentraAI.LLMGames.Networking.Manager
{
    public class NetworkDeckManager : NetworkManagerBase
    {

        [ShowInInspector, ReadOnly] public List<Card> DeckCards { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public List<Card> FloorCardList { get; private set; } = new List<Card>();
        [ShowInInspector, ReadOnly] public Card BackCard => Deck.Instance.BackCard;
        [ShowInInspector, ReadOnly] public int TotalCards => Deck.Instance.CardTemplates.Count;
        [ShowInInspector, ReadOnly] public int RemainingCards => DeckCards.Count;
        [ShowInInspector, ReadOnly] public int FloorCardsCount => FloorCardList.Count;
        [ShowInInspector, ReadOnly] public Card FloorCard { get; set; }
        [ShowInInspector, ReadOnly] public Card SwapCard { get; set; }

        [ShowInInspector, ReadOnly, DictionaryDrawerSettings]
        public Dictionary<PlayerDecision, Card> WildCards { get; private set; } = new Dictionary<PlayerDecision, Card>();

        [ShowInInspector, ReadOnly] private Queue<Card> LastDrawnWildCards { get; set; } = new Queue<Card>();



        public override void InitComponents()
        {
            base.InitComponents();

            DeckCards = new List<Card>(Deck.Instance.CardTemplates);
        }



        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<SetFloorCardEvent<Card>>(OnSetFloorCardEvent);
            EventRegistrar.Subscribe<GetTrumpCardEvent<Card>>(OnGetTrumpCardEvent);
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
                GameLoggerScriptable.LogError($"Error during Shuffle: {ex.Message}", this, ToEditor, ToFile, UseStackTrace);
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
                GameLoggerScriptable.LogError($"Error during DrawCard: {ex.Message}", this);
                return null;
            }
        }


        public async UniTask SetFloorCardList(Card floorCard)
        {
            try
            {
                if (floorCard != null)
                {
                    if (!FloorCardList.Contains(floorCard))
                    {
                        FloorCardList.Add(floorCard);
                    }
                }

                bool success = await EventBus.Instance.PublishAsync(new UpdateFloorCardListEvent<Card>(FloorCardList));

                FloorCard = null;

            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in OnSetFloorCardList: {ex.Message}", this);
            }
        }

        public async UniTask SetRandomWildCards()
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

                IEnumerable<PlayerDecision> extraGameplayDecisions = PlayerDecision.GetExtraGamePlayDecisions();

                foreach (PlayerDecision decision in extraGameplayDecisions)
                {
                    if (decision.Equals(PlayerDecision.Trump) && !GameMode.UseTrump)
                    {
                        continue;
                    }

                    if (!GameMode.UseMagicCards &&
                        (decision.Equals(PlayerDecision.WildCard0) ||
                         decision.Equals(PlayerDecision.WildCard1) ||
                         decision.Equals(PlayerDecision.WildCard2) ||
                         decision.Equals(PlayerDecision.WildCard3)))
                    {
                        continue;
                    }

                    bool validCardFound = false;

                    while (!validCardFound)
                    {
                        int randomIndex = Random.Range(0, cards.Count);
                        Card selectedCard = cards[randomIndex];

                        if (!selectedCards.Contains(selectedCard) && !LastDrawnWildCards.Contains(selectedCard))
                        {
                            validCardFound = true;
                            selectedCards.Add(selectedCard);
                            WildCards[decision] = selectedCard;
                        }
                    }

                    LastDrawnWildCards.Enqueue(WildCards[decision]);
                }

                while (LastDrawnWildCards.Count > 10)
                {
                    LastDrawnWildCards.Dequeue();
                }

                await EventBus.Instance.PublishAsync(new UpdateWildCardsEvent<Card>(WildCards));


            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in SetRandomWildCards: {ex.Message}", this);
            }
        }


        private async UniTask OnGetTrumpCardEvent(GetTrumpCardEvent<Card> arg)
        {
            if (WildCards.TryGetValue(PlayerDecision.Trump, out Card card))
            {
                arg.CompletionSource.TrySetResult(card);
            }
            else
            {
                arg.CompletionSource.TrySetException(new Exception(" Trump Card Not Found!"));
            }

            await UniTask.Yield();
        }

        public async UniTask<bool> ResetForNewGame()
        {
            try
            {
                LastDrawnWildCards = new Queue<Card>();
                await UniTask.Yield();
                return true;

            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in ResetForNewGame: {ex.Message}", this);
                return false;
            }
        }

        public async UniTask<bool> ResetForNewRound()
        {
            try
            {
                DeckCards = new List<Card>(Deck.Instance.CardTemplates);
                FloorCardList.Clear();
                Shuffle();
                await SetRandomWildCards();

                FloorCard = null;
                await EventBus.Instance.PublishAsync(new UpdateFloorCardEvent<Card>(FloorCard));
                await EventBus.Instance.PublishAsync(new UpdateFloorCardListEvent<Card>(FloorCardList, true));

                return true;
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in ResetForNewRound: {ex.Message}", this);
                return false;
            }
        }

        public async UniTask OnSetFloorCardEvent(SetFloorCardEvent<Card> setFloorCardEvent)
        {
            try
            {
                if (setFloorCardEvent.SwapCard != null)
                {
                    FloorCard = setFloorCardEvent.SwapCard;
                    await EventBus.Instance.PublishAsync(new UpdateFloorCardEvent<Card>(FloorCard));
                }
                else
                {
                    if (FloorCard != null)
                    {
                        await SetFloorCardList(FloorCard);
                    }

                    FloorCard = DrawCard();
                    await EventBus.Instance.PublishAsync(new UpdateFloorCardEvent<Card>(FloorCard));
                }
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in OnSetFloorCard: {ex.Message}", this);
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
                GameLoggerScriptable.LogError($"Error in RemoveCardsFromDeck: {ex.Message}", this);
            }
        }

    }
}