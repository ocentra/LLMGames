using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers
{
    public class UIController : MonoBehaviour
    {
        [ShowInInspector]
        private DeckManager DeckManager { get; set; }

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

        [Required, ShowInInspector]
        public PlayerTimer CurrentPlayerTimer { get; set; }

        [Required, ShowInInspector]
        private CardView[] HumanPlayerCardViews { get; set; }

        [Required, ShowInInspector]
        private CardView FloorCardView { get; set; }

        [Required, ShowInInspector]
        private CardView[] ComputerPlayerCardViews { get; set; }

        public TaskCompletionSource<bool> ActionCompletionSource { get; private set; }

        [Required, ShowInInspector]
        public LeftPanelController LeftPanelController { get; set; }

        void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            if (GameManager.Instance != null)
            {
                DeckManager = GameManager.Instance.DeckManager;
            }

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

        private void Start()
        {
            Init();

            if (ShowPlayerHand != null) ShowPlayerHand.onClick.AddListener(async () => await OnShowPlayerHand());
            if (PlayBlind != null) PlayBlind.onClick.AddListener(async () => await OnPlayBlind());
            if (RaiseBet != null) RaiseBet.onClick.AddListener(async () => await OnRaiseBet());
            if (Fold != null) Fold.onClick.AddListener(async () => await OnFold());
            if (Bet != null) Bet.onClick.AddListener(async () => await OnBet());
            if (DrawFromDeck != null) DrawFromDeck.onClick.AddListener(async () => await OnDrawFromDeck());
            if (ShowCall != null) ShowCall.onClick.AddListener(async () => await OnShowCall());

            if (ContinueRound != null) ContinueRound.onClick.AddListener(async () => await GameManager.Instance.ContinueGame(true));
            if (NewGame != null) NewGame.onClick.AddListener(async () => await GameManager.Instance.StartNewGameAsync());
            if (PurchaseCoins != null)
            {
                PurchaseCoins.onClick.AddListener(() => GameManager.Instance.PurchaseCoins(GameManager.Instance.HumanPlayer, 1000));
            }
        }



        public Task InitializePlayers()
        {
            HumanPlayerTimer.SetPlayer(GameManager.Instance.HumanPlayer);
            HumanPlayerTimer.Show(false, nameof(Start));

            ComputerPlayerTimer.SetPlayer(GameManager.Instance.ComputerPlayer);
            ComputerPlayerTimer.Show(false, nameof(Start));

            return Task.CompletedTask;
        }

        private async Task OnShowCall()
        {
            await TakeActionAsync(PlayerAction.Show);
        }

        private async Task OnDrawFromDeck()
        {
            await TakeActionAsync(PlayerAction.DrawFromDeck);
        }

        private async Task OnFold()
        {
            await TakeActionAsync(PlayerAction.Fold);
        }

        private async Task OnPlayBlind()
        {
            await TakeActionAsync(PlayerAction.PlayBlind);
        }

        public void OnDiscardCardSet(CardView cardView)
        {
            if (DeckManager != null)
            {
                Card card = cardView.Card;
                DeckManager.SetSwapCard(card);
            }
        }

        public async Task OnPickFromFloor()
        {
            ShowMessage($"Drop the Card to Hand Card to discard, else Draw new card", 5f);
            await WaitForSwapCardIndexAsync();
        }

        private async Task OnShowPlayerHand()
        {
            ShowPlayerHand.enabled = false;
            await TakeActionAsync(PlayerAction.SeeHand);
        }

        private async Task OnBet()
        {
            await TakeActionAsync(PlayerAction.Bet);
        }

        private async Task OnRaiseBet()
        {
            if (string.IsNullOrEmpty(RaiseAmount.text))
            {
                ShowMessage($" Please Set RaiseAmount! Needs to be higher than CurrentBet {GameManager.Instance.CurrentBet}", 5f);
                return;
            }

            if (int.TryParse(RaiseAmount.text, out int raiseAmount) && raiseAmount > GameManager.Instance.CurrentBet)
            {
                GameManager.Instance.SetCurrentBet(raiseAmount);
                await TakeActionAsync(PlayerAction.Raise);
            }
            else
            {
                ShowMessage($" RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {GameManager.Instance.CurrentBet}", 5f);
            }
        }

        private async Task WaitForSwapCardIndexAsync()
        {
            if (DeckManager != null)
            {
                await Task.Run(async () =>
                {
                    while (DeckManager.SwapCard == null)
                    {
                        await Task.Delay(100); // Check every 100ms
                    }
                });
                await TakeActionAsync(PlayerAction.PickAndSwap);
            }
            else
            {
                Debug.LogError($"DeckManager is null!");
            }
        }

        public void SetComputerSeenHand(bool hasSeenHand)
        {
            string message = GameManager.Instance.BlindMultiplier > 1 ? $" Playing Blind {GameManager.Instance.BlindMultiplier}" : $" Playing Blind ";
            ComputerPlayingBlind.text = hasSeenHand ? "" : message;
        }

        private async Task TakeActionAsync(PlayerAction action)
        {
            await GameManager.Instance.HandlePlayerAction(action);
            ActionCompletionSource?.TrySetResult(true);
        }

        public void UpdateGameState(bool isNewRound = false)
        {
            UpdateCoinsDisplay();
            UpdatePotDisplay();
            UpdateCurrentBetDisplay();
            UpdateHumanPlayerHandDisplay();
            UpdateFloorCard();
            if (isNewRound)
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

        public void EnablePlayerActions()
        {
            bool humanPlayerHasSeenHand = GameManager.Instance.HumanPlayer.HasSeenHand;
            bool isCurrentPlayerHuman = GameManager.Instance.CurrentTurn.CurrentPlayer == GameManager.Instance.HumanPlayer;

            if (PurchaseCoins !=null)
            {
                PurchaseCoins.gameObject.SetActive(GameManager.Instance.HumanPlayer.Coins <=100);
            }

            if (PlayBlind != null)
            {
                PlayBlind.gameObject.SetActive(!humanPlayerHasSeenHand);
            }

            if (RaiseBet != null)
            {
                RaiseBet.gameObject.transform.parent.gameObject.SetActive(humanPlayerHasSeenHand);
            }
            if (Fold != null)
            {
                Fold.gameObject.SetActive(isCurrentPlayerHuman && humanPlayerHasSeenHand);

            }

            if (Bet != null)
            {
                Bet.gameObject.SetActive(isCurrentPlayerHuman && humanPlayerHasSeenHand);
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

        public void UpdateCoinsDisplay()
        {
            if (HumanPlayersCoins != null) HumanPlayersCoins.text = $"{GameManager.Instance.HumanPlayer.Coins}";
            if (ComputerPlayerCoins != null) ComputerPlayerCoins.text = $"{GameManager.Instance.ComputerPlayer.Coins}";
        }

        public void UpdatePotDisplay()
        {
            if (Pot != null) Pot.text = $"{GameManager.Instance.Pot}";
        }

        public void UpdateCurrentBetDisplay()
        {
            if (CurrentBet != null) CurrentBet.text = $"Current Bet: {GameManager.Instance.CurrentBet} ";
        }

        public void UpdateRoundDisplay()
        {
            ComputerPlayerWins.text = $"{GameManager.Instance.ScoreKeeper.ComputerTotalWins}";
            HumanPlayersWins.text = $"{GameManager.Instance.ScoreKeeper.HumanTotalWins}";
        }

        public void UpdateFloorCard()
        {
            if (FloorCardView != null)
            {
                if (DeckManager != null)
                {
                    FloorCardView.SetCard(DeckManager.FloorCard);
                    FloorCardView.UpdateCardView();
                    bool value = FloorCardView.Card != null;
                    FloorCardView.SetActive(value);
                }
                else
                {
                    FloorCardView.SetActive(false);
                }
            }
        }

        public void UpdateFloorCards(Card card)
        {
            LeftPanelController.AddCard(card);
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

                await HideMessageAfterDelay(delay);
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

        public void OfferContinuation(float delay)
        {
            ShowMessage($"Do you want to continue playing one more round?", delay);
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(true);
        }

        public void OfferNewGame()
        {
            ShowMessage($"Do you want to play a new game?", 15f);
            if (NewGame != null) NewGame.gameObject.SetActive(true);
        }

        public async void StartTurnCountdown()
        {
            ActionCompletionSource = new TaskCompletionSource<bool>();
            if (GameManager.Instance.CurrentTurn.CurrentPlayer is HumanPlayer)
            {
                CurrentPlayerTimer = HumanPlayerTimer;
                await HumanPlayerTimer.StartTimer();
            }
            else if (GameManager.Instance.CurrentTurn.CurrentPlayer is ComputerPlayer)
            {
                CurrentPlayerTimer = ComputerPlayerTimer;
                await ComputerPlayerTimer.StartTimer();
            }
        }

        public void StopTurnCountdown()
        {
            HumanPlayerTimer.Show(false, nameof(StopTurnCountdown));
            ComputerPlayerTimer.Show(false, nameof(StopTurnCountdown));
        }

        public void UpdateHumanPlayerHandDisplay(bool isRoundEnd = false)
        {
            if (GameManager.Instance.HumanPlayer.HasSeenHand || isRoundEnd)
            {
                for (int i = 0; i < GameManager.Instance.HumanPlayer.Hand.Count; i++)
                {
                    HumanPlayerCardViews[i].SetCard(GameManager.Instance.HumanPlayer.Hand[i]);
                    HumanPlayerCardViews[i].UpdateCardView();
                }
            }
        }

        public Task WaitForActionAsync()
        {
            ActionCompletionSource = new TaskCompletionSource<bool>();
            return ActionCompletionSource.Task;
        }

        public void ActivateDiscardCard(bool activate)
        {
            // Discard buttons or functionality
        }

        public void UpdateComputerHandDisplay(bool isRoundEnd = false)
        {
            for (int i = 0; i < GameManager.Instance.ComputerPlayer.Hand.Count; i++)
            {
                ComputerPlayerCardViews[i].SetCard(GameManager.Instance.ComputerPlayer.Hand[i]);
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
    }
}
