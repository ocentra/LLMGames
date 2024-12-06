using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

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

        [ShowInInspector, ReadOnly] private NetworkPlayer CurrentPlayer => NetworkTurnManager.CurrentPlayer as NetworkPlayer;

        #endregion

        public override void SubscribeToEvents()
        {
        }

        public override void UnsubscribeFromEvents()
        {
        }


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

                    }
                    await SetRoundRecords(new List<NetworkRoundRecord>(), false);
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

                    foreach (IPlayerBase playerBase in NetworkPlayerManager.GetAllPlayers())
                    {
                        playerBase.SetCoins(GameMode.InitialPlayerCoins);
                    }

                    await SetPot(0, false);

                    if (GameMode != null)
                    {
                        await SetCurrentBet(GameMode.BaseBet, false);
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
            int pot = GetPot();
            int currentBet = GetCurrentBet();
            int totalRounds = GameMode.MaxRounds;
            int currentRound = NetworkTurnManager.CurrentRound;

            List<INetworkRoundRecord> roundRecords = GetRoundRecord().Cast<INetworkRoundRecord>().ToList();

            bool success = await EventBus.Instance.PublishAsync(
                new UpdateScoreDataEvent<INetworkRoundRecord>(pot, currentBet, totalRounds, currentRound, roundRecords)
            );
            await UniTask.Yield();
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

        int GetPot()
        {
            return Pot.Value;
        }

        public List<NetworkRoundRecord> GetRoundRecord()
        {
            return RoundRecords;
        }

        public async UniTask SetCurrentBet(int value, bool sendEvent = true)
        {
            if (IsServer)
            {
                CurrentBet.Value += value;

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

        public async UniTask<(bool Success, string ErrorMessage)> ProcessBlindBet()
        {
            int newBet = GetCurrentBet() * BlindMultiplier;
            if (!await ValidateBet(newBet))
            {
                return (false, $"Not enough coins ({CurrentPlayer.GetCoins()}). Current bet is {newBet}.");
            }

            if (await ProcessBetOnBlind(newBet))
            {
                BlindMultiplier *= 2;
                return (true, string.Empty);
            }

            return (false, "Error processing blind bet");
        }

        public async UniTask<(bool Success, string ErrorMessage)> ProcessRegularBet()
        {
            int betAmount = CurrentPlayer.HasSeenHand.Value ? GetCurrentBet() * 2 : GetCurrentBet();

            if (!await ValidateBet(betAmount))
            {
                return (false, $"Not enough coins ({CurrentPlayer.GetCoins()}). Current bet is {betAmount}.");
            }

            return await ProcessBet(betAmount) ? (true, string.Empty) : (false, "Error processing bet");
        }

        public async UniTask<(bool Success, string ErrorMessage)> ProcessShowBet()
        {
            int showBetAmount = GetCurrentBet() * 2;

            if (!await ValidateBet(showBetAmount))
            {
                return (false,
                    $"Not enough coins ({CurrentPlayer.GetCoins()}) to show. Required bet is {showBetAmount}.");
            }

            if (await ProcessBet(showBetAmount))
            {
                return (true, string.Empty);
            }

            return (false, "Error processing show bet");
        }

        public async UniTask<(bool Success, string ErrorMessage)> ProcessRaise(int raiseAmount)
        {
            if (!await ValidateBet(raiseAmount))
            {
                return (false, $"Not enough coins ({CurrentPlayer.GetCoins()}). Current bet is {GetCurrentBet()}.");
            }

            return await ProcessBet(raiseAmount) ? (true, string.Empty) : (false, "Error processing raise");
        }

        public async UniTask<bool> ProcessFold()
        {
            if (IsServer)
            {
                if (!VerifyTotalCoins())
                {
                    HandleVerificationFailure("ProcessFold - Before");
                    return false;
                }

                CurrentPlayer.HasFolded.Value = true;
                NetworkPlayerManager.AddFoldedPlayer(CurrentPlayer);

                if (NetworkPlayerManager.GetActivePlayers().Count == 1)
                {
                    IPlayerBase winner = NetworkPlayerManager.GetActivePlayers().First();
                    return await AwardPotToWinner(winner as NetworkPlayer);
                }

                return VerifyTotalCoins();
            }

            return false;
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
                await SetPot(0);

                if (!VerifyTotalCoins())
                {
                    HandleVerificationFailure("AwardPotToWinner - After");
                    return false;
                }

                await RecordRound(winner, potAmount);
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

            await SetPot(0);

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardTiedPot - After");
                return false;
            }

            await RecordRound(null, GetPot());
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

        public (ulong WinnerId, int WinCount) GetOverallWinner()
        {
            var grouped = GetRoundRecord()
                .GroupBy(r => r.WinnerId)
                .Select(g => new { WinnerId = g.Key, WinCount = g.Count() })
                .OrderByDescending(g => g.WinCount)
                .FirstOrDefault();

            if (grouped != null)
            {
                return (grouped.WinnerId, grouped?.WinCount ?? 0);
            }

            return (default, 0);
        }

        public List<(ulong PlayerId, int Wins, int TotalWinnings)> GetLeaderboard()
        {
            return GetRoundRecord()
                .GroupBy(r => r.WinnerId)
                .Select(g => (PlayerId: g.Key, Wins: g.Count(), TotalWinnings: g.Sum(r => r.PotAmount)))
                .OrderByDescending(p => p.Wins)
                .ThenByDescending(p => p.TotalWinnings)
                .ToList();
        }

        #endregion


        #region Private Helper Methods

        private async UniTask RecordRound(IPlayerBase winner, int potAmount)
        {
            IReadOnlyList<IPlayerBase> readOnlyList = NetworkPlayerManager.GetAllPlayers();
            NetworkRoundRecord networkRoundRecord = new NetworkRoundRecord
            {
                RoundNumber = NetworkTurnManager.CurrentRound,
                WinnerId = winner.PlayerId.Value,
                PotAmount = potAmount,
                PlayerRecords = readOnlyList.Select(player =>
                {

                    INetworkPlayerRecord networkPlayerRecord = new NetworkPlayerRecord(player);

                    return networkPlayerRecord;
                }).ToList()
            };

            List<NetworkRoundRecord> roundRecords = new List<NetworkRoundRecord>();

            if (!roundRecords.Contains(networkRoundRecord))
            {
                roundRecords.Add(networkRoundRecord);
            }


            await SetRoundRecords(roundRecords);
            await UniTask.Yield();
        }



        // will come from UI so should be Event Todo
        public NetworkRoundRecord GetLastRound()
        {
            return GetRoundRecord().Last();
        }

        public IPlayerBase GetLastRoundWinner()
        {
            NetworkRoundRecord networkRoundRecord = GetRoundRecord().Last();

            if (networkRoundRecord != null && NetworkPlayerManager.TryGetPlayer(networkRoundRecord.WinnerId, out IPlayerBase playerBase))
            {
                return playerBase;
            }

            return null;
        }

        private async UniTask<bool> ValidateBet(int betAmount)
        {
            bool canAffordBet = CurrentPlayer != null && CurrentPlayer.CanAffordBet(betAmount);
            bool validateBet = canAffordBet && GetPot() + betAmount <= GetTotalCoinsInPlay();
            await UniTask.Yield();
            return validateBet;
        }

        private async UniTask<bool> ProcessBet(int betAmount)
        {
            if (CurrentPlayer != null)
            {
                if (!await ValidateBet(betAmount))
                {

                    GameLoggerScriptable.LogError($"Invalid bet: Player {CurrentPlayer.PlayerId.Value}, Amount: {betAmount}, Player Coins: {CurrentPlayer.GetCoins()}", this);


                    return false;
                }

                if (CurrentPlayer.CanAffordBet(betAmount))
                {
                    CurrentPlayer.AdjustCoins(-betAmount);
                }


                await SetPot(betAmount);
                await SetCurrentBet(betAmount);
                bool verificationResult = VerifyTotalCoins();
                return verificationResult;

            }

            return false;
        }

        private async UniTask<bool> ProcessBetOnBlind(int betAmount)
        {
            if (!await ValidateBet(betAmount))
            {
                return false;
            }

            if (CurrentPlayer.CanAffordBet(betAmount))
            {
                CurrentPlayer.AdjustCoins(-betAmount);
                CurrentPlayer.HasBetOnBlind.Value = true;
            }

            await SetPot(betAmount);
            await SetCurrentBet(betAmount);

            return VerifyTotalCoins();
        }

        private bool VerifyTotalCoins()
        {
            int currentTotal = GetPot();

            foreach (IPlayerBase player in NetworkPlayerManager.GetAllPlayers())
            {

                if (CurrentPlayer != null)
                {
                    currentTotal += CurrentPlayer.GetCoins();
                }
            }

            bool isValid = currentTotal == GetTotalCoinsInPlay();
            if (!isValid)
            {
                GameLoggerScriptable.LogError($"Total coins mismatch. Current: {currentTotal}, Expected: {GetTotalCoinsInPlay()}", this);
            }

            return isValid;
        }

        private void HandleVerificationFailure(string methodName)
        {
            GameLoggerScriptable.LogError($"Total coins verification failed in {methodName}. Game state might be corrupted.", this);
        }

        #endregion


    }
}