using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    public class NetworkTurnManager : NetworkManagerBase
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }
        [ShowInInspector, ReadOnly] private float TurnDuration { get; set; }
        [ShowInInspector, ReadOnly] public int MaxRounds { get; set; }
        [ShowInInspector, ReadOnly] public IPlayerBase CurrentPlayer { get; set; }
        [ShowInInspector, ReadOnly] private bool IsShowdown { get; set; }
        [ShowInInspector, ReadOnly] public int CurrentRound { get; private set; } = 1;
        [ShowInInspector, ReadOnly] public bool StartedTurnManager { get; set; }
        [ShowInInspector, ReadOnly] private HashSet<ulong> PlayersReadyForNextRound { get; set; } = new HashSet<ulong>();
        [ShowInInspector, ReadOnly] private HashSet<ulong> PlayersReadyForNewGame { get; set; } = new HashSet<ulong>();
        [ShowInInspector, ReadOnly] bool AllPlayersReady { get; set; } = false;

        public void Initialize(float turnDuration, int maxRounds)
        {
            TurnDuration = turnDuration;
            MaxRounds = maxRounds;
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<StartTurnManagerEvent>(OnStartTurnManagerEvent);
            EventRegistrar.Subscribe<TurnCompletedEvent>(OnTimerCompletedEvent);
            EventRegistrar.Subscribe<PlayerActionNewRoundEvent>(OnPlayerActionNewRound);
            EventRegistrar.Subscribe<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGameEvent);
            EventRegistrar.Subscribe<DecisionTakenEvent>(OnDecisionTakenEvent);
            EventRegistrar.Subscribe<DetermineWinnerEvent>(OnDetermineWinnerEvent);
        }



        private async UniTask OnStartTurnManagerEvent(StartTurnManagerEvent e)
        {

            if (IsServer && !StartedTurnManager)
            {
                IReadOnlyList<IPlayerBase> players = NetworkPlayerManager.GetAllPlayers();

                bool allPlayerReadyForGame = true;
                foreach (IPlayerBase playerBase in players)
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
        private async UniTask OnDetermineWinnerEvent(DetermineWinnerEvent arg)
        {
            await DetermineWinner();
            await UniTask.Yield();

        }
        private async UniTask OnDecisionTakenEvent(DecisionTakenEvent decisionTakenEvent)
        {
            if (IsServer && CurrentPlayer == decisionTakenEvent.PlayerBase)
            {
                if (decisionTakenEvent.EndTurn)
                {
                    CurrentPlayer.SetHasTakenBettingDecision(true);
                    IsShowdown = decisionTakenEvent.Decision == PlayerDecision.ShowCall;
                    await TurnCompleted();
                }

                await UniTask.Yield();
            }
        }

        private async UniTask OnPlayerActionStartNewGameEvent(PlayerActionStartNewGameEvent arg)
        {
            if (NetworkPlayerManager.LocalNetworkHumanPlayer is { } player)
            {
                HandlePlayerReadyForNewGameServerRpc(player.PlayerId.Value);
            }

            if (IsServer)
            {
                await UniTask.WaitUntil(() => AllPlayersReady);

                if (AllPlayersReady)
                {
                    ResetReadyPlayers();
                    await ResetForNewGame();
                    NotifyNewRoundStartedClientRpc(true);
                }
            }
        }

        private async UniTask OnPlayerActionNewRound(PlayerActionNewRoundEvent arg)
        {
            if (NetworkPlayerManager.LocalNetworkHumanPlayer is { } player)
            {
                HandlePlayerReadyForNextRoundServerRpc(player.PlayerId.Value);
            }

            if (IsServer)
            {
                await UniTask.WaitUntil(() => AllPlayersReady);

                if (AllPlayersReady)
                {
                    CurrentRound++;
                    PlayersReadyForNextRound.Clear();
                    await ResetForNewRound();
                    NotifyNewRoundStartedClientRpc(false);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void HandlePlayerReadyForNextRoundServerRpc(ulong clientId)
        {
            if (!IsServer) return;

            PlayersReadyForNextRound.Add(clientId);
            AllPlayersReady = true;
            IReadOnlyList<IPlayerBase> humanPlayers = NetworkPlayerManager.GetAllHumanPlayers();

            foreach (IPlayerBase player in humanPlayers)
            {
                if (!PlayersReadyForNextRound.Contains(player.PlayerId.Value))
                {
                    AllPlayersReady = false;
                    break;
                }
            }


        }

        [ServerRpc(RequireOwnership = false)]
        private void HandlePlayerReadyForNewGameServerRpc(ulong clientId)
        {
            if (!IsServer) return;

            PlayersReadyForNewGame.Add(clientId);
            AllPlayersReady = true;
            IReadOnlyList<IPlayerBase> humanPlayers = NetworkPlayerManager.GetAllHumanPlayers();

            foreach (IPlayerBase player in humanPlayers)
            {
                if (!PlayersReadyForNewGame.Contains(player.PlayerId.Value))
                {
                    AllPlayersReady = false;
                    break;
                }
            }

        }

        [ClientRpc]
        private void NotifyNewRoundStartedClientRpc(bool isNewGame)
        {
            EventBus.Instance.Publish(new NewRoundStartedEvent(isNewGame));
        }

        public void ResetReadyPlayers()
        {
            if (IsServer)
            {
                PlayersReadyForNextRound.Clear();
                PlayersReadyForNewGame.Clear();
            }
        }

        public async UniTask<bool> ResetForNewGame()
        {
            if (IsServer)
            {
                try
                {
                    IsShowdown = false;
                    CurrentPlayer = null;
                    CurrentRound = 1;
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
            if (!IsServer) { return false; }

            try
            {
                bool resetDeck = await NetworkDeckManager.ResetForNewRound();
                bool resetScoreManager = await NetworkScoreManager.ResetForNewRound();
                bool resetPlayerManager = await NetworkPlayerManager.ResetForNewRound();
                IReadOnlyList<IPlayerBase> players = NetworkPlayerManager.GetAllPlayers();

                foreach (IPlayerBase playerBase in players)
                {
                    NetworkPlayer networkPlayer = playerBase as NetworkPlayer;
                    if (networkPlayer != null)
                    {
                        networkPlayer.ResetForNewRound(NetworkDeckManager);
                    }
                }

                IsShowdown = false;

                if (CurrentRound == 1)
                {
                    CurrentPlayer = players[0];
                }
                else
                {
                    IPlayerBase lastRoundWinner = NetworkScoreManager.GetLastRoundWinner();

                    CurrentPlayer = lastRoundWinner ?? players[0];
                }

                CurrentPlayer.SetIsPlayerTurn();

                if (CurrentPlayer is IComputerPlayerData computerPlayer)
                {
                    computerPlayer.SimulateComputerPlayerTurn(CurrentPlayer.PlayerId.Value, NetworkScoreManager.CurrentBet.Value).Forget();
                }

                return await StartTurn();

            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in ResetForNewRound: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                return false;
            }
        }

        private async UniTask<bool> SwitchTurnAsync()
        {
            if (!IsServer) { return false; }

            if (TryGetNextPlayerInOrder(CurrentPlayer, out IPlayerBase nextPlayer))
            {
                CurrentPlayer = nextPlayer;

                if (CurrentPlayer is IComputerPlayerData computerPlayer)
                {
                    computerPlayer.SimulateComputerPlayerTurn(CurrentPlayer.PlayerId.Value, NetworkScoreManager.CurrentBet.Value).Forget();
                }

                StartTurn().Forget();
                await UniTask.Yield();
                return true;
            }

            GameLoggerScriptable.LogError("Failed to determine the next player for round starter. Round reset aborted.", this, ToEditor, ToFile, UseStackTrace);
            return false;
        }

        private async UniTask<bool> StartTurn()
        {
            if (!IsServer) { return false; }

            NotifyTimerStopClientRpc();
            await UniTask.Delay(1);
            NotifyTimerStartedClientRpc(CurrentPlayer.PlayerId.Value, CurrentPlayer.PlayerIndex.Value);
            GameLoggerScriptable.Log($"TurnManager reset for round {CurrentRound}", this, ToEditor, ToFile, UseStackTrace);
            return true;
        }

        private async UniTask OnTimerCompletedEvent(TurnCompletedEvent arg)
        {
            if (!IsServer) { return; }

            await TurnCompleted();
            await UniTask.Yield();
        }
        private async UniTask TurnCompleted()
        {
            if (!IsServer) { return; }

            try
            {
                bool isRoundComplete = IsRoundComplete();
                bool isFixedRoundsOver = IsFixedRoundsOver();

                if (isFixedRoundsOver || isRoundComplete || IsShowdown)
                {
                    await DetermineWinner();
                    return;
                }

                if (!CurrentPlayer.HasTakenBettingDecision.Value)
                {
                    CurrentPlayer.AutoBet();
                    return;
                }

                SwitchTurnAsync().Forget();

            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in OnTimerCompletedEvent: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
            }

            await UniTask.Yield();
        }


        private bool TryGetNextPlayerInOrder(IPlayerBase currentPlayer, out IPlayerBase nextPlayer)
        {
            nextPlayer = currentPlayer;
            if (!IsServer) { return false; }

            try
            {
                IReadOnlyList<IPlayerBase> players = NetworkPlayerManager.GetAllPlayers();

                if (players == null || currentPlayer == null)
                {
                    GameLoggerScriptable.LogError("TryGetNextPlayerInOrder called with null Players, PlayerManager, or CurrentLLMPlayer.", this, ToEditor, ToFile, UseStackTrace);
                    return false;
                }

                int currentIndex = -1;
                for (int i = 0; i < players.Count; i++)
                {

                    if (players[i].Equals(currentPlayer))
                    {
                        currentIndex = i;
                        break;
                    }
                }

                IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();

                if (activePlayers == null || activePlayers.Count == 0)
                {
                    GameLoggerScriptable.LogError("No active players found. Returning current player.", this, ToEditor, ToFile, UseStackTrace);
                    return false;
                }

                for (int i = 1; i <= players.Count; i++)
                {
                    int nextIndex = (currentIndex + i) % players.Count;
                    IPlayerBase potentialNextPlayer = players[nextIndex];

                    if (activePlayers.Contains(potentialNextPlayer))
                    {
                        nextPlayer = potentialNextPlayer;
                        GameLoggerScriptable.Log($"Next player: {nextPlayer.PlayerName.Value.Value}", this, ToEditor, ToFile, UseStackTrace);
                        break;
                    }
                }

                if (nextPlayer == null)
                {
                    GameLoggerScriptable.Log("No next active player found. Returning first active player.", this);
                    nextPlayer = activePlayers[0];
                }

                foreach (IPlayerBase playerBase in players)
                {
                    playerBase.SetIsPlayerTurn(false);
                }

                nextPlayer.SetHasTakenBettingDecision(false);
                nextPlayer.SetIsPlayerTurn();
                return true;
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in TryGetNextPlayerInOrder: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                return false;
            }
        }


        public bool IsRoundComplete()
        {
            if (!IsServer) { return false; }

            IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();

            return activePlayers.Count == 1;
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
                ShowMessage("No active players found when determining the winner", PlayerDecision.NewGame.Name).Forget();
                return;
            }

            Dictionary<NetworkPlayer, int> playerHandValues = new Dictionary<NetworkPlayer, int>();
            foreach (IPlayerBase playerBase in activePlayers)
            {
                NetworkPlayer networkPlayer = playerBase as NetworkPlayer;
                if (networkPlayer != null)
                {
                    playerHandValues[networkPlayer] = networkPlayer.CalculateHandValue();
                }
            }

            int highestHandValue = int.MinValue;
            foreach (int handValue in playerHandValues.Values)
            {
                if (handValue > highestHandValue)
                {
                    highestHandValue = handValue;
                }
            }

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

            await UniTask.Yield();
        }

        private async UniTask EndRound(List<NetworkPlayer> winners, bool showHand)
        {
            if (!IsServer) return;

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

            if (NetworkScoreManager == null)
            {
                GameLoggerScriptable.LogError("Critical component is null in HandleTie.", this);
                return;
            }

            if (await NetworkScoreManager.AwardTiedPot(winners))
            {
                await OfferContinuation(showHand);
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


            try
            {
                bool awardPotToWinner = await NetworkScoreManager.AwardPotToWinner(winner);

                if (awardPotToWinner)
                {
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

            if (IsFixedRoundsOver())
            {
                ILeaderboard leaderBoard = NetworkScoreManager.GetLeaderBoard();

                if (leaderBoard.HasClearWinner())
                {
                    await EndGame();
                }
                else
                {
                    await OfferContinuation(showHand);
                }
            }
            else
            {
                await OfferContinuation(showHand);
            }
        }
        private async UniTask OfferContinuation(bool showHand)
        {
            if (!IsServer) return;

            NetworkPlayerManager.ShowHand(showHand, true);
            NetworkRoundRecord lastRound = NetworkScoreManager.GetLastRound();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            string serializedLastRound = JsonConvert.SerializeObject(lastRound, settings);

            OfferContinuationClientRpc(showHand, serializedLastRound);
            await UniTask.Yield();
        }


        private async UniTask EndGame()
        {
            if (!IsServer) return;

            NetworkPlayerManager.ShowHand(true);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            List<INetworkRoundRecord> roundRecords = NetworkScoreManager.GetRoundRecord().Cast<INetworkRoundRecord>().ToList();
            string serializedRoundRecords = JsonConvert.SerializeObject(roundRecords, settings);

            (IPlayerBase OverallWinner, int WinCount) overallWinner = NetworkScoreManager.GetOverallWinner();
            string serializedOverallWinner = JsonConvert.SerializeObject(overallWinner, settings);

            EndGameClientRpc(serializedRoundRecords, serializedOverallWinner);
            await UniTask.Yield();
        }

        [ClientRpc]
        private void OfferContinuationClientRpc(bool showHand, string serializedLastRound)
        {
            EventBus.Instance.Publish(new TimerStopEvent());

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            NetworkRoundRecord lastRound = JsonConvert.DeserializeObject<NetworkRoundRecord>(serializedLastRound, settings);

            EventBus.Instance.Publish(new OfferContinuationEvent(10, lastRound));
        }


        [ClientRpc]
        private void EndGameClientRpc(string serializedRoundRecords, string serializedOverallWinner)
        {
            EventBus.Instance.Publish(new TimerStopEvent());

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore

            };

            List<INetworkRoundRecord> roundRecords = JsonConvert.DeserializeObject<List<INetworkRoundRecord>>(serializedRoundRecords, settings);
            (IPlayerBase OverallWinner, int WinCount) overallWinner = JsonConvert.DeserializeObject<(IPlayerBase OverallWinner, int WinCount)>(serializedOverallWinner, settings);

            EventBus.Instance.Publish(new OfferNewGameEvent(60, roundRecords, overallWinner));
        }


        [ClientRpc]
        private void NotifyTimerStartedClientRpc(ulong playerId, int playerIndex)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = new CancellationTokenSource();
            EventBus.Instance.Publish(new TimerStartEvent(playerId, playerIndex, TurnDuration, CancellationTokenSource));
        }


        [ClientRpc]
        private void NotifyTimerStopClientRpc()
        {
            EventBus.Instance.Publish(new TimerStopEvent());
        }


        #endregion


        private void Reset()
        {
            if (IsServer)
            {
                try
                {
                    IsShowdown = false;

                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error during reset: {ex.Message}\n{ex.StackTrace}", this, ToEditor, ToFile, UseStackTrace);
                }
            }

        }
    }
}