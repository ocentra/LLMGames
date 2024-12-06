using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GamesNetworking;
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
    public class NetworkTurnManager : NetworkManagerBase
    {

        private CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

        private UniTaskCompletionSource<bool> TimerCompletionSource { get; set; }
        [ShowInInspector, ReadOnly] private IReadOnlyList<IPlayerBase> Players { get; set; }
        [ShowInInspector, ReadOnly] private float TurnDuration { get; set; }
        [ShowInInspector, ReadOnly] private int MaxRounds { get; set; }
        [ShowInInspector, ReadOnly] public IPlayerBase CurrentPlayer { get; set; }
        [ShowInInspector, ReadOnly] private IPlayerBase RoundStarter { get; set; }
        [ShowInInspector, ReadOnly] private IPlayerBase LastBettor { get; set; }
        [ShowInInspector, ReadOnly] private bool IsShowdown { get; set; }
        [ShowInInspector, ReadOnly] public int CurrentRound { get; set; } = 1;
        [ShowInInspector, ReadOnly] public bool StartedTurnManager { get; set; }


        public void Initialize(float turnDuration, int maxRounds)
        {
            TurnDuration = turnDuration;
            MaxRounds = maxRounds;
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventBus.Instance.SubscribeAsync<StartTurnManagerEvent>(OnStartTurnManagerEvent);
            EventBus.Instance.SubscribeAsync<TimerCompletedEvent>(OnTimerCompletedEvent);
            EventBus.Instance.SubscribeAsync<PlayerActionNewRoundEvent>(OnPlayerActionNewRound);
            EventBus.Instance.SubscribeAsync<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGameEvent);
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            EventBus.Instance.UnsubscribeAsync<StartTurnManagerEvent>(OnStartTurnManagerEvent);
            EventBus.Instance.UnsubscribeAsync<TimerCompletedEvent>(OnTimerCompletedEvent);
            EventBus.Instance.UnsubscribeAsync<PlayerActionNewRoundEvent>(OnPlayerActionNewRound);
            EventBus.Instance.UnsubscribeAsync<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGameEvent);
        }


       
        private async UniTask OnStartTurnManagerEvent(StartTurnManagerEvent e)
        {

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
        private async UniTask OnPlayerActionNewRound(PlayerActionNewRoundEvent arg)
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
                    CurrentRound = 1;
                    IsShowdown = false;
                    LastBettor = null;
                    RoundStarter = null;
                    CurrentPlayer = null;
                    TimerCompletionSource = new UniTaskCompletionSource<bool>();
                    bool resetDeck = await NetworkDeckManager.ResetForNewGame();
                    bool resetScoreManager = await NetworkScoreManager.ResetForNewGame();
                    await ResetForNewRound();
                    GameLoggerScriptable.Log("TurnManager reset for new game", this);
                    await UniTask.Yield();
                    return true;
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in ResetForNewGame: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
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
                    bool resetDeck = await NetworkDeckManager.ResetForNewRound();
                    bool resetScoreManager = await NetworkScoreManager.ResetForNewRound();
                    bool resetPlayerManager = await NetworkPlayerManager.ResetForNewRound();

                    foreach (IPlayerBase playerBase in NetworkPlayerManager.GetAllPlayers())
                    {
                        NetworkPlayer networkPlayer = playerBase as NetworkPlayer;
                        if (networkPlayer != null)
                        {
                            networkPlayer.ResetForNewRound(NetworkDeckManager);
                        }
                    }

                    IsShowdown = false;
                    LastBettor = null;

                    if (CurrentRound == 1)
                    {
                        RoundStarter = Players[0];
                    }
                    else
                    {
                        RoundStarter = NetworkScoreManager.GetLastRoundWinner();

                        if (RoundStarter == null)
                        {
                            if (!TryGetNextPlayerInOrder(RoundStarter, out IPlayerBase nextPlayer))
                            {
                                GameLoggerScriptable.LogError("Failed to determine the next player for round starter. Round reset aborted.", this, ToEditor, ToFile, UseStackTrace);
                                return false;
                            }

                            RoundStarter = nextPlayer;
                        }
                    }

                    CurrentPlayer = RoundStarter;
                    await StartTimer(CurrentPlayer);
                    GameLoggerScriptable.Log($"TurnManager reset for round {CurrentRound}", this, ToEditor, ToFile, UseStackTrace);
                    CurrentRound++;
                    await UniTask.Yield();
                    return true;


                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in ResetForNewRound: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                    return false;
                }
            }

            return false;
        }

        private async UniTask StartTimer(IPlayerBase currentPlayer)
        {
            if (IsServer)
            {

                await StopTimer();

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

        public async UniTask StopTimer()
        {
            if (TimerCompletionSource != null)
            {
                NotifyTimerStopClientRpc();
                TimerCompletionSource.TrySetResult(false);
            }

            await UniTask.Yield();
        }


        [ClientRpc]
        private void NotifyTimerStartedClientRpc(int playerIndex)
        {

            TimerCompletionSource = new UniTaskCompletionSource<bool>();
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = new CancellationTokenSource();
            EventBus.Instance.Publish(new TimerStartEvent(playerIndex, TurnDuration, TimerCompletionSource, CancellationTokenSource));

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
                    bool isFixedRoundsOver = IsFixedRoundsOver();
                    
                    if (isFixedRoundsOver || isRoundComplete)
                    {
                        await DetermineWinner();
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
                            GameLoggerScriptable.LogError("Failed to switch to next player", this, ToEditor, ToFile, UseStackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in OnTimerCompletedEvent: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
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
                        GameLoggerScriptable.LogError("TryGetNextPlayerInOrder called with null Players, PlayerManager, or CurrentLLMPlayer.", this, ToEditor, ToFile, UseStackTrace);
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
                        GameLoggerScriptable.LogError("Current player not found in Players list.", this, ToEditor, ToFile, UseStackTrace);
                        return false;
                    }

                    IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();
                    if (activePlayers == null || activePlayers.Count == 0)
                    {
                        GameLoggerScriptable.LogError("No active players found. Returning current player.", this, ToEditor, ToFile, UseStackTrace);
                        return false;
                    }

                    for (int i = 1; i <= Players.Count; i++)
                    {
                        int nextIndex = (currentIndex + i) % Players.Count;
                        IPlayerBase potentialNextPlayer = Players[nextIndex];

                        if (activePlayers.Contains(potentialNextPlayer))
                        {
                            nextPlayer = potentialNextPlayer;
                            GameLoggerScriptable.Log($"Next player: {nextPlayer.PlayerName.Value.Value}", this, ToEditor, ToFile, UseStackTrace);
                            return true;
                        }
                    }

                    GameLoggerScriptable.Log("No next active player found. Returning first active player.", this);
                    nextPlayer = activePlayers[0];
                    return true;
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in TryGetNextPlayerInOrder: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                    return false;
                }
            }

            return false;
        }

        public void CallShow()
        {
            SetLastBettor();
            IsShowdown = true;
        }

        public void SetLastBettor()
        {
            if (IsServer)
            {
                try
                {
                    LastBettor = CurrentPlayer;
                    GameLoggerScriptable.Log($"Last bettor set to {CurrentPlayer?.PlayerId}", this);
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error setting last bettor: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
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


        #region Game End and Continuation

        private async UniTask DetermineWinner()
        {
            if (!IsServer) return;


            IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();
            if (activePlayers == null || activePlayers.Count == 0)
            {
                ShowMessage("No active players found when determining the winner").Forget();
                return;
            }

            // Calculate hand values for each player
            Dictionary<NetworkPlayer, int> playerHandValues = new Dictionary<NetworkPlayer, int>();
            foreach (IPlayerBase playerBase in activePlayers)
            {
                NetworkPlayer networkPlayer = playerBase as NetworkPlayer;
                if (networkPlayer != null)
                {
                    playerHandValues[networkPlayer] = networkPlayer.CalculateHandValue();
                }
            }

            // Find the highest hand value
            int highestHandValue = int.MinValue;
            foreach (int handValue in playerHandValues.Values)
            {
                if (handValue > highestHandValue)
                {
                    highestHandValue = handValue;
                }
            }

            // Identify potential winners with the highest hand value
            List<NetworkPlayer> potentialWinners = new List<NetworkPlayer>();
            foreach (KeyValuePair<NetworkPlayer, int> player in playerHandValues)
            {
                if (player.Value == highestHandValue)
                {
                    potentialWinners.Add(player.Key);
                }
            }

            if (potentialWinners.Count == 1)
            {
                await EndRound(potentialWinners, true);
                return;
            }

            Dictionary<NetworkPlayer, int> potentialWinnersCardValues = new Dictionary<NetworkPlayer, int>();
            foreach (NetworkPlayer networkPlayer in potentialWinners)
            {

                if (networkPlayer != null)
                {
                    potentialWinnersCardValues[networkPlayer] = networkPlayer.Hand.Max();
                }
            }

            int highestCardValue = int.MinValue;
            foreach (int cardValue in potentialWinnersCardValues.Values)
            {
                if (cardValue > highestCardValue)
                {
                    highestCardValue = cardValue;
                }
            }

            List<NetworkPlayer> winners = new List<NetworkPlayer>();

            foreach (KeyValuePair<NetworkPlayer, int> player in potentialWinnersCardValues)
            {
                if (player.Value == highestCardValue)
                {
                    winners.Add(player.Key);
                }
            }

            await EndRound(winners, true);
        }

        private async UniTask EndRound(List<NetworkPlayer> winners, bool showHand)
        {
            if (!IsServer) return;

            await UniTask.SwitchToMainThread();

            if (NetworkTurnManager == null)
            {
                GameLoggerScriptable.LogError("NetworkTurnManager is null. Cannot end round.", this);
                return;
            }

            await NetworkTurnManager.StopTimer();

            if (winners == null || winners.Count == 0)
            {
                GameLoggerScriptable.LogError("EndRound called with no winners.", this);
                return;
            }

            if (winners.Count > 1)
            {
                NetworkPlayer winner = await UniTask.RunOnThreadPool(() => BreakTie(winners));
                if (winner != null)
                {
                    await HandleSingleWinner(winner, showHand);
                }
                else
                {
                    await HandleTie(winners, showHand);
                }
            }
            else
            {
                await HandleSingleWinner(winners[0], showHand);
            }
        }

        public NetworkPlayer BreakTie(List<NetworkPlayer> tiedPlayers)
        {
            if (!IsServer) return null;

            if (tiedPlayers == null || tiedPlayers.Count == 0)
            {
                GameLoggerScriptable.LogError("BreakTie called with no tied players.", this);
                return null;
            }

            // Find the players with the highest card value
            int maxHighCard = int.MinValue;
            List<NetworkPlayer> playersWithMaxHighCard = new List<NetworkPlayer>();

            foreach (NetworkPlayer player in tiedPlayers)
            {

                if (player == null || player.Hand == null || player.Hand.Count() == 0)
                {
                    continue;
                }

                int highestCard = player.Hand.Max();

                if (highestCard > maxHighCard)
                {
                    maxHighCard = highestCard;
                    playersWithMaxHighCard.Clear();
                    playersWithMaxHighCard.Add(player);
                }
                else if (highestCard == maxHighCard)
                {
                    playersWithMaxHighCard.Add(player);
                }
            }

            if (playersWithMaxHighCard.Count == 1)
            {
                return playersWithMaxHighCard[0];
            }

            // Find the players with the highest second card value
            int maxSecondHighCard = int.MinValue;
            List<NetworkPlayer> playersWithMaxSecondHighCard = new List<NetworkPlayer>();

            foreach (NetworkPlayer player in playersWithMaxHighCard)
            {
                if (player.Hand == null || player.Hand.Count() < 2)
                    continue;

                int secondHighestCard = player.Hand.OrderByDescending(c => c.Rank.Value).Skip(1).FirstOrDefault().Rank.Value;

                if (secondHighestCard > maxSecondHighCard)
                {
                    maxSecondHighCard = secondHighestCard;
                    playersWithMaxSecondHighCard.Clear();
                    playersWithMaxSecondHighCard.Add(player);
                }
                else if (secondHighestCard == maxSecondHighCard)
                {
                    playersWithMaxSecondHighCard.Add(player);
                }
            }

            if (playersWithMaxSecondHighCard.Count == 1)
            {
                return playersWithMaxSecondHighCard[0];
            }

            // Find the players with the highest lowest card value if still tied
            int maxLowestCard = int.MinValue;
            List<NetworkPlayer> winnersWithMaxLowestCard = new List<NetworkPlayer>();

            foreach (NetworkPlayer player in playersWithMaxSecondHighCard)
            {
                if (player.Hand == null || player.Hand.Count() == 0)
                    continue;

                int lowestCard = player.Hand.Min();

                if (lowestCard > maxLowestCard)
                {
                    maxLowestCard = lowestCard;
                    winnersWithMaxLowestCard.Clear();
                    winnersWithMaxLowestCard.Add(player);
                }
                else if (lowestCard == maxLowestCard)
                {
                    winnersWithMaxLowestCard.Add(player);
                }
            }

            return winnersWithMaxLowestCard.Count == 1 ? winnersWithMaxLowestCard[0] : null;
        }

        private async UniTask HandleTie(List<NetworkPlayer> winners, bool showHand)
        {
            if (!IsServer) return;

            if (NetworkScoreManager == null || NetworkTurnManager == null || NetworkPlayerManager == null)
            {
                GameLoggerScriptable.LogError("Critical component is null in HandleTie.", this);
                return;
            }

            if (await NetworkScoreManager.AwardTiedPot(winners))
            {
                EventBus.Instance.Publish(new UpdateRoundDisplayEvent<NetworkScoreManager>(NetworkScoreManager));
                OfferContinuation(showHand);
            }
            else
            {
                GameLoggerScriptable.LogError("Failed to award tied pot.", this);
            }

            await UniTask.CompletedTask;
        }

        private async UniTask HandleSingleWinner(NetworkPlayer winner, bool showHand)
        {
            if (!IsServer) return;

            if (NetworkScoreManager == null || NetworkTurnManager == null || NetworkPlayerManager == null)
            {
                GameLoggerScriptable.LogError("Critical component is null in HandleSingleWinner.", this);
                return;
            }

            try
            {
                if (await NetworkScoreManager.AwardPotToWinner(winner))
                {
                    EventBus.Instance.Publish(new UpdateRoundDisplayEvent<NetworkScoreManager>(NetworkScoreManager));

                    bool playerWithZeroCoinsFound = false;
                    IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();

                    foreach (IPlayerBase player in activePlayers)
                    {
                        if (player.GetCoins() <= 0)
                        {
                            playerWithZeroCoinsFound = true;
                            break;
                        }
                    }

                    if (playerWithZeroCoinsFound)
                    {
                        await EndGame();
                    }
                    else
                    {
                        await CheckForContinuation(showHand);
                    }
                }
                else
                {
                    GameLoggerScriptable.LogError("Failed to award pot to winner.", this);
                }
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in HandleSingleWinner: {ex.Message}\n{ex.StackTrace}", this);
            }
        }

        private async UniTask CheckForContinuation(bool showHand)
        {
            if (!IsServer) return;

            if (NetworkTurnManager.IsFixedRoundsOver())
            {
                List<(ulong PlayerId, int Wins, int TotalWinnings)> leaderboard = NetworkScoreManager.GetLeaderboard();

                // If there's only one player, or the top player has zero winnings, or the top player has more winnings than the second player
                if (leaderboard.Count <= 1 ||
                    (leaderboard.Count > 1 && leaderboard[0].TotalWinnings > leaderboard[1].TotalWinnings && leaderboard[0].TotalWinnings > 0))
                {
                    await EndGame();
                }

                else
                {
                    OfferContinuation(showHand);
                }
            }
            else
            {
                OfferContinuation(showHand);
            }
        }
        private void OfferContinuation(bool showHand)
        {
            if (!IsServer) return;

            NetworkTurnManager.CallShow();
            NetworkPlayerManager.ShowHand(showHand, true);
            EventBus.Instance.Publish(new OfferContinuationEvent(10));
        }
        private UniTask EndGame()
        {
            if (!IsServer) return UniTask.CompletedTask;

            NetworkTurnManager.CallShow();
            NetworkPlayerManager.ShowHand(true);
            EventBus.Instance.Publish(new OfferNewGameEvent(60));
            return UniTask.CompletedTask;
        }

        private async UniTask ShowMessage(string message, bool delay = true, float delayTime = 5f)
        {
            if (!IsServer) return;

            await UniTask.SwitchToMainThread();
            EventBus.Instance.Publish(new UIMessageEvent(message, delayTime));
            if (delay)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
                }
                catch (OperationCanceledException)
                {
                    GameLoggerScriptable.Log("ShowMessage delay was cancelled.", this);
                }
            }
        }

        #endregion


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
                    GameLoggerScriptable.LogError($"Error during reset: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                }
            }

        }
    }
}