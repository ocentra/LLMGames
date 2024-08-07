using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [Serializable]
    public class TurnManager : ManagerBase<TurnManager>
    {


        [ShowInInspector, ReadOnly] public float TurnDuration
        {
            get
            {
                if (GameManager.instance !=null && GameManager.instance.GameMode !=null)
                {
                    return GameManager.instance.GameMode.TurnDuration;

                }
                return 60;
            }
        }

        [ShowInInspector, ReadOnly] public int MaxRounds
        {
            get
            {
                if (GameManager.instance != null && GameManager.instance.GameMode != null)
                {
                    return GameManager.instance.GameMode.MaxRounds;

                }
                return 10;
            }
        }

        [ShowInInspector, ReadOnly] private List<Player> Players { get; set; }
        [ShowInInspector, ReadOnly] public Player CurrentPlayer { get; private set; }
        [ShowInInspector, ReadOnly] public Player RoundStarter { get; private set; }
        [ShowInInspector, ReadOnly] public Player LastBettor { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsShowdown { get; private set; }
        [ShowInInspector, ReadOnly] public int CurrentRound { get; private set; } = 0;
        [ShowInInspector, ReadOnly] public float RemainingTime { get; private set; }

        public TaskCompletionSource<bool> ActionCompletionSource { get; private set; }
        public TaskCompletionSource<bool> TimerCompletionSource { get; private set; }
        private CancellationTokenSource TurnCancellationTokenSource { get; set; }

        private CancellationTokenSource GlobalCancellationTokenSource => GameManager.Instance.GlobalCancellationTokenSource;
        private PlayerManager PlayerManager => PlayerManager.Instance;
        private ScoreManager ScoreManager => ScoreManager.Instance;
        private PlayerStartCountDown PlayerStartCountDown { get; set; }


        protected override void Awake()
        {
            base.Awake();
            PlayerStartCountDown = new PlayerStartCountDown(this);

        }




        private List<Player> InitializePlayers()
        {
            var allPlayers = PlayerManager.GetAllPlayers();
            var computerPlayers = allPlayers.Where(p => p is ComputerPlayer).ToList();
            var humanPlayers = allPlayers.Where(p => p is HumanPlayer).ToList();

            if (computerPlayers.Count == 0 || humanPlayers.Count == 0)
            {
                LogError("At least one computer player and one human player are required.", nameof(InitializePlayers));
                return new List<Player>();
            }

            var orderedPlayers = new List<Player> { humanPlayers[0] };
            orderedPlayers.AddRange(computerPlayers);

            Log($"Players initialized. Total players: {orderedPlayers.Count}", nameof(InitializePlayers));
            return orderedPlayers;
        }

        public void ResetForNewGame()
        {
            CurrentRound = 0;
            IsShowdown = false;
            LastBettor = null;
            RoundStarter = null;
            CurrentPlayer = null;
            RemainingTime = TurnDuration;
            ActionCompletionSource = new TaskCompletionSource<bool>();
            TimerCompletionSource = new TaskCompletionSource<bool>();

            Players = InitializePlayers();
            Log("TurnManager reset for new game", "ResetForNewGame");
        }

        public void ResetForNewRound()
        {
            CurrentRound++;
            IsShowdown = false;
            LastBettor = null;

            if (CurrentRound == 1)
            {
                // First round of the game, human starts
                RoundStarter = Players[0]; // Human player starts
            }
            else
            {
                // Subsequent rounds
                RoundStarter = ScoreManager.GetLastRoundWinner();
                if (RoundStarter == null)
                {
                    RoundStarter = GetNextPlayerInOrder(RoundStarter);
                }
            }

            CurrentPlayer = RoundStarter;
            RemainingTime = TurnDuration;
            ActionCompletionSource = new TaskCompletionSource<bool>();
            TimerCompletionSource = new TaskCompletionSource<bool>();

            Log($"TurnManager reset for round {CurrentRound}", "ResetForNewRound");
        }

        public void StartTurn()
        {
            Log($"Turn started for {CurrentPlayer.GetType().Name}", "StartTurn");
            Reset();
            TurnCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationTokenSource.Token);

            StartTimer();
            EventBus.Publish(new UpdateTurnState(CurrentPlayer));
        }

        private async void StartTimer()
        {
            Log($"Timer started. Duration: {TurnDuration}", "StartTimer");
            int remainingSeconds = (int)TurnDuration;

            try
            {
                while (remainingSeconds > 0 && !TurnCancellationTokenSource.IsCancellationRequested)
                {
                    RemainingTime = remainingSeconds;
                    Log($"Timer update: {remainingSeconds}s remaining", $"{nameof(StartTimer)} @ while loop ");
                    await Task.Delay(1000, TurnCancellationTokenSource.Token);

                    if (PlayerStartCountDown != null)
                    {
                        EventBus.Publish(PlayerStartCountDown);
                    }
                    else
                    {
                        Log("Warning: PlayerStartCountDown is null", "StartTimer");
                    }

                    remainingSeconds--;
                }

                RemainingTime = 0;
                if (PlayerStartCountDown != null)
                {
                    EventBus.Publish(PlayerStartCountDown);
                }
                Log("Timer completed", "StartTimer");
                TimerCompletionSource?.TrySetResult(true);
            }
            catch (TaskCanceledException)
            {
                Log("Timer task was canceled", "StartTimer");
            }
            catch (Exception ex)
            {
                LogError($"Exception in StartTimer: {ex.Message}", "StartTimer");
                TimerCompletionSource?.TrySetException(ex);
            }
        }
        public async Task SwitchTurn()
        {
            if (GlobalCancellationTokenSource.IsCancellationRequested)
            {
                Log("Turn switch cancelled due to global cancellation", "SwitchTurn");
                return;
            }

            await StopTurn();

            CurrentPlayer = GetNextPlayerInOrder(CurrentPlayer);
            Log($"Switching turn to {CurrentPlayer.GetType().Name}", "SwitchTurn");

            ActionCompletionSource = new TaskCompletionSource<bool>();
            TimerCompletionSource = new TaskCompletionSource<bool>();

            StartTurn();

            // Ensure any pending operations for the previous turn are completed
            await Task.Yield();
        }

        private Player GetNextPlayerInOrder(Player currentPlayer)
        {
            int currentIndex = Players.IndexOf(currentPlayer);
            var activePlayers = PlayerManager.GetActivePlayers();

            if (activePlayers.Count == 0)
            {
                LogError("No active players found. Returning current player.", "GetNextPlayerInOrder");
                return currentPlayer;
            }

            for (int i = 1; i <= Players.Count; i++)
            {
                int nextIndex = (currentIndex + i) % Players.Count;
                Player nextPlayer = Players[nextIndex];
                if (activePlayers.Contains(nextPlayer))
                {
                    Log($"Next player: {nextPlayer.PlayerName}", "GetNextPlayerInOrder");
                    return nextPlayer;
                }
            }

            Log("No next active player found. Returning first active player.", "GetNextPlayerInOrder");
            return activePlayers[0];
        }

        public async Task StopTurn()
        {
            Log("Stopping turn", "StopTurn");

            try
            {
                TurnCancellationTokenSource?.Cancel();

                await Task.Delay(100);

                TurnCancellationTokenSource?.Dispose();
                TurnCancellationTokenSource = new CancellationTokenSource();

                ActionCompletionSource.TrySetResult(true);

                TimerCompletionSource.TrySetResult(true);

                EventBus.Publish(new PlayerStopCountDown(CurrentPlayer));

                RemainingTime = TurnDuration;

                await Task.Yield();
            }
            catch (Exception ex)
            {
                LogError($"Error stopping turn: {ex.Message}", "StopTurn");
            }
        }

        private void Reset()
        {
            try
            {
                RemainingTime = TurnDuration;

                ActionCompletionSource?.TrySetResult(true);
                TimerCompletionSource?.TrySetResult(true);

                ActionCompletionSource = new TaskCompletionSource<bool>();
                TimerCompletionSource = new TaskCompletionSource<bool>();

                IsShowdown = false;
                LastBettor = null;

                TurnCancellationTokenSource?.Cancel();
                TurnCancellationTokenSource?.Dispose();
                TurnCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationTokenSource.Token);

                Log($"Reset completed. RemainingTime set to {RemainingTime}. New turn state initialized.", "Reset");
            }
            catch (Exception ex)
            {
                LogError($"Error during reset: {ex.Message}", "Reset");
            }
        }



        public void SetLastBettor()
        {
            LastBettor = CurrentPlayer;
            ActionCompletionSource.TrySetResult(true);

            Log($"Last bettor set to {CurrentPlayer.PlayerName}", "SetLastBettor");
        }

        public void CallShow()
        {
            SetLastBettor();
            IsShowdown = true;
        }

        public bool IsRoundComplete()
        {
            var activePlayers = PlayerManager.GetActivePlayers();

            if (activePlayers.Count <= 1)
            {
                Log("Round complete: Only one active player remaining", "IsRoundComplete");
                return true;
            }

            if (IsShowdown)
            {
                Log("Round complete: Showdown initiated", "IsRoundComplete");
                return true;
            }

            if (CurrentPlayer == LastBettor && activePlayers.Count > 1)
            {
                Log("Round complete: All active players have had a turn since last bet", "IsRoundComplete");
                return true;
            }

            Log("Round not complete: Continuing play", "IsRoundComplete");
            return false;
        }

        public bool IsFixedRoundsOver()
        {
            return CurrentRound > MaxRounds;
        }

        protected override void Log(string message, string method)
        {
            base.Log($"[Turn {CurrentRound}] {message}", method);
        }

        protected override void LogError(string message, string method)
        {
            base.LogError($"[Turn {CurrentRound}] {message} ", method);
        }


    }
}
