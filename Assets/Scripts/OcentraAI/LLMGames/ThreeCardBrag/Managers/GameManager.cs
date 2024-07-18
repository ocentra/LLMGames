using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.Scores;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static OcentraAI.LLMGames.Utility;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [RequireComponent(typeof(LLMManager))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [ShowInInspector] public HumanPlayer HumanPlayer { get; private set; }
        [ShowInInspector] public ComputerPlayer ComputerPlayer { get; private set; }
        [ShowInInspector, ReadOnly] public ScoreKeeper ScoreKeeper { get; private set; }
        [ShowInInspector] public float TurnDuration { get; private set; } = 60f;
        [ShowInInspector, ReadOnly] public int Pot { get; private set; } = 0;
        [ShowInInspector] public int BaseBet { get; private set; } = 10;
        [ShowInInspector, ReadOnly] public int CurrentBet { get; private set; }
        [ShowInInspector, ReadOnly] public int BlindMultiplier { get; private set; } = 1;
        [ShowInInspector] public int InitialCoins { get; private set; } = 1000;
        [ShowInInspector, ReadOnly] private int CurrentRound { get; set; } = 0;
        [ShowInInspector, ReadOnly] public DeckManager DeckManager { get; private set; }
        [ShowInInspector] public int MaxRounds { get; private set; } = 10;
        [ShowInInspector] public TurnInfo CurrentTurn { get; private set; }
        public AIHelper AIHelper { get; private set; }
        public CancellationTokenSource GlobalCancellationTokenSource { get; set; }


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            GlobalCancellationTokenSource = new CancellationTokenSource();
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            EventBus.Subscribe<PlayerActionStartNewGame>(OnPlayerActionStartNewGame);
            EventBus.Subscribe<PlayerActionContinueGame>(OnPlayerActionContinueGame);

            EventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Subscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Subscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);


            EventBus.Subscribe<PurchaseCoins>(OnPurchaseCoins);

        }

        private void OnDisable()
        {
            GlobalCancellationTokenSource?.Cancel();
            GlobalCancellationTokenSource?.Dispose();
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            EventBus.Unsubscribe<PlayerActionStartNewGame>(OnPlayerActionStartNewGame);
            EventBus.Unsubscribe<PlayerActionContinueGame>(OnPlayerActionContinueGame);

            EventBus.Unsubscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Unsubscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Unsubscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);


            EventBus.Unsubscribe<PurchaseCoins>(OnPurchaseCoins);

            EventBus.Unsubscribe<SetFloorCard>(DeckManager.OnSetFloorCard);

        }

        private async void Start()
        {
            if (IsCancellationRequested()) return;
            await InitializeGameAsync();
        }

        private async void OnPlayerActionStartNewGame(PlayerActionStartNewGame obj)
        {
            if (IsCancellationRequested()) return;

            await StartNewGameAsync();
        }
        private async void OnPlayerActionContinueGame(PlayerActionContinueGame e)
        {
            if (IsCancellationRequested()) return;


            if (e.ShouldContinue)
            {
                await StartNewRoundAsync();
            }
            else
            {
                await StartNewGameAsync();
            }
        }

        private async void OnPlayerAction(PlayerActionEvent e)
        {
            if (IsCancellationRequested()) return;

            if (CurrentTurn.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                await ProcessPlayerAction(e.Action);
            }
        }

        private async void OnPlayerActionRaiseBet(PlayerActionRaiseBet e)
        {
            if (IsCancellationRequested()) return;

            if (CurrentTurn.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                if (string.IsNullOrEmpty(e.Amount))
                {
                    ShowMessage($" Please Set RaiseAmount! Needs to be higher than CurrentBet {CurrentBet}");

                    return;
                }

                if (int.TryParse(e.Amount, out int raiseAmount) && raiseAmount > CurrentBet)
                {
                    int newBet = raiseAmount;
                    if (raiseAmount <= 0) // this will happen from computer atm
                    {
                        float randomMultiplier = Random.Range(0.25f, 3f);
                        // because raise have to be double + if just doubble its normal bet!
                        newBet = (int)(CurrentBet * 2 + CurrentBet * randomMultiplier);
                    }

                    if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
                    {
                        SetCurrentBet(newBet);
                        CurrentTurn.CurrentPlayer.Raise(CurrentBet);
                        Pot += CurrentBet;
                        await SwitchTurn();
                    }
                    else
                    {
                        ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!");
                        await Fold();
                    }
                }
                else
                {
                    ShowMessage($" RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {CurrentBet}");

                }


            }

        }

        private void OnPlayerActionPickAndSwap(PlayerActionPickAndSwap e)
        {
            if (IsCancellationRequested()) return;

            if (e.CurrentPlayerType == CurrentTurn.CurrentPlayer.GetType())
            {
                CurrentTurn.CurrentPlayer.PickAndSwap(e.FloorCard, e.SwapCard);

            }
        }
        private void OnPurchaseCoins(PurchaseCoins obj)
        {
            // This method would interface with the external service to handle coin purchases
            // For now, we'll just add the coins directly

            HumanPlayer.AdjustCoins(obj.Amount);
        }


        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                GlobalCancellationTokenSource.Cancel();
            }
        }

        private async Task InitializeGameAsync()
        {
            Init();
            if (IsCancellationRequested()) return;

            await Task.WhenAll(
                InitializePlayers(),
                InitializeDeck(),
                InitializeUIPlayers()
            );
            await StartNewGameAsync();
        }


        private Task InitializeUIPlayers()
        {
            TaskCompletionSource<bool> initializedUIPlayersSource = new TaskCompletionSource<bool>();

            EventBus.Publish(new InitializeUIPlayers(initializedUIPlayersSource, this));

            return initializedUIPlayersSource.Task;

        }

        private void Init()
        {
            ScoreKeeper = new ScoreKeeper();
            AIHelper = new AIHelper(GameInfo.Instance, this);
        }

        private Task InitializePlayers()
        {
            HumanPlayer = new HumanPlayer();
            HumanPlayer.SetName(nameof(HumanPlayer));

            ComputerPlayer = new ComputerPlayer();
            ComputerPlayer.SetName(nameof(ComputerPlayer));

            return Task.CompletedTask;
        }

        private Task InitializeDeck()
        {
            DeckManager = new DeckManager();
            EventBus.Subscribe<SetFloorCard>(DeckManager.OnSetFloorCard);

            return Task.CompletedTask;
        }

        public async Task StartNewGameAsync()
        {

            CurrentRound = 0;
            HumanPlayer.AdjustCoins(InitialCoins);
            ComputerPlayer.AdjustCoins(InitialCoins);
            DeckManager.ResetForNewGame();
            ScoreKeeper.ResetScores();
            EventBus.Publish(new NewGameEventArgs(this, $"Starting new game"));

            await StartNewRoundAsync();

        }

        private async Task StartNewRoundAsync()
        {

            CurrentRound++;
            DeckManager.Reset();
            HumanPlayer.ResetForNewRound();
            ComputerPlayer.ResetForNewRound();
            Pot = 0;
            CurrentBet = BaseBet;
            BlindMultiplier = 1;


            for (int i = 0; i < 3; i++)
            {
                HumanPlayer.Hand.Add(DeckManager.DrawCard());
                ComputerPlayer.Hand.Add(DeckManager.DrawCard());
            }

            DeckManager.SetRandomTrumpCard();
            DeckManager.SetRandomTrumpCard();

            EventBus.Publish(new UpdateGameState(this, isNewRound: true));

            CurrentTurn = new TurnInfo(TurnDuration);
            CurrentTurn.StartTurn(HumanPlayer);
            await PlayerTurnAsync();
        }

        private async Task PlayerTurnAsync()
        {

            EventBus.Publish(new UpdateGameState(this));

            if (CurrentTurn.CurrentPlayer is HumanPlayer)
            {
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
            else if (CurrentTurn.CurrentPlayer is ComputerPlayer)
            {
                await ComputerPlayer.MakeDecision(CurrentBet);
                await WaitForPlayerActionAndSwitchTurnAsync();
            }

        }




        private async Task WaitForPlayerActionAndSwitchTurnAsync()
        {
            if (IsCancellationRequested()) return;

            using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationTokenSource.Token);


            Task completedTask = await Task.WhenAny(CurrentTurn.ActionCompletionSource.Task, CurrentTurn.TimerCompletionSource.Task);


            if (completedTask == CurrentTurn.ActionCompletionSource.Task)
            {
                CurrentTurn.StopTurn();
            }
            else if (completedTask == CurrentTurn.TimerCompletionSource.Task)
            {
                ShowMessage("Time's up! Placing automatic bet.");
                await ProcessPlayerAction(PlayerAction.Bet);
            }

            cancellationTokenSource.Cancel();
        }




        private async Task ProcessPlayerAction(PlayerAction action)
        {

            string message = $"<color={GetColor(Color.white)}>Player : </color> <color={GetColor(Color.blue)}>{CurrentTurn.CurrentPlayer.PlayerName}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>PlayerAction : </color> <color={GetColor(Color.green)}>{action.ToString()}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>Current bet : </color> <color={GetColor(Color.yellow)}>{CurrentBet}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>Player coins : </color> <color={GetColor(Color.yellow)}>{CurrentTurn.CurrentPlayer.Coins}</color>";


            ShowMessage(message, false);


            if (action is PlayerAction.SeeHand or PlayerAction.DrawFromDeck)
            {

                switch (action)
                {
                    case PlayerAction.SeeHand:
                        CurrentTurn.CurrentPlayer.SeeHand();
                        break;
                    case PlayerAction.DrawFromDeck:
                        CurrentTurn.CurrentPlayer.DrawFromDeck();
                        break;
                }

                EventBus.Publish(new UpdateGameState(this));
                await CurrentTurn.ActionCompletionSource.Task;

            }
            else if (action is PlayerAction.PlayBlind or PlayerAction.Bet  )
            {
                switch (action)
                {
                    case PlayerAction.PlayBlind:
                        await PlayBlind();
                        break;
                    case PlayerAction.Bet:
                        await Bet();
                        break;
                }

                await SwitchTurn();
            }

            else if (action == PlayerAction.Fold)
            {
                await Fold();
            }
            else if (action == PlayerAction.Show)
            {
                await Show();
            }


        }

        private async Task SwitchTurn()
        {
            EventBus.Publish(new UpdateGameState(this));
            await CurrentTurn.SwitchTurn(GlobalCancellationTokenSource);
            CurrentTurn.ActionCompletionSource.TrySetResult(true);
            await PlayerTurnAsync();
        }


        private async Task PlayBlind()
        {
            CurrentBet *= BlindMultiplier;
            if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
            {
                CurrentTurn.CurrentPlayer.BetOnBlind(CurrentBet);
                Pot += CurrentBet;
                BlindMultiplier *= 2;
                CurrentTurn.ActionCompletionSource.TrySetResult(true);

            }
            else
            {
                ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!");
                await Fold();
            }
        }



        private async Task Bet()
        {

            int betAmount = CurrentTurn.CurrentPlayer.HasSeenHand ? CurrentBet * 2 : CurrentBet;
            if (CurrentTurn.CurrentPlayer.Coins >= betAmount)
            {
                CurrentTurn.CurrentPlayer.Bet(CurrentBet);
                Pot += betAmount;
                CurrentTurn.ActionCompletionSource.TrySetResult(true);

            }
            else
            {
                ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!");
                await Fold();
            }
        }



        private async Task Fold()
        {
            CurrentTurn.CurrentPlayer.Fold();
            await EndRound(GetOtherPlayer(CurrentTurn.CurrentPlayer), false);
        }

        private async Task Show()
        {
            HumanPlayer.ShowHand(true);
            ComputerPlayer.ShowHand(true);

            await DetermineWinner();
        }



        private Player GetOtherPlayer(Player currentPlayer)
        {
            return currentPlayer == HumanPlayer ? ComputerPlayer : HumanPlayer;
        }

        private async Task<Player> DetermineWinner()
        {
            int humanValue = HumanPlayer.CalculateHandValue();
            int computerValue = ComputerPlayer.CalculateHandValue();

            Player winner;
            if (humanValue > computerValue)
            {
                winner = HumanPlayer;
            }
            else if (computerValue > humanValue)
            {
                winner = ComputerPlayer;
            }
            else
            {
                int humanHighCard = HumanPlayer.GetHighestCardValue();
                int computerHighCard = ComputerPlayer.GetHighestCardValue();

                if (humanHighCard > computerHighCard)
                {
                    winner = HumanPlayer;
                }
                else if (computerHighCard > humanHighCard)
                {
                    winner = ComputerPlayer;
                }
                else
                {
                    // It's a tie
                    winner = null;
                }
            }

            await EndRound(winner, true);

            return winner;
        }

        private async Task EndRound(Player winner, bool showHand)
        {
            CurrentTurn.StopTurn();

            if (winner == null)
            {
                OfferContinuation(showHand, "It's a tie!");
                await DelayWithCancellation(GlobalCancellationTokenSource, 5000);
                return;
            }

            winner.AdjustCoins(Pot);
            ScoreKeeper.AddToTotalRoundScores(winner, Pot);
            EventBus.Publish(new UpdateRoundDisplay(ScoreKeeper));
            EventBus.Publish(new UpdateGameState(this));

            if (HumanPlayer.Coins <= 0 || ComputerPlayer.Coins <= 0)
            {
                await EndGame();
            }
            else
            {
                await CheckForContinuation(showHand, winner);
            }


        }

        private async void ShowMessage(string message, bool delay = true, float delayTime = 5f)
        {
            EventBus.Publish(new UIMessage(message, delayTime));
            if (delay)
            {
                await DelayWithCancellation(GlobalCancellationTokenSource, (int)delayTime * 1000);

            }

        }

        private async Task CheckForContinuation(bool showHand, Player winner)
        {
            string message = ColouredMessage("Round Over!", Color.red) +
                             ColouredMessage($"{winner.PlayerName} ", Color.white, true) + ColouredMessage($"wins the game!", Color.cyan) +
                             $"{Environment.NewLine}" +
                             ColouredMessage("Continue Next Rounds ?", Color.red, true);

            if (CurrentRound >= MaxRounds)
            {
                Player trailingPlayer = ScoreKeeper.HumanTotalWins < ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
                Player leadingPlayer = GetOtherPlayer(trailingPlayer);

                if (trailingPlayer.Coins > leadingPlayer.Coins)
                {
                    OfferContinuation(showHand, message);
                }
                else
                {
                    await EndGame();
                }
            }
            else
            {
                OfferContinuation(showHand, message);
            }

        }

        private void OfferContinuation(bool showHand, string message)
        {
            CurrentTurn.CurrentPlayer = null;
            EventBus.Publish(new OfferContinuation(10, message));
            HumanPlayer.ShowHand(showHand);
            ComputerPlayer.ShowHand(showHand);

        }


        private Task EndGame()
        {
            Player winner;
            if (HumanPlayer.Coins <= 0)
            {
                winner = ComputerPlayer;
            }
            else if (ComputerPlayer.Coins <= 0)
            {
                winner = HumanPlayer;
            }
            else if (ScoreKeeper.HumanTotalWins != ScoreKeeper.ComputerTotalWins)
            {
                winner = ScoreKeeper.HumanTotalWins > ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
            }
            else
            {
                winner = HumanPlayer.Coins > ComputerPlayer.Coins ? HumanPlayer : ComputerPlayer;
            }

            HumanPlayer.ShowHand(true);
            ComputerPlayer.ShowHand(true);

            string message = ColouredMessage("Game Over!", Color.red) +
                             ColouredMessage($"{winner.PlayerName}", Color.white, true) + ColouredMessage($"wins the game!", Color.cyan) +
                             $"{Environment.NewLine}" +
                             ColouredMessage("Play New Game of 10 rounds ?", Color.red, true);

            CurrentTurn.CurrentPlayer = null;
            EventBus.Publish(new OfferNewGame(60, message));
            return Task.CompletedTask;
        }




        public void SetCurrentBet(int bet)
        {
            CurrentBet = bet;
        }

        private bool IsCancellationRequested()
        {
            return GlobalCancellationTokenSource?.IsCancellationRequested ?? false;
        }
    }


}
