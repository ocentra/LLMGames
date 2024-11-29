using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager;
using OcentraAI.LLMGames.Players;
using OcentraAI.LLMGames.Players.UI;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.UI.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Required] private PlayerManager PlayerManager => PlayerManager.Instance;
        [Required] private ScoreManager ScoreManager => ScoreManager.Instance;

        #region UI Elements

        [Required][ShowInInspector] private Button3D ShowPlayerHand { get; set; }
        [Required][ShowInInspector] private Button3D PlayBlind { get; set; }
        [Required][ShowInInspector] private Button3D RaiseBet { get; set; }
        [Required][ShowInInspector] private Button3D Fold { get; set; }
        [Required][ShowInInspector] private Button3D Bet { get; set; }
        [Required][ShowInInspector] private Button3D DrawFromDeck { get; set; }
        [Required][ShowInInspector] private Button3D ShowCall { get; set; }


        [Required][ShowInInspector] private Button3D PurchaseCoins { get; set; }
        [Required][ShowInInspector] private Transform MessageHolder { get; set; }
        [Required][ShowInInspector] private TMP_InputField RaiseAmount { get; set; }
        [Required][ShowInInspector] private TextMeshProUGUI Message { get; set; }
        [Required][ShowInInspector] private TextMeshPro Pot { get; set; }
        [Required][ShowInInspector] private TextMeshPro CurrentBet { get; set; }
        [ShowInInspector][Required] public GameObject MainTable { get; set; }


        [ShowInInspector] public PlayerUI CurrentPlayerUI { get; set; }
        [ShowInInspector] public LLMPlayer CurrentLLMPlayer { get; set; }

        [Required][ShowInInspector] private CardView FloorCardView { get; set; }
        [Required][ShowInInspector] private CardView TrumpCardView { get; set; }
        [Required][ShowInInspector] private CardView MagicCard0 { get; set; }
        [Required][ShowInInspector] private CardView MagicCard1 { get; set; }
        [Required][ShowInInspector] private CardView MagicCard2 { get; set; }
        [Required][ShowInInspector] private CardView MagicCard3 { get; set; }

        [Required][ShowInInspector] public CardView[] LocalPlayerCardViews { get; set; }
        [Required][ShowInInspector] public LeftPanelController LeftPanelController { get; set; }

        [ShowInInspector] public ButtonState ButtonState { get; private set; } = ButtonState.TakeAction;

        #endregion

        #region Unity Lifecycle Methods

        private void OnValidate()
        {
            Init();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            Init();
            SetupButtonListeners();
        }

        #endregion

        #region Initialization

        private void Init()
        {
            FindUIComponents();
            SetupInitialUIState();
        }

        private void FindUIComponents()
        {
            MessageHolder = transform.FindChildRecursively<Transform>(nameof(MessageHolder));

            ShowPlayerHand = GameObject.Find(nameof(ShowPlayerHand)).GetComponent<Button3D>();
            DrawFromDeck = GameObject.Find(nameof(DrawFromDeck)).GetComponent<Button3D>();

            PlayBlind = GameObject.Find(nameof(PlayBlind)).GetComponent<Button3D>();
            RaiseBet = GameObject.Find(nameof(RaiseBet)).GetComponent<Button3D>();
            Fold = GameObject.Find(nameof(Fold)).GetComponent<Button3D>();
            Bet = GameObject.Find(nameof(Bet)).GetComponent<Button3D>();

            ShowCall = GameObject.Find(nameof(ShowCall)).GetComponent<Button3D>();
            PurchaseCoins = GameObject.Find(nameof(PurchaseCoins)).GetComponent<Button3D>();


            if (ShowPlayerHand != null)
            {
                LocalPlayerCardViews = ShowPlayerHand.GetComponentsInChildren<CardView>(true);
            }

            FloorCardView = GameObject.Find(nameof(FloorCardView)).GetComponent<CardView>();
            TrumpCardView = GameObject.Find(nameof(TrumpCardView)).GetComponent<CardView>();
            MagicCard0 = GameObject.Find(nameof(MagicCard0)).GetComponent<CardView>();
            MagicCard1 = GameObject.Find(nameof(MagicCard1)).GetComponent<CardView>();
            MagicCard2 = GameObject.Find(nameof(MagicCard2)).GetComponent<CardView>();
            MagicCard3 = GameObject.Find(nameof(MagicCard3)).GetComponent<CardView>();

            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));

            Pot = transform.FindChildRecursively<TextMeshPro>(nameof(Pot));
            CurrentBet = transform.FindChildRecursively<TextMeshPro>(nameof(CurrentBet));


            MainTable = GameObject.Find(nameof(MainTable));

            RaiseAmount = GameObject.Find(nameof(RaiseAmount)).GetComponent<TMP_InputField>();

            LeftPanelController = FindAnyObjectByType<LeftPanelController>();
        }


        private void SetupInitialUIState()
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
            }

            if (ShowPlayerHand != null)
            {
                ShowPlayerHand.SetInteractable(true);
            }

            if (LocalPlayerCardViews is { Length: > 0 })
            {
                foreach (var cardView in LocalPlayerCardViews)
                {
                    cardView.SetCard(null);
                    cardView.UpdateCardView();
                }
            }

            ButtonState = ButtonState.TakeAction;
        }

        private void SetupButtonListeners()
        {
            if (ShowPlayerHand != null)
            {
                ShowPlayerHand.onClick.AddListener(() => OnPlayerSeeHand(ShowPlayerHand));
            }

            if (PlayBlind != null)
            {
                PlayBlind.onClick.AddListener(() => OnPlayBlind(PlayBlind));
            }

            if (RaiseBet != null)
            {
                RaiseBet.onClick.AddListener(() => OnRaiseBet(RaiseBet));
            }

            if (Fold != null)
            {
                Fold.onClick.AddListener(() => OnFold(Fold));
            }

            if (Bet != null)
            {
                Bet.onClick.AddListener(() => OnBet(Bet));
            }

            if (DrawFromDeck != null)
            {
                DrawFromDeck.onClick.AddListener(() => OnDrawFromDeck(DrawFromDeck));
            }

            if (ShowCall != null)
            {
                ShowCall.onClick.AddListener(() => OnShowCall(ShowCall));
            }

            if (PurchaseCoins != null)
            {
                PurchaseCoins.onClick.AddListener(OnPurchaseCoins);
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<NewGameEvent<GameManager>>(OnNewGame);
            EventBus.Instance.Subscribe<NewRoundEvent<GameManager>>(OnNewRound);
            EventBus.Instance.Subscribe<UpdateGameStateEvent<GameManager>>(OnUpdateGameState);
            EventBus.Instance.Subscribe<PlayerStartCountDownEvent<TurnManager>>(OnPlayerStartCountDown);
            EventBus.Instance.Subscribe<UpdateTurnStateEvent<LLMPlayer>>(OnUpdateTurnState);
            EventBus.Instance.Subscribe<PlayerStopCountDownEvent<LLMPlayer>>(OnPlayerStopCountDown);
            EventBus.Instance.Subscribe<UpdateFloorCardEvent<Card>>(OnUpdateFloorCard);
            EventBus.Instance.Subscribe<UpdateWildCardsEvent<GameMode, Card>>(OnUpdateWildCards);
            EventBus.Instance.Subscribe<UpdateWildCardsHighlightEvent<Card>>(OnUpdateWildCardsHighlight);
            EventBus.Instance.Subscribe<UpdateFloorCardListEvent<Card>>(OnUpdateFloorCardList);
            EventBus.Instance.Subscribe<UpdatePlayerHandDisplayEvent<LLMPlayer>>(OnUpdatePlayerHandDisplay);
            EventBus.Instance.Subscribe<UpdateRoundDisplayEvent<ScoreManager>>(OnUpdateRoundDisplay);
            EventBus.Instance.Subscribe<OfferContinuationEvent>(OnOfferContinuation);
            EventBus.Instance.Subscribe<OfferNewGameEvent>(OnOfferNewGame);
            EventBus.Instance.Subscribe<UIMessageEvent>(OnMessage);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<NewGameEvent<GameManager>>(OnNewGame);
            EventBus.Instance.Unsubscribe<NewRoundEvent<GameManager>>(OnNewRound);
            EventBus.Instance.Unsubscribe<UpdateGameStateEvent<GameManager>>(OnUpdateGameState);
            EventBus.Instance.Unsubscribe<PlayerStartCountDownEvent<TurnManager>>(OnPlayerStartCountDown);
            EventBus.Instance.Unsubscribe<UpdateTurnStateEvent<LLMPlayer>>(OnUpdateTurnState);
            EventBus.Instance.Unsubscribe<PlayerStopCountDownEvent<LLMPlayer>>(OnPlayerStopCountDown);
            EventBus.Instance.Unsubscribe<UpdateFloorCardEvent<Card>>(OnUpdateFloorCard);
            EventBus.Instance.Unsubscribe<UpdateWildCardsEvent<GameMode, Card>>(OnUpdateWildCards);
            EventBus.Instance.Unsubscribe<UpdateWildCardsHighlightEvent<Card>>(OnUpdateWildCardsHighlight);

            EventBus.Instance.Unsubscribe<UpdateFloorCardListEvent<Card>>(OnUpdateFloorCardList);
            EventBus.Instance.Unsubscribe<UpdatePlayerHandDisplayEvent<LLMPlayer>>(OnUpdatePlayerHandDisplay);
            EventBus.Instance.Unsubscribe<UpdateRoundDisplayEvent<ScoreManager>>(OnUpdateRoundDisplay);
            EventBus.Instance.Unsubscribe<OfferContinuationEvent>(OnOfferContinuation);
            EventBus.Instance.Unsubscribe<OfferNewGameEvent>(OnOfferNewGame);
            EventBus.Instance.Unsubscribe<UIMessageEvent>(OnMessage);
        }

        #endregion

        #region Player Actions

        private void OnPlayerSeeHand(Button3D button)
        {
            button.SetInteractable(false, false);
            TakeAction(PlayerAction.SeeHand, ButtonState.TakeAction);
        }

        private void OnPlayBlind(Button3D button)
        {
            button.SetInteractable(false);
            TakeAction(PlayerAction.PlayBlind, ButtonState.ActionTaken);
        }

        private void OnRaiseBet(Button3D button)
        {
            if (int.TryParse(RaiseAmount.text, out int raiseAmount) && raiseAmount > 0)
            {
                EventBus.Instance.Publish(new PlayerActionRaiseBetEvent(typeof(HumanLLMPlayer), raiseAmount.ToString()));
                SetButtonState(ButtonState.ActionTaken);
                button.SetInteractable(false);
            }
            else
            {
                ShowMessage("Please enter a valid raise amount.", 3f);
            }
        }

        private void OnFold(Button3D button)
        {
            button.SetInteractable(false);
            TakeAction(PlayerAction.Fold, ButtonState.ActionTaken);
        }

        private void OnBet(Button3D button)
        {
            button.SetInteractable(false);
            TakeAction(PlayerAction.Bet, ButtonState.ActionTaken);
        }

        private void OnDrawFromDeck(Button3D button)
        {
            button.SetInteractable(false);
            TakeAction(PlayerAction.DrawFromDeck, ButtonState.Draw);
            MainTableUI.Instance.ShowDrawnCard();
        }

        private void OnShowCall(Button3D button)
        {
            button.SetInteractable(false);
            TakeAction(PlayerAction.Show, ButtonState.ActionTaken);
        }

        private void OnPurchaseCoins()
        {
            EventBus.Instance.Publish(new PurchaseCoinsEvent(1000));
            HideMessage();
        }

        private void TakeAction(PlayerAction action, ButtonState buttonState)
        {
            SetButtonState(buttonState);
            EventBus.Instance.Publish(new PlayerActionEvent<PlayerAction>(typeof(HumanLLMPlayer), action));
        }

        #endregion

        #region Event Handlers

        private void OnNewGame(NewGameEvent<GameManager> newGameEvent)
        {
            SetupInitialUIState();
            ShowMessage(newGameEvent.Message);
            ResetAllCardViews();
            EnablePlayerActions();
            UpdateUI();
        }

        private void OnNewRound(NewRoundEvent<GameManager> newRoundEvent)
        {
            SetupInitialUIState();
            ResetAllCardViews();
            EnablePlayerActions();
            UpdateUI();
        }


        private void OnUpdateGameState(UpdateGameStateEvent<GameManager> e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateCoinsDisplay();
            UpdatePotDisplay(ScoreManager.Pot);
            UpdateCurrentBetDisplay(ScoreManager.CurrentBet);
            EnablePlayerActions();
        }


        private void OnPlayerStartCountDown(PlayerStartCountDownEvent<TurnManager> e)
        {
            CurrentLLMPlayer = e.TurnManager.CurrentLLMPlayer;


            if (CurrentPlayerUI != null)
            {
                CurrentPlayerUI.StartTimer(e.TurnManager);
            }
            else
            {
                Debug.Log(" CurrentPlayerUI not found !!!");
            }
        }

        private void OnUpdateTurnState(UpdateTurnStateEvent<LLMPlayer> e)
        {
            CurrentLLMPlayer = e.CurrentLLMPlayer;

            if (CurrentPlayerUI == null || CurrentPlayerUI.PlayerIndex != CurrentLLMPlayer.PlayerIndex)
            {
                if (MainTableUI.Instance.TryGetPlayerUI(e.CurrentLLMPlayer.PlayerIndex, out PlayerUI playerUI))
                {
                    CurrentPlayerUI = playerUI;
                }
            }
            // todo only update local player 

            if (e.IsHumanTurn)
            {
                SetCardHighlights(LocalPlayerCardViews, true);
                SetButtonState(ButtonState.TakeAction);
            }
            else if (e.IsComputerTurn && CurrentLLMPlayer is ComputerLLMPlayer computerPlayer)
            {
                SetCardHighlights(LocalPlayerCardViews, false);
                computerPlayer.ResetState();
            }

            EnablePlayerActions();
        }

        private void OnPlayerStopCountDown(PlayerStopCountDownEvent<LLMPlayer> e)
        {
            CurrentLLMPlayer = e.CurrentLLMPlayer;

            if (CurrentPlayerUI == null || CurrentPlayerUI.PlayerIndex != CurrentLLMPlayer.PlayerIndex)
            {
                if (MainTableUI.Instance.TryGetPlayerUI(e.CurrentLLMPlayer.PlayerIndex, out PlayerUI playerUI))
                {
                    CurrentPlayerUI = playerUI;
                }
            }

            if (CurrentPlayerUI != null)
            {
                CurrentPlayerUI.StopTimer(CurrentLLMPlayer);
                SetButtonState(ButtonState.TakeAction);
            }
            else
            {
                Debug.Log(" CurrentPlayerUI not found !!!");
            }
        }

        private void OnUpdateWildCards(UpdateWildCardsEvent<GameMode, Card> e)
        {
            void Update(CardView cardView, string cardViewName, bool useCondition)
            {
                if (useCondition)
                {
                    if (e.WildCards.TryGetValue(cardViewName, out Card card))
                    {
                        UpdateCardView(cardView, card);
                    }
                }
                else
                {
                    cardView.SetActive();
                }
            }

            Update(TrumpCardView, "TrumpCard", e.GameMode.UseTrump);
            Update(MagicCard0, nameof(MagicCard0), e.GameMode.UseMagicCards);
            Update(MagicCard1, nameof(MagicCard1), e.GameMode.UseMagicCards);
            Update(MagicCard2, nameof(MagicCard2), e.GameMode.UseMagicCards);
            Update(MagicCard3, nameof(MagicCard3), e.GameMode.UseMagicCards);
        }

        private void OnUpdateWildCardsHighlight(UpdateWildCardsHighlightEvent<Card> e)
        {
            TrumpCardView.SetHighlight(e.WildCardsInHand.TryGetValue("TrumpCard", out Card _));
            MagicCard0.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard0), out Card _));
            MagicCard1.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard1), out Card _));
            MagicCard2.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard2), out Card _));
            MagicCard3.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard3), out Card _));
        }


        private void OnUpdateFloorCard(UpdateFloorCardEvent<Card> e)
        {
            UpdateCardView(FloorCardView, e.Card);
            FloorCardView.SetActive(FloorCardView.Card != null);
        }

        private void OnUpdateFloorCardList(UpdateFloorCardListEvent<Card> e)
        {
            LeftPanelController.AddCard(e.Card, e.Reset);
        }

        // todo only update current local players hand
        private void OnUpdatePlayerHandDisplay(UpdatePlayerHandDisplayEvent<LLMPlayer> e)
        {
            switch (e.Player)
            {
                case HumanLLMPlayer humanPlayer:
                    UpdateHumanPlayerHandDisplay(humanPlayer, e.IsRoundEnd);
                    break;
            }
        }

        private void OnUpdateRoundDisplay(UpdateRoundDisplayEvent<ScoreManager> e)
        {
            // todo
            // UpdateWinDisplay(e.ScoreManager.HumanTotalWins, e.ScoreManager.ComputerTotalWins);
        }

        private void OnOfferContinuation(OfferContinuationEvent e)
        {
            EnablePlayerActions();
        }

        private void OnOfferNewGame(OfferNewGameEvent e)
        {
            EnablePlayerActions();
        }

        private void OnMessage(UIMessageEvent e)
        {
            ShowMessage(e.Message, e.Delay);
        }

        #endregion

        #region Helper Methods

        private void EnablePlayerActions()
        {
            ButtonState buttonState = ButtonState;

            bool isLocalPlayerTurn = !(CurrentLLMPlayer is ComputerLLMPlayer);


            bool hasSeenHand = false;
            int coins = 0;

            if (isLocalPlayerTurn)
            {
                HumanLLMPlayer humanLLMPlayer = (HumanLLMPlayer)CurrentLLMPlayer;
                if (humanLLMPlayer != null)
                {
                    hasSeenHand = CurrentLLMPlayer.HasSeenHand;
                    coins = CurrentLLMPlayer.Coins;
                }
            }


            bool showState = buttonState is ButtonState.TakeAction or ButtonState.DrawnFromDeck or ButtonState.Draw;


            SetButtonState(PurchaseCoins, isLocalPlayerTurn && coins <= 100 && showState);
            SetButtonState(PlayBlind, isLocalPlayerTurn && !hasSeenHand && showState);
            SetButtonState(RaiseBet, isLocalPlayerTurn && hasSeenHand && showState);
            SetButtonState(Fold, isLocalPlayerTurn && hasSeenHand && showState);
            SetButtonState(Bet, isLocalPlayerTurn && hasSeenHand && showState);
            SetButtonState(ShowCall, isLocalPlayerTurn && hasSeenHand && showState);

            if (DrawFromDeck != null)
            {
                DrawFromDeck.SetInteractable(isLocalPlayerTurn && hasSeenHand &&
                                             buttonState != ButtonState.ActionTaken && buttonState != ButtonState.Draw &&
                                             buttonState != ButtonState.DrawnFromDeck);
            }
        }

        private void SetButtonState(Button3D button, bool state)
        {
            if (button != null)
            {
                button.gameObject.SetActive(state);
                button.SetInteractable(state);
            }
        }

        public void SetButtonState(ButtonState buttonState)
        {
            ButtonState = buttonState;
            EnablePlayerActions();
        }

        private void SetCardHighlights(CardView[] cardViews, bool e)
        {
            foreach (var cardView in cardViews)
            {
                cardView.SetHighlight(e);
            }
        }

        private void UpdateCoinsDisplay()
        {
            List<LLMPlayer> allPlayers = PlayerManager.GetAllPlayers();
            foreach (LLMPlayer player in allPlayers)
            {
                foreach (PlayerUI playerUI in MainTableUI.Instance.AllPlayerUI)
                {
                    if (player.PlayerIndex == playerUI.PlayerIndex)
                    {
                        playerUI.UpdatePlayerCoins(player);
                    }
                }
            }
        }

        private void UpdatePotDisplay(int potAmount)
        {
            if (Pot != null)
            {
                Pot.text = $"{potAmount}";
            }
        }

        private void UpdateCurrentBetDisplay(int currentBet)
        {
            if (CurrentBet != null)
            {
                CurrentBet.text = $"Current Bet: {currentBet}";
            }
        }


        private void UpdateCardView(CardView cardView, Card card)
        {
            if (cardView != null)
            {
                cardView.SetCard(card);
                cardView.UpdateCardView();
            }
        }


        private void UpdateHumanPlayerHandDisplay(LLMPlayer llmPlayer, bool isRoundEnd)
        {
            if (llmPlayer.HasSeenHand || isRoundEnd)
            {
                for (int i = 0; i < llmPlayer.Hand.Count(); i++)
                {
                    LocalPlayerCardViews[i].SetCard(llmPlayer.Hand.GetCard(i));
                    LocalPlayerCardViews[i].UpdateCardView();
                }
            }
        }


        private void ResetAllCardViews()
        {
            foreach (CardView cardView in LocalPlayerCardViews)
            {
                cardView.ResetCardView();
            }

            LeftPanelController.ResetView();
            ShowPlayerHand.SetInteractable(true);
        }

        private void ShowMessage(string message, float delay = 5f)
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(true);
                if (Message != null)
                {
                    Message.text = message;
                }

                // StartCoroutine(HideMessageAfterDelay(delay));
            }
        }

        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideMessage();
        }

        private void HideMessage()
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
                if (Message != null)
                {
                    Message.text = "";
                }
            }
        }

        #endregion
    }
}