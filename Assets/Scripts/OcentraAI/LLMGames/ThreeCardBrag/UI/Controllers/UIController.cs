using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers
{
    public class UIController : MonoBehaviour
    {

        [Required, ShowInInspector]
        private Button ShowPlayerHand { get; set; }
        [Required, ShowInInspector]
        private Button PlayBlind { get; set; }
        [Required, ShowInInspector]
        private Button RaiseBet { get; set; }

        [Required, ShowInInspector]
        private Button Fold { get; set; }

        [Required, ShowInInspector]
        private Button Bet { get; set; }

        [Required, ShowInInspector]
        private Button DrawFromDeck { get; set; }

        [Required, ShowInInspector]
        private Button ShowCall { get; set; }

        [Required, ShowInInspector]
        private Button ContinueRound { get; set; }

        [Required, ShowInInspector]
        private Button NewGame { get; set; }
        [Required, ShowInInspector]
        private Button PurchaseCoins { get; set; }

        [Required, ShowInInspector]
        private Transform ComputerHand { get; set; }

        [Required, ShowInInspector]
        private Transform MessageHolder { get; set; }


        [Required, ShowInInspector]
        private TMP_InputField RaiseAmount { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI Message { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI HumanPlayersCoins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI ComputerPlayerCoins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI HumanPlayersWins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI ComputerPlayerWins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI Pot { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI CurrentBet { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI ComputerPlayingBlind { get; set; }

        [Required, ShowInInspector]
        private PlayerTimer HumanPlayerTimer { get; set; }

        [Required, ShowInInspector]
        private PlayerTimer ComputerPlayerTimer { get; set; }

        [ShowInInspector]
        public PlayerTimer CurrentPlayerTimer { get; set; }



        [Required, ShowInInspector]
        private CardView FloorCardView { get; set; }

        [Required, ShowInInspector]
        private CardView[] HumanPlayerCardViews { get; set; }

        [Required, ShowInInspector]
        private CardView[] ComputerPlayerCardViews { get; set; }


        [Required, ShowInInspector]
        public LeftPanelController LeftPanelController { get; set; }

        void OnValidate()
        {
            Init();
        }

        private void Init()
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

            HumanPlayerCardViews = ShowPlayerHand.GetComponentsInChildren<CardView>();
            ComputerPlayerCardViews = ComputerHand.GetComponentsInChildren<CardView>();

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
            if (NewGame != null) NewGame.gameObject.SetActive(false);
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(false);
            if (MessageHolder != null) MessageHolder.gameObject.SetActive(false);

            LeftPanelController = FindObjectOfType<LeftPanelController>();
        }


        private void OnEnable()
        {

            EventBus.Subscribe<InitializeUIPlayers>(OnInitializeUIPlayers);

            EventBus.Subscribe<NewGameEventArgs>(OnNewGame);
            EventBus.Subscribe<UpdateFloorCard>(OnUpdateFloorCard);

            EventBus.Subscribe<UpdateGameState>(OnUpdateGameState);
            EventBus.Subscribe<UIMessage>(OnMessage);
            EventBus.Subscribe<PlayerStartCountDown>(OnPlayerStartCountDown);
            EventBus.Subscribe<PlayerStopCountDown>(OnPlayerStopCountDown);
            EventBus.Subscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Subscribe<UpdateRoundDisplay>(OnUpdateRoundDisplay);
            EventBus.Subscribe<OfferNewGame>(OnOfferNewGame);
            EventBus.Subscribe<UpdateFloorCardList>(OnUpdateFloorCardList);

            EventBus.Subscribe<UpdatePlayerHandDisplay>(OnUpdatePlayerHandDisplay);



        }




        private void OnDisable()
        {
            EventBus.Unsubscribe<InitializeUIPlayers>(OnInitializeUIPlayers);

            EventBus.Unsubscribe<NewGameEventArgs>(OnNewGame);
            EventBus.Unsubscribe<UpdateFloorCard>(OnUpdateFloorCard);

            EventBus.Unsubscribe<UpdateGameState>(OnUpdateGameState);
            EventBus.Unsubscribe<UIMessage>(OnMessage);
            EventBus.Unsubscribe<PlayerStartCountDown>(OnPlayerStartCountDown);
            EventBus.Unsubscribe<PlayerStopCountDown>(OnPlayerStopCountDown);
            EventBus.Unsubscribe<OfferContinuation>(OnOfferContinuation);
            EventBus.Unsubscribe<UpdateRoundDisplay>(OnUpdateRoundDisplay);
            EventBus.Unsubscribe<OfferNewGame>(OnOfferNewGame);
            EventBus.Unsubscribe<UpdateFloorCardList>(OnUpdateFloorCardList);


            EventBus.Unsubscribe<UpdatePlayerHandDisplay>(OnUpdatePlayerHandDisplay);


        }



        private void OnUpdateFloorCardList(UpdateFloorCardList e)
        {
            LeftPanelController.AddCard(e.Card);
        }

        private void OnUpdateFloorCard(UpdateFloorCard e)
        {
            UpdateFloorCard(e.Card);
        }



        public void OnInitializeUIPlayers(InitializeUIPlayers e)
        {
            HumanPlayerTimer.SetPlayer(e.GameManager.HumanPlayer);
            HumanPlayerTimer.Show(false);

            ComputerPlayerTimer.SetPlayer(e.GameManager.ComputerPlayer);
            ComputerPlayerTimer.Show(false);

            FloorCardView.gameObject.SetActive(false);
            FloorCardView.transform.parent.gameObject.SetActive(false);


            e.CompletionSource.TrySetResult(true);
        }

        private void OnOfferNewGame(OfferNewGame obj)
        {
            ShowMessage($"Do you want to play a new game?", 15f);
            if (NewGame != null) NewGame.gameObject.SetActive(true);
        }



        private void OnUpdateRoundDisplay(UpdateRoundDisplay e)
        {
            ComputerPlayerWins.text = $"{e.ScoreKeeper.ComputerTotalWins}";
            HumanPlayersWins.text = $"{e.ScoreKeeper.HumanTotalWins}";
        }

        private void OnOfferContinuation(OfferContinuation e)
        {

            if (ContinueRound != null) ContinueRound.gameObject.SetActive(true);

        }



        private void OnMessage(UIMessage e)
        {
            ShowMessage(e.Message, e.Delay);
        }
        private void OnPlayerStartCountDown(PlayerStartCountDown e)
        {
            EnablePlayerActions(e.TurnInfo.CurrentPlayer);


            if (e.TurnInfo.CurrentPlayer is HumanPlayer)
            {
                CurrentPlayerTimer = HumanPlayerTimer;
            }
            else if (e.TurnInfo.CurrentPlayer is ComputerPlayer)
            {
                CurrentPlayerTimer = ComputerPlayerTimer;
            }

            CurrentPlayerTimer.StartTimer(e.TurnInfo);


        }

        private void OnPlayerStopCountDown(PlayerStopCountDown e)
        {
            if (e.CurrentPlayer is HumanPlayer)
            {
                CurrentPlayerTimer = HumanPlayerTimer;
            }
            else if (e.CurrentPlayer is ComputerPlayer)
            {
                CurrentPlayerTimer = ComputerPlayerTimer;
            }
            CurrentPlayerTimer.StopTimer();

        }
        private void OnNewGame(NewGameEventArgs e)
        {
            ShowMessage("New game started with initial coins: " + e.InitialCoins, 5f);
            UpdateCoinsDisplay(e.InitialCoins);
        }




        private void Start()
        {
            Init();

            if (ShowPlayerHand != null) ShowPlayerHand.onClick.AddListener(OnPlayerSeeHand);
            if (PlayBlind != null) PlayBlind.onClick.AddListener(OnPlayBlind);
            if (RaiseBet != null) RaiseBet.onClick.AddListener(OnRaiseBet);
            if (Fold != null) Fold.onClick.AddListener(OnFold);
            if (Bet != null) Bet.onClick.AddListener(OnBet);
            if (DrawFromDeck != null)
            {
                DrawFromDeck.interactable = false;

                DrawFromDeck.onClick.AddListener(OnDrawFromDeck);
            }
            if (ShowCall != null) ShowCall.onClick.AddListener(OnShowCall);

            if (ContinueRound != null) ContinueRound.onClick.AddListener(() =>
            {
                EventBus.Publish(new PlayerActionContinueGame(true));
            });

            if (NewGame != null) NewGame.onClick.AddListener(() =>
            {
                EventBus.Publish(new PlayerActionStartNewGame());

            });

            if (PurchaseCoins != null)
            {
                PurchaseCoins.onClick.AddListener(() =>
                {
                    EventBus.Publish(new PurchaseCoins(1000));

                });
            }
        }



        private void OnShowCall()
        {
            TakeActionAsync(PlayerAction.Show);
        }

        private void OnDrawFromDeck()
        {
            TakeActionAsync(PlayerAction.DrawFromDeck);
        }

        private void OnFold()
        {
            TakeActionAsync(PlayerAction.Fold);
        }

        private void OnPlayBlind()
        {
            TakeActionAsync(PlayerAction.PlayBlind);
        }


        public void OnPickFromFloor()
        {
            ShowMessage($"Drop the Card to Hand Card to discard, else Draw new card", 5f);
        }

        private void OnPlayerSeeHand()
        {
            ShowPlayerHand.enabled = false;
            TakeActionAsync(PlayerAction.SeeHand);
        }

        private void OnBet()
        {
            TakeActionAsync(PlayerAction.Bet);
        }

        private void OnRaiseBet()
        {
            EventBus.Publish(new PlayerActionRaiseBet(typeof(HumanPlayer), RaiseAmount.text));
        }



        public void TakeActionAsync(PlayerAction action)
        {
            EventBus.Publish(new PlayerActionEvent(typeof(HumanPlayer), action));

        }


        public void OnUpdateGameState(UpdateGameState e)
        {
            UpdateCoinsDisplay(e.GameManager.HumanPlayer.Coins);
            UpdatePotDisplay(e.GameManager.Pot);
            UpdateCurrentBetDisplay(e.GameManager.CurrentBet);

            if (e.GameManager.CurrentTurn is { CurrentPlayer: not null })
            {
                EnablePlayerActions(e.GameManager.CurrentTurn.CurrentPlayer);

            }

            if (e.IsNewRound)
            {
                ResetAllCardViews();
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

        public void EnablePlayerActions(Player currentPlayer)
        {
            bool humanPlayerHasSeenHand = false;
            bool isCurrentPlayerHuman = currentPlayer is HumanPlayer;
            int humanPlayerCoins = 0;

            if (currentPlayer is HumanPlayer humanPlayer)
            {
                humanPlayerHasSeenHand = humanPlayer.HasSeenHand;
                humanPlayerCoins = humanPlayer.Coins;
            }

            if (PurchaseCoins != null)
            {
                PurchaseCoins.gameObject.SetActive(humanPlayerCoins <= 100 && isCurrentPlayerHuman);
            }

            if (PlayBlind != null)
            {
                PlayBlind.gameObject.SetActive(!humanPlayerHasSeenHand && isCurrentPlayerHuman);
            }

            if (RaiseBet != null)
            {
                RaiseBet.gameObject.transform.parent.gameObject.SetActive(humanPlayerHasSeenHand && isCurrentPlayerHuman);
            }
            if (Fold != null)
            {
                Fold.gameObject.SetActive(humanPlayerHasSeenHand && isCurrentPlayerHuman);

            }

            if (Bet != null)
            {
                Bet.gameObject.SetActive(humanPlayerHasSeenHand && isCurrentPlayerHuman);
            }


            if (DrawFromDeck != null)
            {
                DrawFromDeck.interactable = humanPlayerHasSeenHand && isCurrentPlayerHuman;
            }

            if (ShowCall != null)
            {
                ShowCall.gameObject.SetActive(humanPlayerHasSeenHand && isCurrentPlayerHuman);
            }
        }

        public void UpdateCoinsDisplay(int coins)
        {
            if (HumanPlayersCoins != null) HumanPlayersCoins.text = $"{coins}";
            if (ComputerPlayerCoins != null) ComputerPlayerCoins.text = $"{coins}";
        }

        public void UpdatePotDisplay(int potAmount)
        {
            if (Pot != null) Pot.text = $"{potAmount}";
        }

        public void UpdateCurrentBetDisplay(int currentBet)
        {
            if (CurrentBet != null) CurrentBet.text = $"Current Bet: {currentBet} ";
        }



        public void UpdateFloorCard(Card floorCard = null)
        {
            if (FloorCardView != null)
            {
                if (floorCard != null)
                {
                    FloorCardView.SetCard(floorCard);
                    FloorCardView.UpdateCardView();
                }

                FloorCardView.SetActive(floorCard != null);
                FloorCardView.transform.parent.gameObject.SetActive(floorCard != null);

            }
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

        public void UpdateHumanPlayerHandDisplay(Player player, bool isRoundEnd = false)
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


        public void UpdateComputerHandDisplay(Player player, bool isRoundEnd = false)
        {
            ComputerPlayingBlind.text = player.HasSeenHand ? "" : $" Playing Blind ";

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

        public async void ShowMessage(string message, float delay = 5f)
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(true);
                if (Message != null)
                {
                    Message.text = message;
                }

              //  await HideMessageAfterDelay(delay);
            }
        }

        private async Task HideMessageAfterDelay(float delay)
        {
            await Utility.Delay(delay);

            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
            }
        }
    }

}
