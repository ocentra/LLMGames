using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.ThreeCardBrag.Scores;
using Sirenix.OdinInspector;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [RequireComponent(typeof(LLMManager))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [ShowInInspector]
        public HumanPlayer HumanPlayer { get; private set; }

        [ShowInInspector]
        public ComputerPlayer ComputerPlayer { get; private set; }

        [ShowInInspector]
        public UIController UIController { get; private set; }

        [ShowInInspector, ReadOnly]
        public ScoreKeeper ScoreKeeper { get; private set; }

        public float TurnDuration = 15f;

        [ShowInInspector, ReadOnly]
        public int Pot { get; private set; } = 0;

        [ShowInInspector]
        public int BaseBet { get; private set; } = 10;

        [ShowInInspector, ReadOnly]
        public int CurrentBet { get; private set; }

        [ShowInInspector, ReadOnly]
        public int BlindMultiplier { get; private set; } = 1;

        [ShowInInspector]
        public int InitialCoins { get; private set; } = 1000;

        [ShowInInspector, ReadOnly]
        private int CurrentRound { get; set; } = 0;

        [ShowInInspector, ReadOnly]
        public DeckManager DeckManager { get; private set; }

        [ShowInInspector]
        public int MaxRounds { get; private set; } = 10;

        [ShowInInspector]
        public TurnInfo CurrentTurn { get; private set; }

        public AIHelper AIHelper { get; private set; }

        public GameState CurrentGameState { get; private set; }

        public delegate void GameEventHandler(string eventDescription, GameState newState);
        public event GameEventHandler OnGameEvent;

        private CancellationTokenSource globalCancellationTokenSource;

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
            globalCancellationTokenSource = new CancellationTokenSource();
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            globalCancellationTokenSource.Cancel();
            globalCancellationTokenSource.Dispose();
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                globalCancellationTokenSource.Cancel();
            }
        }

        private async void Start()
        {
            await InitializeGameAsync();
        }

        private async Task InitializeGameAsync()
        {
            RaiseGameEvent("Initializing game", GameState.Initializing);
            Init();
            await Task.WhenAll(
                InitializePlayers(),
                InitializeUI(),
                InitializeDeck(),
                UIController.InitializePlayers()
            );
            await StartNewGameAsync();
        }

        private void Init()
        {
            UIController = FindObjectOfType<UIController>();
            ScoreKeeper = new ScoreKeeper();
            AIHelper = new AIHelper(GameInfo.Instance, this);
        }

        private Task InitializePlayers()
        {
            HumanPlayer = new HumanPlayer();
            HumanPlayer.SetName(nameof(HumanPlayer));
            HumanPlayer.OnActionTaken += async (action) => await HandlePlayerAction(action);

            ComputerPlayer = new ComputerPlayer();
            ComputerPlayer.SetName(nameof(ComputerPlayer));
            ComputerPlayer.OnActionTaken += async (action) => await HandlePlayerAction(action);

            HumanPlayer.OnCoinsChanged += () => UIController.UpdateCoinsDisplay();
            ComputerPlayer.OnCoinsChanged += () => UIController.UpdateCoinsDisplay();

            return Task.CompletedTask;
        }

        private void RaiseGameEvent(string eventDescription, GameState newState, bool log = false)
        {
            CurrentGameState = newState;
            if (log)
            {
                Debug.Log($"Game Event: {eventDescription} Game State changed to: {newState}");
            }
            OnGameEvent?.Invoke(eventDescription, newState);
        }

        private Task InitializeUI()
        {
            UIController = FindObjectOfType<UIController>();
            return Task.CompletedTask;
        }

        private Task InitializeDeck()
        {
            DeckManager = new DeckManager();
            return Task.CompletedTask;
        }

        public async Task StartNewGameAsync()
        {
            RaiseGameEvent("Starting a new game", GameState.StartNewGame);

            CurrentRound = 0;
            HumanPlayer.AdjustCoins(InitialCoins);
            ComputerPlayer.AdjustCoins(InitialCoins);
            await StartNewRoundAsync();
            DeckManager.ResetForNewGame();
        }

        private async Task StartNewRoundAsync()
        {
            RaiseGameEvent("Starting a New Round", GameState.StartNewRound);

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

            UpdateGameState();
            await PlayerTurnAsync(HumanPlayer);
        }

        private async Task PlayerTurnAsync(Player currentPlayer)
        {
            CurrentTurn = new TurnInfo(currentPlayer);
          //  Debug.Log($"PlayerTurnAsync started for: {currentPlayer.PlayerName}");

            RaiseGameEvent($"{CurrentTurn.CurrentPlayer.PlayerName}'s turn started", GameState.WaitingForPlayerAction);

            UIController.StartTurnCountdown();
            UIController.EnablePlayerActions();

            if (CurrentTurn.CurrentPlayer is HumanPlayer)
            {
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
            else
            {
                await ComputerTurnAsync();
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
        }

        private async Task WaitForPlayerActionAndSwitchTurnAsync()
        {
            using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(globalCancellationTokenSource.Token))
            {
                Task playerActionTask = UIController.WaitForActionAsync();
                Task<bool> timerTask = UIController.CurrentPlayerTimer.WaitForCompletion();

                Task completedTask = await Task.WhenAny(playerActionTask, timerTask);
                if (completedTask == playerActionTask)
                {
                    UIController.CurrentPlayerTimer.StopTimer();
                    SwitchTurn();

                    cts.Cancel();
                }
                else
                {
                    if (await timerTask)
                    {
                        UIController.ShowMessage("Time's up! Turn is passed to the opponent.", 5f);
                    }
                    UIController.CurrentPlayerTimer.StopTimer();
                    SwitchTurn();
                }
            }
        }

        private async Task ComputerTurnAsync()
        {
            ComputerPlayer.MakeDecision(CurrentBet);
            await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f)), globalCancellationTokenSource.Token);
        }

        public async Task HandlePlayerAction(PlayerAction action)
        {
            Debug.Log($"HandlePlayerAction: {action} by {CurrentTurn.CurrentPlayer.PlayerName}");
            await ProcessPlayerAction(action);
            UpdateGameState();

            if (action != PlayerAction.Fold && action != PlayerAction.Show)
            {
                Debug.Log($"Switching turn to: {CurrentTurn.CurrentPlayer.PlayerName}");
            }
        }

        private async Task ProcessPlayerAction(PlayerAction action)
        {
            RaiseGameEvent($"{CurrentTurn.CurrentPlayer.PlayerName} performing action: {action}", GameState.ProcessingAction);
            switch (action)
            {
                case PlayerAction.SeeHand:
                    CurrentTurn.CurrentPlayer.SeeHand();
                    UIController.EnablePlayerActions();
                    UIController.UpdateHumanPlayerHandDisplay();
                    await WaitForTurnCompletion();
                    break;
                case PlayerAction.PlayBlind:
                    PlayBlind();
                    break;
                case PlayerAction.Bet:
                    Bet();
                    break;
                case PlayerAction.Raise:
                    Raise();
                    break;
                case PlayerAction.Fold:
                    Fold();
                    break;
                case PlayerAction.DrawFromDeck:
                    CurrentTurn.CurrentPlayer.DrawFromDeck();
                    await WaitForTurnCompletion();
                    break;
                case PlayerAction.PickAndSwap:
                    CurrentTurn.CurrentPlayer.PickAndSwap();
                    break;
                case PlayerAction.Show:
                    Show();
                    break;
            }

            RaiseGameEvent($"Finished processing action: {action}", GameState.WaitingForPlayerAction);
        }

        private void UpdateGameState()
        {
            UIController.UpdateGameState();
        }

        public async Task WaitForTurnCompletion()
        {
            await CurrentTurn.TurnCompletionSource.Task;
        }

        private void PlayBlind()
        {
            CurrentBet *= BlindMultiplier;
            RaiseGameEvent($"{CurrentTurn.CurrentPlayer.PlayerName} is playing blind CurrentBet {CurrentBet}", GameState.ProcessingAction);
            if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
            {
                CurrentTurn.CurrentPlayer.BetOnBlind();
                Pot += CurrentBet;
                BlindMultiplier *= 2;
            }
            else
            {
                UIController.ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
            }
        }

        private void Bet()
        {
            int betAmount = CurrentTurn.CurrentPlayer.HasSeenHand ? CurrentBet * 2 : CurrentBet;
            RaiseGameEvent($"{CurrentTurn.CurrentPlayer.PlayerName} is calling betAmount {betAmount}", GameState.ProcessingAction);
            if (CurrentTurn.CurrentPlayer.Coins >= betAmount)
            {
                CurrentTurn.CurrentPlayer.Bet();
                Pot += betAmount;
            }
            else
            {
                UIController.ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
               Fold();
            }
        }

        private void Raise()
        {
            RaiseGameEvent($"{CurrentTurn.CurrentPlayer.PlayerName} is raising", GameState.ProcessingAction);
            if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
            {

                CurrentTurn.CurrentPlayer.Raise();
                Pot += CurrentBet;
            }
            else
            {
                UIController.ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
            }
        }

        private void Fold()
        {
            RaiseGameEvent($"Player {CurrentTurn.CurrentPlayer} is folding their hands", GameState.ProcessingAction);
            CurrentTurn.CurrentPlayer.Fold();
            EndRound(GetOtherPlayer(CurrentTurn.CurrentPlayer));
        }

        private void Show()
        {
            RaiseGameEvent("Players are showing their hands", GameState.ProcessingAction);
            HumanPlayer.ShowHand(true);
            ComputerPlayer.ShowHand(true);
            DetermineWinner();
        }

        private async void SwitchTurn()
        {
            Player nextPlayer = CurrentTurn.CurrentPlayer is HumanPlayer ? ComputerPlayer : HumanPlayer;
            await PlayerTurnAsync(nextPlayer);
        }

        private Player GetOtherPlayer(Player currentPlayer)
        {
            return currentPlayer == HumanPlayer ? ComputerPlayer : HumanPlayer;
        }

        private void DetermineWinner()
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

            EndRound(winner);
        }

        private async void EndRound(Player winner)
        {
            RaiseGameEvent($"Ending round. Winner: {(winner == null ? "Tie" : winner.PlayerName)}", GameState.EndingRound);
            UIController.StopTurnCountdown();
            if (winner == null)
            {
                UIController.ShowMessage("It's a tie! Play another round!", 5f);
            }
            else
            {
                winner.AdjustCoins(Pot);
                ScoreKeeper.AddToTotalRoundScores(winner, Pot);
                UIController.ShowMessage($"{winner.PlayerName} wins the round and {Pot} coins!", 6f);
                await Task.Delay(6000, globalCancellationTokenSource.Token);
                UIController.UpdateRoundDisplay();
            }

            if (HumanPlayer.Coins <= 0 || ComputerPlayer.Coins <= 0)
            {
                await EndGame();
            }
            else
            {
                CheckForContinuation();
            }

            Pot = 0;
            UpdateGameState();
        }

        private async void CheckForContinuation()
        {
            RaiseGameEvent("Checking for game continuation", GameState.EndingRound);
            if (CurrentRound >= MaxRounds)
            {
                Player trailingPlayer = ScoreKeeper.HumanTotalWins < ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
                Player leadingPlayer = GetOtherPlayer(trailingPlayer);

                if (trailingPlayer.Coins > leadingPlayer.Coins)
                {
                    UIController.OfferContinuation(10);
                    await Task.Delay(10000, globalCancellationTokenSource.Token);
                }
                else
                {
                    await EndGame();
                }
            }
            else
            {
                await StartNewRoundAsync();
            }
        }

        public async Task ContinueGame(bool playerWantsToContinue)
        {
            if (playerWantsToContinue)
            {
                await StartNewRoundAsync();
            }
            else
            {
                await EndGame();
            }
        }

        private async Task EndGame()
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

            UIController.ShowMessage($"Game Over! {winner.PlayerName} wins the game!", 6f);

            RaiseGameEvent($"Game Over! {winner.PlayerName} wins the game!", GameState.GameOver);

            await Task.Delay(6000, globalCancellationTokenSource.Token);
            UIController.OfferNewGame();
        }

        public void PurchaseCoins(Player player, int amount)
        {
            // This method would interface with the external service to handle coin purchases
            // For now, we'll just add the coins directly
            player.AdjustCoins(amount);
            UpdateGameState();
        }

        public void SetCurrentBet(int bet)
        {
            CurrentBet = bet;
        }
    }

    public enum GameState
    {
        Initializing,
        WaitingForPlayerAction,
        ProcessingAction,
        EndingRound,
        GameOver,
        StartNewGame,
        StartNewRound
    }
}
