using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Players;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.Manager
{
    [Serializable]
    public class TurnManager : ManagerBase<TurnManager>
    {
        [ShowInInspector, ReadOnly] public float TurnDuration { get; private set; }
        [ShowInInspector, ReadOnly] public int MaxRounds { get; private set; }
        [ShowInInspector][ReadOnly] private List<LLMPlayer> Players { get; set; }
        [ShowInInspector][ReadOnly] public LLMPlayer CurrentLLMPlayer { get; private set; }
        [ShowInInspector][ReadOnly] public LLMPlayer RoundStarter { get; private set; }
        [ShowInInspector][ReadOnly] public LLMPlayer LastBettor { get; private set; }
        [ShowInInspector][ReadOnly] public bool IsShowdown { get; private set; }
        [ShowInInspector][ReadOnly] public int CurrentRound { get; private set; } = 1;
        [ShowInInspector][ReadOnly] public float RemainingTime { get; private set; }
        private ITurnTimer TurnTimer { get; set; }

        
        private List<LLMPlayer> InitializePlayers(PlayerManager playerManager)
        {
            List<LLMPlayer> allPlayers = playerManager.GetAllPlayers();

            allPlayers.Sort((p1, p2) => p1.PlayerIndex.CompareTo(p2.PlayerIndex));

            List<LLMPlayer> orderedPlayers = new List<LLMPlayer>();

            foreach (LLMPlayer player in allPlayers)
            {
                if (player is HumanLLMPlayer)
                {
                    orderedPlayers.Add(player);
                }
            }

            foreach (LLMPlayer player in allPlayers)
            {
                if (player is ComputerLLMPlayer)
                {
                    orderedPlayers.Add(player);
                }
            }

            if (!orderedPlayers.Any(p => p is HumanLLMPlayer) || !orderedPlayers.Any(p => p is ComputerLLMPlayer))
            {
                LogError("At least one computer player and one human player are required.", null);
                return new List<LLMPlayer>();
            }

            Log($"Players initialized. Total players: {orderedPlayers.Count}", this);
            return orderedPlayers;
        }



        public async UniTask<bool> ResetForNewGame(PlayerManager playerManager, GameMode gameMode)
        {
            try
            {
                CurrentRound = 0;
                IsShowdown = false;
                LastBettor = null;
                RoundStarter = null;
                CurrentLLMPlayer = null;
                TurnDuration = gameMode.TurnDuration;
                RemainingTime = TurnDuration;
                MaxRounds = gameMode.MaxRounds;

                TurnTimer = new TurnTimer(TurnDuration);
                Players = InitializePlayers(playerManager);

                Log("TurnManager reset for new game", this);
                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewGame: {ex.Message}\n{ex.StackTrace}", null);
                return false;
            }
        }



        public async UniTask<bool> ResetForNewRound(ScoreManager scoreManager, PlayerManager playerManager)
        {
            try
            {
                CurrentRound++;
                IsShowdown = false;
                LastBettor = null;

                if (CurrentRound == 1)
                {
                    RoundStarter = Players[0]; // Human player starts
                }
                else
                {
                    RoundStarter = scoreManager.GetLastRoundWinner(playerManager);

                    if (RoundStarter == null)
                    {
                        if (!TryGetNextPlayerInOrder(RoundStarter, playerManager, out LLMPlayer nextPlayer))
                        {
                            LogError("Failed to determine the next player for round starter. Round reset aborted.", null);
                            return false;
                        }
                        RoundStarter = nextPlayer;
                    }
                }

                CurrentLLMPlayer = RoundStarter;
                RemainingTime = TurnDuration;

                Log($"TurnManager reset for round {CurrentRound}", this);
                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewRound: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        public void StartTurn(CancellationTokenSource turnCancellationTokenSource, UniTaskCompletionSource<bool> timerCompletionSource)
        {

            StartTurnInternal(turnCancellationTokenSource, timerCompletionSource).Forget();


        }

        private async UniTask StartTurnInternal(CancellationTokenSource turnCancellationTokenSource, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            try
            {
                Log($"Turn started for {CurrentLLMPlayer.GetType().Name}", this);
                Reset();

                TurnTimer.StartTimer(turnCancellationTokenSource.Token, timerCompletionSource);
                await UniTask.SwitchToMainThread();

                EventBus.Instance.Publish(new PlayerStartCountDownEvent<TurnManager>(this));

            }
            catch (Exception ex)
            {
                LogError($"Error starting turn: {ex.Message}\n{ex.StackTrace}", this);
            }
        }




        public async UniTask SwitchTurn(PlayerManager playerManager, CancellationTokenSource globalCancellationTokenSource, CancellationTokenSource turnCancellationTokenSource, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            try
            {
                if (globalCancellationTokenSource.IsCancellationRequested)
                {
                    Log("Turn switch cancelled due to global cancellation", this);
                    return;
                }

                await StopTurnInternal(turnCancellationTokenSource, timerCompletionSource);

                if (TryGetNextPlayerInOrder(CurrentLLMPlayer, playerManager, out LLMPlayer nextPlayer))
                {
                    CurrentLLMPlayer = nextPlayer;
                    Log($"Switching turn to {CurrentLLMPlayer.GetType().Name}", this);

                    await StartTurnInternal(turnCancellationTokenSource, timerCompletionSource);
                    await UniTask.SwitchToMainThread();
                    EventBus.Instance.Publish(new UpdateTurnStateEvent<LLMPlayer>(CurrentLLMPlayer, CurrentLLMPlayer is HumanLLMPlayer, CurrentLLMPlayer is ComputerLLMPlayer));

                }
                else
                {
                    LogError("Failed to get the next player. Turn switch aborted.", this);
                }

                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                LogError($"Error switching turn: {ex.Message}\n{ex.StackTrace}", this);
            }
        }



        public async UniTask StopTurn(CancellationTokenSource turnCancellationTokenSource, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            await StopTurnInternal(turnCancellationTokenSource, timerCompletionSource);
            await UniTask.Yield();

        }

        private async UniTask StopTurnInternal(CancellationTokenSource turnCancellationTokenSource, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            try
            {
                Log("Stopping turn", this);

                TurnTimer.StopTimer(timerCompletionSource);
                turnCancellationTokenSource?.Cancel();
                turnCancellationTokenSource?.Dispose();
                timerCompletionSource?.TrySetResult(true);

                await UniTask.SwitchToMainThread();
                EventBus.Instance.Publish(new PlayerStopCountDownEvent<LLMPlayer>(CurrentLLMPlayer));

                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                LogError($"Error stopping turn: {ex.Message}\n{ex.StackTrace}", this);
            }
        }



        private bool TryGetNextPlayerInOrder(LLMPlayer currentLLMPlayer, PlayerManager playerManager, out LLMPlayer nextLLMPlayer)
        {
            nextLLMPlayer = currentLLMPlayer;

            try
            {
                if (Players == null || playerManager == null || currentLLMPlayer == null)
                {
                    LogError("TryGetNextPlayerInOrder called with null Players, PlayerManager, or CurrentLLMPlayer.", this);
                    return false;
                }

                int currentIndex = Players.IndexOf(currentLLMPlayer);
                if (currentIndex == -1)
                {
                    LogError("Current player not found in Players list.", this);
                    return false;
                }

                List<LLMPlayer> activePlayers = playerManager.GetActivePlayers();
                if (activePlayers == null || activePlayers.Count == 0)
                {
                    LogError("No active players found. Returning current player.", this);
                    return false;
                }

                for (int i = 1; i <= Players.Count; i++)
                {
                    int nextIndex = (currentIndex + i) % Players.Count;
                    LLMPlayer potentialNextPlayer = Players[nextIndex];

                    if (potentialNextPlayer?.AuthPlayerData != null && activePlayers.Contains(potentialNextPlayer))
                    {
                        nextLLMPlayer = potentialNextPlayer;
                        Log($"Next player: {nextLLMPlayer.AuthPlayerData.PlayerID}", this);
                        return true;
                    }
                }

                Log("No next active player found. Returning first active player.", this);
                nextLLMPlayer = activePlayers[0];
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in TryGetNextPlayerInOrder: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }




        private void Reset()
        {
            try
            {
                RemainingTime = TurnDuration;
                IsShowdown = false;
                LastBettor = null;

                Log($"Reset completed. RemainingTime set to {RemainingTime}. New turn state initialized.", this);
            }
            catch (Exception ex)
            {
                LogError($"Error during reset: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        public void SetLastBettor(UniTaskCompletionSource<bool> actionCompletionSource)
        {
            try
            {
                LastBettor = CurrentLLMPlayer;
                actionCompletionSource?.TrySetResult(true);

                Log($"Last bettor set to {CurrentLLMPlayer?.AuthPlayerData?.PlayerID}", this);
            }
            catch (Exception ex)
            {
                LogError($"Error setting last bettor: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        public void CallShow(UniTaskCompletionSource<bool> actionCompletionSource)
        {
            SetLastBettor(actionCompletionSource);
            IsShowdown = true;
        }

        public bool IsRoundComplete(PlayerManager playerManager)
        {
            List<LLMPlayer> activePlayers = playerManager.GetActivePlayers();

            if (activePlayers.Count <= 1)
            {
                return true;
            }

            if (IsShowdown)
            {
                return true;
            }

            if (CurrentLLMPlayer == LastBettor && activePlayers.Count > 1)
            {
                return true;
            }

            return false;
        }

        public bool IsFixedRoundsOver()
        {
            return CurrentRound >= MaxRounds;
        }

        public override void Log(string message, Object context, bool toEditor = default, bool toFile = default)
        {
            base.Log($"  [CurrentRound {CurrentRound}] {message}", context, toEditor, toFile);
        }

        public override void LogError(string message, Object context, bool toEditor = default, bool toFile = default)
        {
            base.LogError($" [CurrentRound {CurrentRound}] {message}", context, toEditor, toFile);
        }
    }
}