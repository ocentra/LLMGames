using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    public class NetworkTurnManager : NetworkBehaviour, IEventHandler
    {
        private CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [ShowInInspector] public NetworkGameManager NetworkGameManager { get; set; }
        [ShowInInspector] public NetworkPlayerManager NetworkPlayerManager { get; set; }
        [ShowInInspector] NetworkBettingProcessManager NetworkBettingProcessManager { get; set; }

        private UniTaskCompletionSource<bool> TimerCompletionSource { get; set; }
        [ShowInInspector, ReadOnly] private IReadOnlyList<IPlayerBase> Players { get; set; }
        [ShowInInspector, ReadOnly] private float TurnDuration { get; set; }
        [ShowInInspector, ReadOnly] private int MaxRounds { get; set; }
        [ShowInInspector, ReadOnly] private IPlayerBase CurrentPlayer { get; set; }
        [ShowInInspector, ReadOnly] private IPlayerBase RoundStarter { get; set; }
        [ShowInInspector, ReadOnly] private IPlayerBase LastBettor { get; set; }
        [ShowInInspector, ReadOnly] private bool IsShowdown { get; set; }
        [ShowInInspector, ReadOnly] private int CurrentRound { get; set; } = 1;
        [ShowInInspector, ReadOnly] public bool StartedTurnManager { get; set; }

        [SerializeField] private GameMode gameMode;

        [ShowInInspector, Required]
        public GameMode GameMode
        {
            get => gameMode;
            set => gameMode = value;
        }
        void OnValidate()
        {
            InitComponents();
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
        }
        public void Initialize(float turnDuration, int maxRounds)
        {
            TurnDuration = turnDuration;
            MaxRounds = maxRounds;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitComponents();
            SubscribeToEvents();

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            UnsubscribeFromEvents();
        }

        private void InitComponents()
        {
            if (NetworkPlayerManager == null)
            {
                NetworkPlayerManager = GetComponent<NetworkPlayerManager>();
            }

            if (NetworkGameManager == null)
            {
                NetworkGameManager = GetComponent<NetworkGameManager>();
            }

            if (NetworkBettingProcessManager == null)
            {
                NetworkBettingProcessManager = GetComponent<NetworkBettingProcessManager>();
            }
            if (NetworkGameManager != null && gameMode == null)
            {
                gameMode = NetworkGameManager.GameMode;
            }

        }

        public void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<StartTurnManagerEvent>(OnStartTurnManagerEvent);
            EventBus.Instance.SubscribeAsync<TimerCompletedEvent>(OnTimerCompletedEvent);
            EventBus.Instance.SubscribeAsync<PlayerActionNewRound>(OnPlayerActionNewRound);
            EventBus.Instance.SubscribeAsync<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGameEvent);
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<StartTurnManagerEvent>(OnStartTurnManagerEvent);
            EventBus.Instance.UnsubscribeAsync<TimerCompletedEvent>(OnTimerCompletedEvent);
            EventBus.Instance.UnsubscribeAsync<PlayerActionNewRound>(OnPlayerActionNewRound);
            EventBus.Instance.UnsubscribeAsync<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGameEvent);
        }



        private async UniTask OnStartTurnManagerEvent(StartTurnManagerEvent e)
        {
            await UniTask.DelayFrame(1);

            if (IsServer && !StartedTurnManager)
            {
               
                Players = NetworkPlayerManager.GetAllPlayers();

                bool allPlayerReadyForGame = true;
                foreach (IPlayerBase playerBase in Players)
                {
                    if (!playerBase.ReadyForNewGame.Value)
                    {
                        allPlayerReadyForGame = false;
                    }
                }

                await UniTask.WaitUntil(() => allPlayerReadyForGame);

                await ResetForNewGame();
                StartedTurnManager = true;
            }

        }


        private async UniTask OnPlayerActionStartNewGameEvent(PlayerActionStartNewGameEvent arg)
        {
            if (IsServer)
            {
                await ResetForNewGame();
            }

        }
        private async UniTask OnPlayerActionNewRound(PlayerActionNewRound arg)
        {
            if (IsServer)
            {
                await ResetForNewRound();
            }

        }

        public async UniTask<bool> ResetForNewGame()
        {
            if (IsServer)
            {
                try
                {
                    CurrentRound = 0;
                    IsShowdown = false;
                    LastBettor = null;
                    RoundStarter = null;
                    CurrentPlayer = null;
                    TimerCompletionSource = new UniTaskCompletionSource<bool>();
                    await ResetForNewRound();
                    GameLoggerScriptable.Log("TurnManager reset for new game", this);
                    await UniTask.Yield();
                    return true;
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in ResetForNewGame: {ex.Message}\n{ex.StackTrace}", null);
                    return false;
                }

            }
            return false;

        }

        public async UniTask<bool> ResetForNewRound()
        {
            if (IsServer)
            {
                try
                {
                    CurrentRound++;
                    IsShowdown = false;
                    LastBettor = null;

                    if (CurrentRound == 1)
                    {
                        RoundStarter = Players[0];
                    }
                    else
                    {
                        // RoundStarter = scoreManager.GetLastRoundWinner(playerManager); // todo

                        if (RoundStarter == null)
                        {
                            if (!TryGetNextPlayerInOrder(CurrentPlayer, out IPlayerBase nextPlayer))
                            {
                                GameLoggerScriptable.LogError("Failed to determine the next player for round starter. Round reset aborted.", null);
                                return false;
                            }
                            RoundStarter = nextPlayer;
                        }
                    }

                    CurrentPlayer = RoundStarter;
                    await StartTimer(CurrentPlayer);

                    GameLoggerScriptable.Log($"TurnManager reset for round {CurrentRound}", this);
                    await UniTask.Yield();
                    return true;


                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in ResetForNewRound: {ex.Message}\n{ex.StackTrace}", this);
                    return false;
                }
            }

            return false;
        }

        private async UniTask StartTimer(IPlayerBase currentPlayer)
        {
            if (IsServer)
            {
                
                if (TimerCompletionSource != null)
                {
                    NotifyTimerStopClientRpc();
                    TimerCompletionSource.TrySetResult(false);
                }

                foreach (IPlayerBase playerBase in Players)
                {
                    playerBase.SetIsPlayerTurn(false);
                }

                CurrentPlayer.SetIsPlayerTurn();

                try
                {
                    NotifyTimerStartedClientRpc(CurrentPlayer.PlayerIndex.Value);

                }
                catch (OperationCanceledException)
                {
                    if (TimerCompletionSource != null)
                    {
                        TimerCompletionSource.TrySetResult(false);
                    }
                }

                await UniTask.Yield();
            }
        }


        [ClientRpc]
        private void NotifyTimerStartedClientRpc(int playerIndex)
        {

            TimerCompletionSource = new UniTaskCompletionSource<bool>();
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = new CancellationTokenSource();
            EventBus.Instance.Publish(new TimerStartEvent(playerIndex,TurnDuration, TimerCompletionSource, CancellationTokenSource));
            
        }

        [ClientRpc]
        private void NotifyTimerStopClientRpc()
        {
            EventBus.Instance.Publish(new TimerStopEvent());
        }



        private async UniTask OnTimerCompletedEvent(TimerCompletedEvent arg)
        {
            if (IsServer)
            {
                try
                {
                    bool isRoundComplete = IsRoundComplete();
                    if (isRoundComplete)
                    {
                        EventBus.Instance.Publish(new OfferContinuationEvent(10));
                    }

                    bool isFixedRoundsOver = IsFixedRoundsOver();

                    if (isFixedRoundsOver)
                    {
                        EventBus.Instance.Publish(new OfferNewGameEvent(10));
                    }

                    if (!isRoundComplete && !isFixedRoundsOver)
                    {
                        if (TryGetNextPlayerInOrder(CurrentPlayer, out IPlayerBase nextPlayer))
                        {
                            CurrentPlayer = nextPlayer;
                            await TimerCompletionSource.Task;
                            await StartTimer(CurrentPlayer);
                        }
                        else
                        {
                            GameLoggerScriptable.LogError("Failed to switch to next player", this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in OnTimerCompletedEvent: {ex.Message}\n{ex.StackTrace}", this);
                }
            }
            await UniTask.Yield();
        }


        private bool TryGetNextPlayerInOrder(IPlayerBase currentLLMPlayer, out IPlayerBase nextPlayer)
        {
            nextPlayer = currentLLMPlayer;

            if (IsServer)
            {
                try
                {
                    if (Players == null || currentLLMPlayer == null)
                    {
                        GameLoggerScriptable.LogError("TryGetNextPlayerInOrder called with null Players, PlayerManager, or CurrentLLMPlayer.", this);
                        return false;
                    }

                    int currentIndex = -1;
                    for (int i = 0; i < Players.Count; i++)
                    {
                        if (Players[i].Equals(currentLLMPlayer))
                        {
                            currentIndex = i;
                            break;
                        }
                    }

                    if (currentIndex == -1)
                    {
                        GameLoggerScriptable.LogError("Current player not found in Players list.", this);
                        return false;
                    }

                    IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();
                    if (activePlayers == null || activePlayers.Count == 0)
                    {
                        GameLoggerScriptable.LogError("No active players found. Returning current player.", this);
                        return false;
                    }

                    for (int i = 1; i <= Players.Count; i++)
                    {
                        int nextIndex = (currentIndex + i) % Players.Count;
                        IPlayerBase potentialNextPlayer = Players[nextIndex];

                        if (activePlayers.Contains(potentialNextPlayer))
                        {
                            nextPlayer = potentialNextPlayer;
                            GameLoggerScriptable.Log($"Next player: {nextPlayer.PlayerName.Value.Value}", this);
                            return true;
                        }
                    }

                    GameLoggerScriptable.Log("No next active player found. Returning first active player.", this);
                    nextPlayer = activePlayers[0];
                    return true;
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in TryGetNextPlayerInOrder: {ex.Message}\n{ex.StackTrace}", this);
                    return false;
                }
            }

            return false;
        }

        public void CallShow(UniTaskCompletionSource<bool> actionCompletionSource)
        {
            SetLastBettor(actionCompletionSource);
            IsShowdown = true;
        }

        public void SetLastBettor(UniTaskCompletionSource<bool> actionCompletionSource)
        {
            if (IsServer)
            {
                try
                {
                    LastBettor = CurrentPlayer;
                    actionCompletionSource?.TrySetResult(true);

                    GameLoggerScriptable.Log($"Last bettor set to {CurrentPlayer?.PlayerId}", this);
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error setting last bettor: {ex.Message}\n{ex.StackTrace}", this);
                }
            }

        }

        public bool IsRoundComplete()
        {
            if (IsServer)
            {
                IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();

                if (activePlayers.Count <= 1)
                {
                    return true;
                }

                if (IsShowdown)
                {
                    return true;
                }

                if (CurrentPlayer == LastBettor && activePlayers.Count > 1)
                {
                    return true;
                }

                return false;
            }


            return false;
        }

        public bool IsFixedRoundsOver()
        {
            return CurrentRound >= MaxRounds;
        }

        private void Reset()
        {
            if (IsServer)
            {
                try
                {
                    IsShowdown = false;
                    LastBettor = null;

                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error during reset: {ex.Message}\n{ex.StackTrace}", this);
                }
            }

        }
    }
}