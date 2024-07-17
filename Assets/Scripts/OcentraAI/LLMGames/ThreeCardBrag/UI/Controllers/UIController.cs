using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers
{
    public class UIController : MonoBehaviour
    {

        #region UI Elements
        [Required, ShowInInspector] private Button ShowPlayerHand { get; set; }
        [Required, ShowInInspector] private Button PlayBlind { get; set; }
        [Required, ShowInInspector] private Button RaiseBet { get; set; }
        [Required, ShowInInspector] private Button Fold { get; set; }
        [Required, ShowInInspector] private Button Bet { get; set; }
        [Required, ShowInInspector] private Button DrawFromDeck { get; set; }
        [Required, ShowInInspector] private Button ShowCall { get; set; }
        [Required, ShowInInspector] private Button ContinueRound { get; set; }
        [Required, ShowInInspector] private Button NewGame { get; set; }
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
        [Required, ShowInInspector] private CardView[] HumanPlayerCardViews { get; set; }
        [Required, ShowInInspector] private Image[] HumanPlayerCardHighlight { get; set; }
        [Required, ShowInInspector] private CardView[] ComputerPlayerCardViews { get; set; }
        [Required, ShowInInspector] private Image[] ComputerPlayerCardHighlight { get; set; }
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
            ContinueRound = transform.FindChildRecursively<Button>(nameof(ContinueRound));
            NewGame = transform.FindChildRecursively<Button>(nameof(NewGame));
            PurchaseCoins = transform.FindChildRecursively<Button>(nameof(PurchaseCoins));

            SetupCardViews();

            FloorCardView = transform.FindChildRecursively<CardView>(nameof(FloorCardView));
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
            HumanPlayerCardHighlight = new Image[HumanPlayerCardViews.Length];

            for (int index = 0; index < HumanPlayerCardViews.Length; index++)
            {
                CardView cardView = HumanPlayerCardViews[index];
                Image image = cardView.transform.FindChildRecursively<Image>();
                HumanPlayerCardHighlight[index] = image;
            }

            ComputerPlayerCardViews = ComputerHand.GetComponentsInChildren<CardView>();
            ComputerPlayerCardHighlight = new Image[ComputerPlayerCardViews.Length];

            for (int index = 0; index < ComputerPlayerCardViews.Length; index++)
            {
                CardView cardView = ComputerPlayerCardViews[index];
                Image image = cardView.transform.FindChildRecursively<Image>();
                ComputerPlayerCardHighlight[index] = image;
            }
        }

        private void SetupInitialUIState()
        {
            if (NewGame != null) NewGame.gameObject.SetActive(false);
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(false);
            if (MessageHolder != null) MessageHolder.gameObject.SetActive(false);
        }

        private void SetupButtonListeners()
        {
            if (ShowPlayerHand != null) ShowPlayerHand.onClick.AddListener(OnPlayerSeeHand);
            if (PlayBlind != null) PlayBlind.onClick.AddListener(OnPlayBlind);
            if (RaiseBet != null) RaiseBet.onClick.AddListener(OnRaiseBet);
            if (Fold != null) Fold.onClick.AddListener(OnFold);
            if (Bet != null) Bet.onClick.AddListener(OnBet);
            if (DrawFromDeck != null) DrawFromDeck.onClick.AddListener(OnDrawFromDeck);
            if (ShowCall != null) ShowCall.onClick.AddListener(OnShowCall);
            if (ContinueRound != null) ContinueRound.onClick.AddListener(() => EventBus.Publish(new PlayerActionContinueGame(true)));
            if (NewGame != null) NewGame.onClick.AddListener(() => EventBus.Publish(new PlayerActionStartNewGame()));
            if (PurchaseCoins != null) PurchaseCoins.onClick.AddListener(() => EventBus.Publish(new PurchaseCoins(1000)));
        }
        #endregion

        #region Event Subscription
        private void SubscribeToEvents()
        {
            EventBus.Subscribe<InitializeUIPlayers>(OnInitializeUIPlayers);
            EventBus.Subscribe<NewGameEventArgs>(OnNewGame);
            EventBus.Subscribe<UpdateGameState>(OnUpdateGameState);
            EventBus.Subscribe<PlayerStartCountDown>(OnPlayerStartCountDown);
            EventBus.Subscribe<PlayerStopCountDown>(OnPlayerStopCountDown);
            EventBus.Subscribe<UpdateFloorCard>(OnUpdateFloorCard);
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
            EventBus.Unsubscribe<UpdateGameState>(OnUpdateGameState);
            EventBus.Unsubscribe<PlayerStartCountDown>(OnPlayerStartCountDown);
            EventBus.Unsubscribe<PlayerStopCountDown>(OnPlayerStopCountDown);
            EventBus.Unsubscribe<UpdateFloorCard>(OnUpdateFloorCard);
            EventBus.Unsubscribe<UpdateFloorCardList>(OnUpdateFloorCardList);
            EventBus.Unsubscribe<UpdatePlayerHandDisplay>(OnUpdatePlayerHandDisplay);
            EventBus.Unsubscribe<UpdateRoundDisplay>(OnUpdateRoundDisplay);
            EventBus.Unsubscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Unsubscribe<OfferNewGame>(OnOfferNewGame);
            EventBus.Unsubscribe<UIMessage>(OnMessage);
        }
        #endregion

        #region Player Actions
        private void OnPlayerSeeHand()
        {
            TakeAction(PlayerAction.SeeHand, ButtonState.TakeAction);
        }

        private void OnPlayBlind()
        {
            TakeAction(PlayerAction.PlayBlind, ButtonState.ActionTaken);

        }

        private void OnRaiseBet()
        {
            if (int.TryParse(RaiseAmount.text, out int raiseAmount) && raiseAmount > 0)
            {
                EventBus.Publish(new PlayerActionRaiseBet(typeof(HumanPlayer), raiseAmount.ToString()));
                SetButtonState(ButtonState.ActionTaken); 

            }
            else
            {
                ShowMessage("Please enter a valid raise amount.", 3f);
            }
        }

        private void OnFold()
        {
            TakeAction(PlayerAction.Fold, ButtonState.ActionTaken);

        }

        private void OnBet()
        {
            TakeAction(PlayerAction.Bet, ButtonState.ActionTaken);

        }

        private void OnDrawFromDeck()
        {
            TakeAction(PlayerAction.DrawFromDeck, ButtonState.ActionTaken);

        }

        private void OnShowCall()
        {
            TakeAction(PlayerAction.Show, ButtonState.ActionTaken);

        }

        private void TakeAction(PlayerAction action, ButtonState buttonState)
        {
            EventBus.Publish(new PlayerActionEvent(typeof(HumanPlayer), action));
            SetButtonState(buttonState);
        }
        #endregion

        #region Event Handlers
        private void OnInitializeUIPlayers(InitializeUIPlayers e)
        {
            HumanPlayerTimer.SetPlayer(e.GameManager.HumanPlayer);
            HumanPlayerTimer.Show(false);

            ComputerPlayerTimer.SetPlayer(e.GameManager.ComputerPlayer);
            ComputerPlayerTimer.Show(false);

            FloorCardView.gameObject.SetActive(false);
            FloorCardView.transform.parent.gameObject.SetActive(false);

            e.CompletionSource.TrySetResult(true);
        }

        private void OnNewGame(NewGameEventArgs e)
        {
            ShowMessage($"New game started with initial coins: {e.InitialCoins}", 5f);
            UpdateCoinsDisplay(e.InitialCoins, e.InitialCoins);
        }

        private void OnUpdateGameState(UpdateGameState e)
        {
            UpdateCoinsDisplay(e.GameManager.HumanPlayer.Coins, e.GameManager.ComputerPlayer.Coins);
            UpdatePotDisplay(e.GameManager.Pot);
            UpdateCurrentBetDisplay(e.GameManager.CurrentBet);

            EnablePlayerActions();


            if (e.IsNewRound)
            {
                ResetAllCardViews();
            }
        }

        private void OnPlayerStartCountDown(PlayerStartCountDown e)
        {
            CurrentPlayer = e.TurnInfo.CurrentPlayer;

            if (CurrentPlayer is HumanPlayer)
            {
                CurrentPlayerTimer = HumanPlayerTimer;
                SetCardHighlights(HumanPlayerCardHighlight, true);
                SetCardHighlights(ComputerPlayerCardHighlight, false);
            }
            else if (CurrentPlayer is ComputerPlayer)
            {
                CurrentPlayerTimer = ComputerPlayerTimer;
                SetCardHighlights(HumanPlayerCardHighlight, false);
                SetCardHighlights(ComputerPlayerCardHighlight, true);
            }

            CurrentPlayerTimer.StartTimer(e.TurnInfo);
        }

        private void OnPlayerStopCountDown(PlayerStopCountDown e)
        {
            CurrentPlayer = e.CurrentPlayer;

            CurrentPlayerTimer = (e.CurrentPlayer is HumanPlayer) ? HumanPlayerTimer : ComputerPlayerTimer;

            EnablePlayerActions();

            SetButtonState(ButtonState.ActionTaken);
            SetCardHighlights(ComputerPlayerCardHighlight, false);
            SetCardHighlights(HumanPlayerCardHighlight, false);

            CurrentPlayerTimer.StopTimer();

        }

        private void OnUpdateFloorCard(UpdateFloorCard e)
        {
            UpdateFloorCard(e.Card);
        }

        private void OnUpdateFloorCardList(UpdateFloorCardList e)
        {
            LeftPanelController.AddCard(e.Card);
        }

        private void OnUpdatePlayerHandDisplay(UpdatePlayerHandDisplay e)
        {
            switch (e.Player)
            {
                case HumanPlayer humanPlayer:
                    UpdateHumanPlayerHandDisplay(humanPlayer);
                    break;
                case ComputerPlayer computerPlayer:
                    UpdateComputerHandDisplay(computerPlayer);
                    break;
            }
        }

        private void OnUpdateRoundDisplay(UpdateRoundDisplay e)
        {
            UpdateWinDisplay(e.ScoreKeeper.HumanTotalWins, e.ScoreKeeper.ComputerTotalWins);
        }

        private void OnOfferContinuation(OfferContinuation e)
        {
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(true);
        }

        private void OnOfferNewGame(OfferNewGame obj)
        {
            ShowMessage("Do you want to play a new game?", 15f);
            if (NewGame != null) NewGame.gameObject.SetActive(true);
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

            bool isHumanTurn = CurrentPlayer is HumanPlayer or null;
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


            SetButtonState(PurchaseCoins, isHumanTurn && coins <= 100 && buttonState != ButtonState.ActionTaken);
            SetButtonState(PlayBlind, isHumanTurn && !hasSeenHand && buttonState != ButtonState.ActionTaken);
            SetButtonState(RaiseBet, isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken);
            SetButtonState(Fold, isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken);
            SetButtonState(Bet, isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken);
            SetButtonState(ShowCall, isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken);

            if (DrawFromDeck != null)
            {
                DrawFromDeck.interactable = isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken && buttonState != ButtonState.DrawnFromDeck;
            }

            if (RaiseBet != null)
            {
                RaiseBet.transform.parent.gameObject.SetActive(isHumanTurn && hasSeenHand && buttonState != ButtonState.ActionTaken);
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
        }

        private void SetCardHighlights(Image[] highlights, bool enabled)
        {
            foreach (Image highlight in highlights)
            {
                if (highlight != null)
                {
                    highlight.enabled = enabled;
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

        private void UpdateFloorCard(Card card)
        {
            if (FloorCardView != null)
            {
                if (card != null)
                {
                    FloorCardView.SetCard(card);
                    FloorCardView.UpdateCardView();
                }

                FloorCardView.SetActive(card != null);
                FloorCardView.transform.parent.gameObject.SetActive(card != null);
            }
        }

        private void UpdateHumanPlayerHandDisplay(Player player, bool isRoundEnd = false)
        {
            if (player.HasSeenHand || isRoundEnd)
            {
                for (int i = 0; i < player.Hand.Count; i++)
                {
                    HumanPlayerCardViews[i].SetCard(player.Hand[i]);
                    HumanPlayerCardViews[i].UpdateCardView();
                }
            }
        }

        private void UpdateComputerHandDisplay(Player player, bool isRoundEnd = false)
        {
            ComputerPlayingBlind.text = player.HasSeenHand ? "" : "Playing Blind";

            for (int i = 0; i < player.Hand.Count; i++)
            {
                ComputerPlayerCardViews[i].SetCard(player.Hand[i]);
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

                StartCoroutine(HideMessageAfterDelay(delay));
            }
        }

        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
            }
        }
        #endregion


    }


    public enum ButtonState
    {
        TakeAction,
        ActionTaken,
        DrawnFromDeck
    }
}
