using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [Required] private PlayerManager PlayerManager => PlayerManager.Instance;
        [Required] private ScoreManager ScoreManager => ScoreManager.Instance;
        [Required] private DeckManager DeckManager => DeckManager.Instance;
        [Required] private TurnManager TurnManager => TurnManager.Instance;
        [Required] private GameManager GameManager => GameManager.Instance;
        #region UI Elements
        [Required, ShowInInspector] private Button ShowPlayerHand { get; set; }
        [Required, ShowInInspector] private Button PlayBlind { get; set; }
        [Required, ShowInInspector] private Button RaiseBet { get; set; }
        [Required, ShowInInspector] private Button Fold { get; set; }
        [Required, ShowInInspector] private Button Bet { get; set; }
        [Required, ShowInInspector] private Button DrawFromDeck { get; set; }
        [Required, ShowInInspector] private Button ShowCall { get; set; }


        [Required, ShowInInspector] private Button PurchaseCoins { get; set; }

        [Required, ShowInInspector] private Transform ComputerHand { get; set; }
        [Required, ShowInInspector] private Transform MessageHolder { get; set; }

        [Required, ShowInInspector] private TMP_InputField RaiseAmount { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI Message { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI HumanPlayersCoins { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI ComputerPlayerCoins { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI HumanPlayersWins { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI ComputerPlayerWins { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI Pot { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI CurrentBet { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI ComputerPlayingBlind { get; set; }

        [Required, ShowInInspector] private PlayerTimer HumanPlayerTimer { get; set; }
        [Required, ShowInInspector] private PlayerTimer ComputerPlayerTimer { get; set; }
        [ShowInInspector] public PlayerTimer CurrentPlayerTimer { get; set; }

        [ShowInInspector] public Player CurrentPlayer { get; set; }

        [Required, ShowInInspector] private CardView FloorCardView { get; set; }

        [Required, ShowInInspector] private CardView TrumpCardView { get; set; }
        [Required, ShowInInspector] private CardView MagicCard0 { get; set; }
        [Required, ShowInInspector] private CardView MagicCard1 { get; set; }
        [Required, ShowInInspector] private CardView MagicCard2 { get; set; }
        [Required, ShowInInspector] private CardView MagicCard3 { get; set; }

        [Required, ShowInInspector] private CardView[] HumanPlayerCardViews { get; set; }
        [Required, ShowInInspector] private CardView[] ComputerPlayerCardViews { get; set; }
        [Required, ShowInInspector] public LeftPanelController LeftPanelController { get; set; }

        [ShowInInspector]
        public ButtonState ButtonState { get; private set; } = ButtonState.TakeAction;

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
            ComputerHand = transform.FindChildRecursively<Transform>(nameof(ComputerHand));
            MessageHolder = transform.FindChildRecursively<Transform>(nameof(MessageHolder));

            ShowPlayerHand = transform.FindChildRecursively<Button>(nameof(ShowPlayerHand));
            PlayBlind = transform.FindChildRecursively<Button>(nameof(PlayBlind));
            RaiseBet = transform.FindChildRecursively<Button>(nameof(RaiseBet));
            Fold = transform.FindChildRecursively<Button>(nameof(Fold));
            Bet = transform.FindChildRecursively<Button>(nameof(Bet));
            DrawFromDeck = transform.FindChildRecursively<Button>(nameof(DrawFromDeck));
            ShowCall = transform.FindChildRecursively<Button>(nameof(ShowCall));
            PurchaseCoins = transform.FindChildRecursively<Button>(nameof(PurchaseCoins));

            SetupCardViews();

            FloorCardView = transform.FindChildRecursively<CardView>(nameof(FloorCardView));

            TrumpCardView = transform.FindChildRecursively<CardView>(nameof(TrumpCardView));
            MagicCard0 = transform.FindChildRecursively<CardView>(nameof(MagicCard0));
            MagicCard1 = transform.FindChildRecursively<CardView>(nameof(MagicCard1));
            MagicCard2 = transform.FindChildRecursively<CardView>(nameof(MagicCard2));
            MagicCard3 = transform.FindChildRecursively<CardView>(nameof(MagicCard3));

            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));
            HumanPlayersCoins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HumanPlayersCoins));
            ComputerPlayerCoins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayerCoins));
            Pot = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Pot));
            CurrentBet = transform.FindChildRecursively<TextMeshProUGUI>(nameof(CurrentBet));
            ComputerPlayingBlind = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayingBlind));

            ComputerPlayerWins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayerWins));
            HumanPlayersWins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HumanPlayersWins));

            HumanPlayerTimer = transform.parent.FindChildRecursively<PlayerTimer>(nameof(HumanPlayerTimer));
            ComputerPlayerTimer = transform.parent.FindChildRecursively<PlayerTimer>(nameof(ComputerPlayerTimer));

            RaiseAmount = transform.FindChildRecursively<TMP_InputField>(nameof(RaiseAmount));

            LeftPanelController = FindObjectOfType<LeftPanelController>();
        }

        private void SetupCardViews()
        {
            HumanPlayerCardViews = ShowPlayerHand.GetComponentsInChildren<CardView>();
            ComputerPlayerCardViews = ComputerHand.GetComponentsInChildren<CardView>();
        }

        private void SetupInitialUIState()
        {

            if (MessageHolder != null) MessageHolder.gameObject.SetActive(false);
            if (ShowPlayerHand != null) ShowPlayerHand.interactable = true;

            if (HumanPlayerCardViews is { Length: > 0 })
            {
                foreach (var cardView in HumanPlayerCardViews)
                {
                    cardView.SetCard(null);
                    cardView.UpdateCardView();
                }
            }


            if (ComputerPlayerCardViews is { Length: > 0 })
            {
                foreach (var cardView in ComputerPlayerCardViews)
                {
                    cardView.SetCard(null);
                    cardView.UpdateCardView();
                }
            }

            ButtonState = ButtonState.TakeAction;


        }

        private void SetupButtonListeners()
        {
            if (ShowPlayerHand != null) ShowPlayerHand.onClick.AddListener(() => OnPlayerSeeHand(ShowPlayerHand));
            if (PlayBlind != null) PlayBlind.onClick.AddListener(() => OnPlayBlind(PlayBlind));
            if (RaiseBet != null) RaiseBet.onClick.AddListener(() => OnRaiseBet(RaiseBet));
            if (Fold != null) Fold.onClick.AddListener(() => OnFold(Fold));
            if (Bet != null) Bet.onClick.AddListener(() => OnBet(Bet));
            if (DrawFromDeck != null) DrawFromDeck.onClick.AddListener(() => OnDrawFromDeck(DrawFromDeck));
            if (ShowCall != null) ShowCall.onClick.AddListener(() => OnShowCall(ShowCall));
            if (PurchaseCoins != null) PurchaseCoins.onClick.AddListener(OnPurchaseCoins);
        }



        #endregion

        #region Event Subscription
        private void SubscribeToEvents()
        {
            EventBus.Subscribe<InitializeUIPlayers>(OnInitializeUIPlayers);
            EventBus.Subscribe<NewGameEventArgs>(OnNewGame);
            EventBus.Subscribe<NewRoundEventArgs>(OnNewRound);
            EventBus.Subscribe<UpdateGameState>(OnUpdateGameState);
            EventBus.Subscribe<PlayerStartCountDown>(OnPlayerStartCountDown);
            EventBus.Subscribe<UpdateTurnState>(OnUpdateTurnState);
            EventBus.Subscribe<PlayerStopCountDown>(OnPlayerStopCountDown);
            EventBus.Subscribe<UpdateFloorCard>(OnUpdateFloorCard);
            EventBus.Subscribe<UpdateWildCards>(OnUpdateWildCards);
            EventBus.Subscribe<UpdateWildCardsHighlight>(OnUpdateWildCardsHighlight);
            EventBus.Subscribe<UpdateFloorCardList>(OnUpdateFloorCardList);
            EventBus.Subscribe<UpdatePlayerHandDisplay>(OnUpdatePlayerHandDisplay);
            EventBus.Subscribe<UpdateRoundDisplay>(OnUpdateRoundDisplay);
            EventBus.Subscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Subscribe<OfferNewGame>(OnOfferNewGame);
            EventBus.Subscribe<UIMessage>(OnMessage);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<InitializeUIPlayers>(OnInitializeUIPlayers);
            EventBus.Unsubscribe<NewGameEventArgs>(OnNewGame);
            EventBus.Unsubscribe<NewRoundEventArgs>(OnNewRound);
            EventBus.Unsubscribe<UpdateGameState>(OnUpdateGameState);
            EventBus.Unsubscribe<PlayerStartCountDown>(OnPlayerStartCountDown);
            EventBus.Unsubscribe<UpdateTurnState>(OnUpdateTurnState);
            EventBus.Unsubscribe<PlayerStopCountDown>(OnPlayerStopCountDown);
            EventBus.Unsubscribe<UpdateFloorCard>(OnUpdateFloorCard);
            EventBus.Unsubscribe<UpdateWildCards>(OnUpdateWildCards);
            EventBus.Unsubscribe<UpdateWildCardsHighlight>(OnUpdateWildCardsHighlight);

            EventBus.Unsubscribe<UpdateFloorCardList>(OnUpdateFloorCardList);
            EventBus.Unsubscribe<UpdatePlayerHandDisplay>(OnUpdatePlayerHandDisplay);
            EventBus.Unsubscribe<UpdateRoundDisplay>(OnUpdateRoundDisplay);
            EventBus.Unsubscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Unsubscribe<OfferNewGame>(OnOfferNewGame);
            EventBus.Unsubscribe<UIMessage>(OnMessage);
        }



        #endregion

        #region Player Actions
        private void OnPlayerSeeHand(Button button)
        {
            button.interactable = false;
            TakeAction(PlayerAction.SeeHand, ButtonState.TakeAction);
        }

        private void OnPlayBlind(Button button)
        {
            button.interactable = false;
            TakeAction(PlayerAction.PlayBlind, ButtonState.ActionTaken);

        }

        private void OnRaiseBet(Button button)
        {

            if (int.TryParse(RaiseAmount.text, out int raiseAmount) && raiseAmount > 0)
            {
                EventBus.Publish(new PlayerActionRaiseBet(typeof(HumanPlayer), raiseAmount.ToString()));
                SetButtonState(ButtonState.ActionTaken);
                button.interactable = false;
            }
            else
            {
                ShowMessage("Please enter a valid raise amount.", 3f);
            }
        }

        private void OnFold(Button button)
        {
            button.interactable = false;
            TakeAction(PlayerAction.Fold, ButtonState.ActionTaken);
        }

        private void OnBet(Button button)
        {
            button.interactable = false;
            TakeAction(PlayerAction.Bet, ButtonState.ActionTaken);
        }

        private void OnDrawFromDeck(Button button)
        {
            button.interactable = false;
            TakeAction(PlayerAction.DrawFromDeck, ButtonState.Draw);
        }

        private void OnShowCall(Button button)
        {
            button.interactable = false;
            TakeAction(PlayerAction.Show, ButtonState.ActionTaken);
        }

        private void OnPurchaseCoins()
        {
            EventBus.Publish(new PurchaseCoins(1000));
            HideMessage();
        }





        private void TakeAction(PlayerAction action, ButtonState buttonState)
        {
            SetButtonState(buttonState);
            EventBus.Publish(new PlayerActionEvent(typeof(HumanPlayer), action));
        }
        #endregion

        #region Event Handlers
        private void OnInitializeUIPlayers(InitializeUIPlayers e)
        {
            HumanPlayerTimer.SetPlayer(PlayerManager.HumanPlayer);
            HumanPlayerTimer.Show(false);

            ComputerPlayerTimer.SetPlayer(PlayerManager.ComputerPlayer);
            ComputerPlayerTimer.Show(false);

            FloorCardView.gameObject.SetActive(false);
            FloorCardView.transform.parent.gameObject.SetActive(false);

            e.CompletionSource.TrySetResult(true);
        }

        private void OnNewGame(NewGameEventArgs e)
        {
            SetupInitialUIState();
            ShowMessage(e.Message);
            ResetAllCardViews();
            EnablePlayerActions();
            UpdateUI();
        }

        private void OnNewRound(NewRoundEventArgs e)
        {
            SetupInitialUIState();
            ResetAllCardViews();
            EnablePlayerActions();
            UpdateUI();
        }

        private void OnUpdateGameState(UpdateGameState e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateCoinsDisplay(PlayerManager.HumanPlayer.Coins, PlayerManager.ComputerPlayer.Coins);
            UpdatePotDisplay(ScoreManager.Pot);
            UpdateCurrentBetDisplay(ScoreManager.CurrentBet);
            EnablePlayerActions();
        }


        private void OnPlayerStartCountDown(PlayerStartCountDown e)
        {
            CurrentPlayer = e.TurnManager.CurrentPlayer;
            CurrentPlayerTimer.StartTimer(e.TurnManager);
        }

        private void OnUpdateTurnState(UpdateTurnState e)
        {
            CurrentPlayer = e.CurrentPlayer;

            if (e.IsHumanTurn)
            {
                CurrentPlayerTimer = HumanPlayerTimer;
                SetCardHighlights(HumanPlayerCardViews, true);
                SetCardHighlights(ComputerPlayerCardViews, false);
                SetButtonState(ButtonState.TakeAction);

            }
            else if (e.IsComputerTurn && CurrentPlayer is ComputerPlayer computerPlayer)
            {
                CurrentPlayerTimer = ComputerPlayerTimer;
                SetCardHighlights(HumanPlayerCardViews, false);
                SetCardHighlights(ComputerPlayerCardViews, true);
                computerPlayer.ResetState();

            }

            EnablePlayerActions();

        }

        private void OnPlayerStopCountDown(PlayerStopCountDown e)
        {
            CurrentPlayer = e.CurrentPlayer;

            CurrentPlayerTimer = (e.CurrentPlayer is HumanPlayer) ? HumanPlayerTimer : ComputerPlayerTimer;
            SetButtonState(ButtonState.TakeAction);
            CurrentPlayerTimer.StopTimer(CurrentPlayer);

        }
        private void OnUpdateWildCards(UpdateWildCards e)
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
                    cardView.Hide();
                }
            }

            Update(TrumpCardView,"TrumpCard", e.GameMode.UseTrump);
            Update(MagicCard0,nameof(MagicCard0), e.GameMode.UseMagicCards);
            Update(MagicCard1,nameof(MagicCard1), e.GameMode.UseMagicCards);
            Update(MagicCard2,nameof(MagicCard2), e.GameMode.UseMagicCards);
            Update(MagicCard3,nameof(MagicCard3), e.GameMode.UseMagicCards);

        }


        private void OnUpdateWildCardsHighlight(UpdateWildCardsHighlight e)
        {
            TrumpCardView.SetHighlight(e.WildCardsInHand.TryGetValue("TrumpCard", out Card _));
            MagicCard0.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard0), out Card _));
            MagicCard1.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard1), out Card _));
            MagicCard2.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard2), out Card _));
            MagicCard3.SetHighlight(e.WildCardsInHand.TryGetValue(nameof(MagicCard3), out Card _));

        }



        private void OnUpdateFloorCard(UpdateFloorCard e)
        {
            UpdateCardView(FloorCardView, e.Card);
            FloorCardView.SetActive(FloorCardView.Card != null);
            FloorCardView.transform.parent.gameObject.SetActive(FloorCardView.Card != null);
        }

        private void OnUpdateFloorCardList(UpdateFloorCardList e)
        {
            LeftPanelController.AddCard(e.Card, e.Reset);
        }

        // todo only update current local players hand
        private void OnUpdatePlayerHandDisplay(UpdatePlayerHandDisplay e)
        {
            switch (e.Player)
            {
                case HumanPlayer humanPlayer:
                    UpdateHumanPlayerHandDisplay(humanPlayer, e.IsRoundEnd);
                    break;
                case ComputerPlayer computerPlayer:
                    UpdateComputerHandDisplay(computerPlayer, e.IsRoundEnd);
                    break;
            }
        }

        private void OnUpdateRoundDisplay(UpdateRoundDisplay e)
        {
            // todo
            // UpdateWinDisplay(e.ScoreManager.HumanTotalWins, e.ScoreManager.ComputerTotalWins);
        }

        private void OnOfferContinuation(OfferContinuation e)
        {
            EnablePlayerActions();

        }

        private void OnOfferNewGame(OfferNewGame e)
        {

            EnablePlayerActions();

        }

        private void OnMessage(UIMessage e)
        {
            ShowMessage(e.Message, e.Delay);
        }
        #endregion

        #region Helper Methods
        private void EnablePlayerActions()
        {
            ButtonState buttonState = ButtonState;

            bool isHumanTurn = !(CurrentPlayer is ComputerPlayer);


            bool hasSeenHand = false;
            int coins = 0;

            if (isHumanTurn)
            {
                HumanPlayer humanPlayer = (HumanPlayer)CurrentPlayer;
                if (humanPlayer != null)
                {
                    hasSeenHand = humanPlayer.HasSeenHand;
                    coins = humanPlayer.Coins;
                }
            }


            bool showState = buttonState is ButtonState.TakeAction or ButtonState.DrawnFromDeck or ButtonState.Draw;



            SetButtonState(PurchaseCoins, isHumanTurn && coins <= 100 && showState);
            SetButtonState(PlayBlind, isHumanTurn && !hasSeenHand && showState);
            SetButtonState(RaiseBet, isHumanTurn && hasSeenHand && showState);
            SetButtonState(Fold, isHumanTurn && hasSeenHand && showState);
            SetButtonState(Bet, isHumanTurn && hasSeenHand && showState);
            SetButtonState(ShowCall, isHumanTurn && hasSeenHand && showState);

            if (DrawFromDeck != null)
            {
                DrawFromDeck.interactable = isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken && buttonState != ButtonState.Draw && buttonState != ButtonState.DrawnFromDeck;
            }

            if (RaiseBet != null)
            {
                RaiseBet.transform.parent.gameObject.SetActive(isHumanTurn && hasSeenHand && showState);
            }
        }

        private void SetButtonState(Button button, bool state)
        {
            if (button != null)
            {
                button.gameObject.SetActive(state);
                button.interactable = state;
            }
        }

        public void SetButtonState(ButtonState buttonState)
        {
            ButtonState = buttonState;
            EnablePlayerActions();
        }

        private void SetCardHighlights(CardView[] cardViews, bool enabled)
        {
            foreach (var cardView in cardViews)
            {
                if (cardView != null && cardView.HighlightImage != null)
                {
                    cardView.HighlightImage.enabled = enabled;
                }
            }
        }

        private void UpdateCoinsDisplay(int humanCoins, int computerCoins)
        {
            if (HumanPlayersCoins != null) HumanPlayersCoins.text = $"{humanCoins}";
            if (ComputerPlayerCoins != null) ComputerPlayerCoins.text = $"{computerCoins}";
        }

        private void UpdatePotDisplay(int potAmount)
        {
            if (Pot != null) Pot.text = $"{potAmount}";
        }

        private void UpdateCurrentBetDisplay(int currentBet)
        {
            if (CurrentBet != null) CurrentBet.text = $"Current Bet: {currentBet}";
        }

        private void UpdateWinDisplay(int humanWins, int computerWins)
        {
            if (HumanPlayersWins != null) HumanPlayersWins.text = $"{humanWins}";
            if (ComputerPlayerWins != null) ComputerPlayerWins.text = $"{computerWins}";
        }

        private void UpdateCardView(CardView cardView, Card card)
        {


            if (cardView != null)
            {

                cardView.SetCard(card);
                cardView.UpdateCardView();
            }
        }



        private void UpdateHumanPlayerHandDisplay(Player player, bool isRoundEnd)
        {
            if (player.HasSeenHand || isRoundEnd)
            {
                for (int i = 0; i < player.Hand.Count(); i++)
                {
                    HumanPlayerCardViews[i].SetCard(player.Hand.GetCard(i));
                    HumanPlayerCardViews[i].UpdateCardView();
                }
            }
        }

        private void UpdateComputerHandDisplay(Player player, bool isRoundEnd)
        {
            ComputerPlayingBlind.text = player.HasSeenHand ? "" : "Playing Blind";

            for (int i = 0; i < player.Hand.Count(); i++)
            {
                ComputerPlayerCardViews[i].SetCard(player.Hand.GetCard(i));
            }

            if (isRoundEnd)
            {
                foreach (CardView cardView in ComputerPlayerCardViews)
                {
                    cardView.UpdateCardView();
                }
            }
            else
            {
                foreach (CardView cardView in ComputerPlayerCardViews)
                {
                    cardView.ShowBackside();
                }
            }
        }

        private void ResetAllCardViews()
        {
            foreach (CardView cardView in HumanPlayerCardViews)
            {
                cardView.ResetCardView();
            }

            foreach (CardView cardView in ComputerPlayerCardViews)
            {
                cardView.ResetCardView();
            }

            LeftPanelController.ResetView();

            ShowPlayerHand.interactable = true;
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
                if (Message != null) Message.text = "";

            }
        }

        #endregion


    }


    public enum ButtonState
    {
        TakeAction,
        ActionTaken,
        Draw,
        DrawnFromDeck
    }
}
