using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Leaderboard = OcentraAI.LLMGames.GamesNetworking.Leaderboard;

namespace OcentraAI.LLMGames.Networking.Manager
{
    public class NetworkScoreManager : NetworkManagerBase
    {
        #region Properties

        [ShowInInspector, ReadOnly] public NetworkVariable<int> Pot { get; private set; } = new NetworkVariable<int>();

        [ShowInInspector, ReadOnly] public NetworkVariable<int> CurrentBet { get; private set; } = new NetworkVariable<int>();

        [ShowInInspector, ReadOnly] private int BlindMultiplier { get; set; }

        [ShowInInspector, ReadOnly] public NetworkVariable<int> TotalCoinsInPlay { get; private set; } = new NetworkVariable<int>();

        [ShowInInspector, ReadOnly] private List<NetworkRoundRecord> RoundRecords { get; set; } = new List<NetworkRoundRecord>();


        #endregion

        

        #region Initialization

        public async UniTask<bool> ResetForNewGame()
        {
            if (IsServer)
            {
                try
                {
                    if (GameMode != null)
                    {
                        await SetTotalCoinsInPlay(GameMode.InitialPlayerCoins * NetworkPlayerManager.GetActivePlayers().Count, false);

                        foreach (IPlayerBase playerBase in NetworkPlayerManager.GetActivePlayers())
                        {
                            playerBase.SetCoins(GameMode.InitialPlayerCoins);
                        }

                    }

                    RoundRecords = new List<NetworkRoundRecord>();
                    NetworkPlayerManager.ResetFoldedPlayer(false);
                    await UniTask.Yield();
                    return true;
                }
                catch (Exception ex)
                {
                    GameLoggerScriptable.LogError($"Error in ResetForNewGame: {ex.Message}\n{ex.StackTrace}", this);
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
                    NetworkPlayerManager.ResetFoldedPlayer();
                    
                    await ResetPot(false);

                    if (GameMode != null)
                    {
                        await SetCurrentBet(GameMode.BaseBet,true, false);
                        BlindMultiplier = GameMode.BaseBlindMultiplier;
                    }
                    await SendUIUpdateEvent();
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

        public async UniTask SendUIUpdateEvent()
        {
            if (IsServer)
            {
                int pot = GetPot();
                int currentBet = GetCurrentBet();
                int totalRounds = GameMode.MaxRounds;
                int currentRound = NetworkTurnManager.CurrentRound;
                List<NetworkRoundRecord> roundRecords = GetRoundRecord();

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string serializedRoundRecords = JsonConvert.SerializeObject(roundRecords, settings);
                SendUIUpdateEventClientRpc(serializedRoundRecords, pot, currentBet, totalRounds, currentRound);
            }

            await UniTask.Yield();
        }

        [ClientRpc]
        private void SendUIUpdateEventClientRpc(string serializedRoundRecords, int pot, int currentBet, int totalRounds, int currentRound)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            List<NetworkRoundRecord> deserializeObject = JsonConvert.DeserializeObject<List<NetworkRoundRecord>>(serializedRoundRecords, settings);

            List<INetworkRoundRecord> networkRoundRecords = deserializeObject.Cast<INetworkRoundRecord>().ToList();

            UpdateScoreDataEvent<INetworkRoundRecord> interfaceEvent = new UpdateScoreDataEvent<INetworkRoundRecord>(
                pot,
                currentBet,
                totalRounds,
                currentRound,
                networkRoundRecords);

            EventBus.Instance.Publish(interfaceEvent);
        }




        #endregion

        public async UniTask SetTotalCoinsInPlay(int totalCoinsInPlay, bool sendEvent = true)
        {
            if (IsServer)
            {
                TotalCoinsInPlay.Value += totalCoinsInPlay;
                if (sendEvent)
                {
                    await SendUIUpdateEvent();
                }
            }

            await UniTask.Yield();
        }

        int GetTotalCoinsInPlay()
        {
            return TotalCoinsInPlay.Value;
        }

        public async UniTask SetPot(int value, bool sendEvent = true)
        {
            if (IsServer)
            {
                Pot.Value += value;
                if (sendEvent)
                {
                    await SendUIUpdateEvent();
                }
            }

            await UniTask.Yield();
        }

        public async UniTask ResetPot(bool sendEvent = true)
        {
            if (IsServer)
            {
                Pot.Value = 0;
                if (sendEvent)
                {
                    await SendUIUpdateEvent();
                }
            }

            await UniTask.Yield();
        }

        int GetPot()
        {
            return Pot.Value;
        }

        public List<NetworkRoundRecord> GetRoundRecord()
        {
            return RoundRecords;
        }

        public async UniTask SetCurrentBet(int value, bool resetBet = false ,bool sendEvent = true)
        {
            if (IsServer)
            {
                if (resetBet)
                {
                    CurrentBet.Value = value;
                }
                else
                {
                    CurrentBet.Value += value;
                }
               

                if (sendEvent)
                {
                    await SendUIUpdateEvent();
                }

            }

            await UniTask.Yield();
        }

        int GetCurrentBet()
        {
            return CurrentBet.Value;
        }

        public async UniTask SetRoundRecords(List<NetworkRoundRecord> roundRecords, bool sendEvent = true)
        {
            RoundRecords = roundRecords;
            if (sendEvent)
            {
                await SendUIUpdateEvent();
            }
            await UniTask.Yield();
        }

        #region Betting Operations


        public async UniTask<(bool Success, string ErrorMessage)> HandlePlayBlind(NetworkPlayer currentPlayer)
        {
            if (!IsServer) return (false, "Unauthorized!");

            int newBet = GetCurrentBet() * BlindMultiplier;
            string failureMessage = $"Not enough coins ({currentPlayer.GetCoins()}). Current bet is {newBet}.";

            (bool success, string errorMessage) = await ProcessBetWithValidation(newBet, failureMessage, currentPlayer);
            if (success)
            {
                currentPlayer.HasBetOnBlind.Value = true;
                BlindMultiplier *= 2;
            }

            return (success, errorMessage);
        }

        public async UniTask<(bool Success, string ErrorMessage)> HandleBet(NetworkPlayer currentPlayer)
        {
            if (!IsServer) return (false, "Unauthorized!");

            int betAmount = currentPlayer.HasSeenHand.Value ? GetCurrentBet() * 2 : GetCurrentBet();
            return await ProcessBetWithValidation(betAmount, $"Not enough coins ({currentPlayer.GetCoins()}). Current bet is {betAmount}.", currentPlayer);
        }

        public async UniTask<(bool Success, string ErrorMessage)> HandleShowCall(NetworkPlayer currentPlayer)
        {
            if (!IsServer) return (false, "Unauthorized!");

            int showBetAmount = GetCurrentBet() * 2;
            return await ProcessBetWithValidation(showBetAmount, $"Not enough coins ({currentPlayer.GetCoins()}) to show. Required bet is {showBetAmount}.", currentPlayer);
        }

        public async UniTask<(bool Success, string ErrorMessage)> HandleRaiseBet(int raiseAmount, NetworkPlayer currentPlayer)
        {
            if (!IsServer) return (false, "Unauthorized!");

            return await ProcessBetWithValidation(raiseAmount, $"Not enough coins ({currentPlayer.GetCoins()}). Current bet is {GetCurrentBet()}.", currentPlayer);
        }



        public async UniTask<(bool Success, string ErrorMessage)> HandleShowAllFloorCards(NetworkPlayer currentPlayer)
        {
            if (!IsServer) return (false, "Unauthorized!");

            int newBet = 10;
            string failureMessage = $"Not enough coins ({currentPlayer.GetCoins()}). Cost To ShowAllFloorCards is {newBet}.";
            return await ProcessBetWithValidation(newBet, failureMessage, currentPlayer, false);
        }
        private async UniTask<(bool Success, string ErrorMessage)> ProcessBetWithValidation(int betAmount, string failureMessage, NetworkPlayer currentPlayer, bool showFoldMessage = true)
        {
            if (!IsServer) return (false, "Unauthorized!");

            if (!await ValidateBet(currentPlayer,betAmount))
            {
                if (showFoldMessage)
                {
                    await HandleFold(failureMessage, currentPlayer);
                }

                return (false, failureMessage);
            }

            return await ProcessBet(currentPlayer,betAmount, failureMessage) ? (true, string.Empty) : (false, "Error processing bet");
        }

        private async UniTask<bool> ProcessBet(NetworkPlayer currentPlayer,int betAmount, string failureMessage)
        {
            if (!IsServer) return false;

            if (currentPlayer == null || !currentPlayer.CanAffordBet(betAmount))
            {
                await HandleFold(failureMessage, currentPlayer);
                return false;
            }

            currentPlayer.AdjustCoins(-betAmount);
            await SetPot(betAmount);
            await SetCurrentBet(betAmount);
            return VerifyTotalCoins();
        }

        public async UniTask<bool> HandleFold(string failureMessage, NetworkPlayer currentPlayer, bool fromUI = false)
        {
            if (!IsServer) return false;

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("ProcessFold - Before");
                return false;
            }

            if (currentPlayer is IHumanPlayerData && !fromUI)
            {
                await ShowMessage(failureMessage, PlayerDecision.Fold.Name);

                return false;
            }

            currentPlayer.HasFolded.Value = true;
            NetworkPlayerManager.AddFoldedPlayer(currentPlayer);

            IReadOnlyList<IPlayerBase> activePlayers = NetworkPlayerManager.GetActivePlayers();

            if (activePlayers.Count == 1)
            {
                IPlayerBase winner = activePlayers.First();
                return await AwardPotToWinner(winner as NetworkPlayer);
            }

            return VerifyTotalCoins();
        }

        #endregion



        #region Player Scoring

        public async UniTask<bool> AwardPotToWinner(NetworkPlayer winner)
        {
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardPotToWinner - Before");
                return false;
            }

            int potAmount = GetPot();
            if (winner != null)
            {
                winner.AdjustCoins(potAmount);
                await RecordRound(winner, potAmount);

                await ResetPot();

                if (!VerifyTotalCoins())
                {
                    HandleVerificationFailure("AwardPotToWinner - After");
                    return false;
                }

            }

            return true;
        }

        public async UniTask<bool> AwardTiedPot(List<NetworkPlayer> tiedPlayers)
        {
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardTiedPot - Before");
                return false;
            }

            int splitAmount = GetPot() / tiedPlayers.Count;
            foreach (NetworkPlayer player in tiedPlayers)
            {
                if (player != null)
                {
                    player.AdjustCoins(splitAmount);
                }
            }

            int remainder = GetPot() % tiedPlayers.Count;
            if (remainder > 0)
            {
                tiedPlayers[0].AdjustCoins(remainder);
            }

            await RecordRound(null, GetPot());

            await ResetPot();
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardTiedPot - After");
                return false;
            }


            return true;
        }

        public int GetPlayerWins(ulong playerId)
        {
            return GetRoundRecord().Count(r => r.WinnerId == playerId);
        }

        public int GetPlayerTotalWinnings(ulong playerId)
        {
            return GetRoundRecord()
                .Where(r => r.WinnerId == playerId)
                .Sum(r => r.PotAmount);
        }

        public (IPlayerBase OverallWinner, int WinCount) GetOverallWinner()
        {
            Dictionary<ulong, int> winCounts = new Dictionary<ulong, int>();

            foreach (NetworkRoundRecord record in RoundRecords)
            {
                if (winCounts.ContainsKey(record.WinnerId))
                {
                    winCounts[record.WinnerId]++;
                }
                else
                {
                    winCounts[record.WinnerId] = 1;
                }
            }

            ulong overallWinnerId = 0;
            int maxWinCount = 0;

            foreach (KeyValuePair<ulong, int> entry in winCounts)
            {
                if (entry.Value > maxWinCount)
                {
                    overallWinnerId = entry.Key;
                    maxWinCount = entry.Value;
                }
            }

            IPlayerBase overallWinner = null;
            if (overallWinnerId != 0)
            {
                NetworkPlayerManager.TryGetPlayer(overallWinnerId, out overallWinner);
            }

            return (overallWinner, maxWinCount);
        }


        public ILeaderboard GetLeaderBoard()
        {
            List<INetworkRoundRecord> roundRecords = new List<INetworkRoundRecord>(GetRoundRecord());
            ILeaderboard leaderboard = new Leaderboard(roundRecords);
            return leaderboard;
        }



        #endregion


        #region Private Helper Methods

        private async UniTask RecordRound(IPlayerBase winner, int potAmount)
        {
            IReadOnlyList<IPlayerBase> allPlayers = NetworkPlayerManager.GetAllPlayers();

            List<INetworkPlayerRecord> playerRecords = new List<INetworkPlayerRecord>();
           
            foreach (IPlayerBase player in allPlayers)
            {
                if (player != null)
                {
                    try
                    {
                        NetworkPlayerRecord playerRecord = new NetworkPlayerRecord(player);
                        if (!playerRecords.Contains(playerRecord))
                        {
                            playerRecords.Add(playerRecord);
                        }
                    }
                    catch (Exception ex)
                    {
                        GameLoggerScriptable.LogError($"Failed to create NetworkPlayerRecord for player: {ex.Message}", this);
                    }
                }
                else
                {
                    GameLoggerScriptable.LogError("Encountered null player while recording round.", this);
                }
            }

            ulong winnerId = winner?.PlayerId?.Value ?? default;
            string winnerName = winner?.PlayerName?.Value.Value ?? "Tie";

            NetworkRoundRecord networkRoundRecord = new NetworkRoundRecord
            {
                RoundNumber = NetworkTurnManager.CurrentRound,
                MaxRounds = NetworkTurnManager.MaxRounds,
                WinnerId = winnerId,
                Winner = winnerName,
                PotAmount = potAmount,
                PlayerRecords = playerRecords
            };

            RoundRecords ??= new List<NetworkRoundRecord>();

            if (!RoundRecords.Contains(networkRoundRecord))
            {
                RoundRecords.Add(networkRoundRecord);
            }

            await SetRoundRecords(RoundRecords);
            await UniTask.Yield();
        }





        public NetworkRoundRecord GetLastRound()
        {
            List<NetworkRoundRecord> networkRoundRecords = GetRoundRecord();
            NetworkRoundRecord networkRoundRecord = networkRoundRecords.Last();
            return networkRoundRecord;
        }

        public IPlayerBase GetLastRoundWinner()
        {
            List<NetworkRoundRecord> networkRoundRecords = GetRoundRecord();
            NetworkRoundRecord networkRoundRecord = networkRoundRecords.Last();

            if (networkRoundRecord != null && NetworkPlayerManager.TryGetPlayer(networkRoundRecord.WinnerId, out IPlayerBase playerBase))
            {
                return playerBase;
            }

            return null;
        }

        private async UniTask<bool> ValidateBet(NetworkPlayer currentPlayer,int betAmount)
        {
            bool canAffordBet = currentPlayer != null && currentPlayer.CanAffordBet(betAmount);
            int currentTotal = GetPot();
            int totalCoinsInPlay = GetTotalCoinsInPlay();

            bool validateBet = canAffordBet && currentTotal + betAmount <= totalCoinsInPlay;
            await UniTask.Yield();
            return validateBet;
        }

        private bool VerifyTotalCoins()
        {
            int currentTotal = GetPot();

            IEnumerable<IPlayerBase> allPlayers = NetworkPlayerManager.GetAllPlayers();

            foreach (IPlayerBase player in allPlayers)
            {

                if (player != null)
                {
                    currentTotal += player.GetCoins();
                }
            }
            int totalCoinsInPlay = GetTotalCoinsInPlay();
            bool isValid = currentTotal == totalCoinsInPlay;
            if (!isValid)
            {
                GameLoggerScriptable.LogError($"Total coins mismatch. Current: {currentTotal}, Expected: {GetTotalCoinsInPlay()}", this);
            }

            return isValid;
        }

        private void HandleVerificationFailure(string methodName)
        {
            GameLoggerScriptable.LogError($"Total coins verification failed in {methodName}. Game state might be corrupted.", this, true, true);

        }

        #endregion


    }
}