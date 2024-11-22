using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Players;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager
{
    public class ScoreManager : ManagerBase<ScoreManager>
    {
        #region Properties

        [ShowInInspector] [ReadOnly] public int Pot { get; private set; }
        [ShowInInspector] [ReadOnly] public int CurrentBet { get; private set; }

        [ShowInInspector] [ReadOnly] private int BlindMultiplier { get; set; }

        [ShowInInspector] [ReadOnly] private int TotalCoinsInPlay { get; set; }

        [ShowInInspector] [ReadOnly] private List<RoundRecord> RoundRecords { get; set; } = new List<RoundRecord>();

        

        #endregion

        #region Initialization

        public async UniTask<bool> ResetForNewGame(GameMode gameMode, PlayerManager playerManager)
        {
            if (gameMode == null)
            {
                LogError("GameMode is null! Cannot proceed with ResetForNewGame.", this);
                return false;
            }

            if (playerManager == null)
            {
                LogError("PlayerManager is null! Cannot proceed with ResetForNewGame.", this);
                return false;
            }

            await UniTask.Yield();

            try
            {
                TotalCoinsInPlay = gameMode.InitialPlayerCoins * playerManager.GetActivePlayers().Count;
                Pot = 0;
                CurrentBet = gameMode.BaseBet;
                BlindMultiplier = gameMode.BaseBlindMultiplier;
                RoundRecords.Clear();

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewGame: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        public async UniTask<bool> ResetForNewRound(GameMode gameMode)
        {
            if (gameMode == null)
            {
                LogError("GameMode is null! Cannot proceed with ResetForNewRound.", this);
                return false;
            }

            await UniTask.Yield();

            try
            {
                Pot = 0;
                CurrentBet = gameMode.BaseBet;
                BlindMultiplier = gameMode.BaseBlindMultiplier;

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewRound: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }


        #endregion

        #region Betting Operations

        public (bool Success, string ErrorMessage) ProcessBlindBet(TurnManager turnManager, PlayerManager playerManager)
        {
            int newBet = CurrentBet * BlindMultiplier;
            if (!ValidateBet(newBet, turnManager))
            {
                return (false, $"Not enough coins ({turnManager.CurrentLLMPlayer.Coins}). Current bet is {newBet}.");
            }

            if (ProcessBetOnBlind(newBet, turnManager, playerManager))
            {
                BlindMultiplier *= 2;
                return (true, string.Empty);
            }

            return (false, "Error processing blind bet");
        }

        public (bool Success, string ErrorMessage) ProcessRegularBet(TurnManager turnManager, PlayerManager playerManager)
        {
            int betAmount = turnManager.CurrentLLMPlayer.HasSeenHand ? CurrentBet * 2 : CurrentBet;

            if (!ValidateBet(betAmount, turnManager))
            {
                return (false, $"Not enough coins ({turnManager.CurrentLLMPlayer.Coins}). Current bet is {betAmount}.");
            }

            return ProcessBet(betAmount, turnManager, playerManager) ? (true, string.Empty) : (false, "Error processing bet");
        }

        public (bool Success, string ErrorMessage) ProcessShowBet(TurnManager turnManager, PlayerManager playerManager)
        {
            int showBetAmount = CurrentBet * 2;

            if (!ValidateBet(showBetAmount, turnManager))
            {
                return (false,
                    $"Not enough coins ({turnManager.CurrentLLMPlayer.Coins}) to show. Required bet is {showBetAmount}.");
            }

            if (ProcessBet(showBetAmount, turnManager, playerManager))
            {
                return (true, string.Empty);
            }

            return (false, "Error processing show bet");
        }

        public (bool Success, string ErrorMessage) ProcessRaise(int raiseAmount, TurnManager turnManager, PlayerManager playerManager)
        {
            if (!ValidateBet(raiseAmount, turnManager))
            {
                return (false, $"Not enough coins ({turnManager.CurrentLLMPlayer.Coins}). Current bet is {CurrentBet}.");
            }

            return ProcessBet(raiseAmount, turnManager, playerManager) ? (true, string.Empty) : (false, "Error processing raise");
        }

        public bool ProcessFold(PlayerManager playerManager, TurnManager turnManager)
        {
            if (!VerifyTotalCoins(playerManager))
            {
                HandleVerificationFailure("ProcessFold - Before");
                return false;
            }

            playerManager.FoldPlayer(turnManager);

            if (playerManager.GetActivePlayers().Count == 1)
            {
                LLMPlayer winner = playerManager.GetActivePlayers().First();
                return AwardPotToWinner(winner, turnManager, playerManager);
            }

            return VerifyTotalCoins(playerManager);
        }

        #endregion

        #region Player Scoring

        public bool AwardPotToWinner(LLMPlayer winner, TurnManager turnManager, PlayerManager playerManager)
        {
            if (!VerifyTotalCoins(playerManager))
            {
                HandleVerificationFailure("AwardPotToWinner - Before");
                return false;
            }

            int potAmount = Pot;
            winner.AdjustCoins(potAmount);
            Pot = 0;

            if (!VerifyTotalCoins(playerManager))
            {
                HandleVerificationFailure("AwardPotToWinner - After");
                return false;
            }

            RecordRound(winner, potAmount, turnManager, playerManager);
            return true;
        }

        public bool AwardTiedPot(List<LLMPlayer> tiedPlayers, TurnManager turnManager, PlayerManager playerManager)
        {
            if (!VerifyTotalCoins(playerManager))
            {
                HandleVerificationFailure("AwardTiedPot - Before");
                return false;
            }

            int splitAmount = Pot / tiedPlayers.Count;
            foreach (LLMPlayer player in tiedPlayers)
            {
                player.AdjustCoins(splitAmount);
            }

            int remainder = Pot % tiedPlayers.Count;
            if (remainder > 0)
            {
                tiedPlayers[0].AdjustCoins(remainder);
            }

            Pot = 0;

            if (!VerifyTotalCoins(playerManager))
            {
                HandleVerificationFailure("AwardTiedPot - After");
                return false;
            }

            RecordRound(null, Pot, turnManager, playerManager);
            return true;
        }

        public int GetPlayerWins(string playerId)
        {
            return RoundRecords.Count(r => r.WinnerId == playerId);
        }

        public int GetPlayerTotalWinnings(string playerId)
        {
            return RoundRecords
                .Where(r => r.WinnerId == playerId)
                .Sum(r => r.PotAmount);
        }

        public (string WinnerId, int WinCount) GetOverallWinner()
        {
            var grouped = RoundRecords
                .GroupBy(r => r.WinnerId)
                .Select(g => new {WinnerId = g.Key, WinCount = g.Count()})
                .OrderByDescending(g => g.WinCount)
                .FirstOrDefault();

            return (grouped?.WinnerId, grouped?.WinCount ?? 0);
        }

        public List<(string PlayerId, int Wins, int TotalWinnings)> GetLeaderboard()
        {
            return RoundRecords
                .GroupBy(r => r.WinnerId)
                .Select(g => (PlayerId: g.Key, Wins: g.Count(), TotalWinnings: g.Sum(r => r.PotAmount)))
                .OrderByDescending(p => p.Wins)
                .ThenByDescending(p => p.TotalWinnings)
                .ToList();
        }

        #endregion

        #region Private Helper Methods

        private void RecordRound(LLMPlayer winner, int potAmount, TurnManager turnManager, PlayerManager playerManager)
        {
            RoundRecord roundRecord = new RoundRecord
            {
                RoundNumber = turnManager.CurrentRound,
                WinnerId = winner?.AuthPlayerData.PlayerID,
                PotAmount = potAmount,
                PlayerRecords = playerManager.GetAllPlayers().Select(player => new PlayerRecord(player)).ToList()
            };

            if (!RoundRecords.Contains(roundRecord))
            {
                RoundRecords.Add(roundRecord);
            }
        }


        public List<RoundRecord> GetRoundRecords()
        {
            return RoundRecords;
        }

        public RoundRecord GetLastRound()
        {
            return RoundRecords.Last();
        }

        public LLMPlayer GetLastRoundWinner(PlayerManager playerManager)
        {
            RoundRecord roundRecord = RoundRecords.Last();
            return roundRecord != null ? playerManager.GetPlayerById(roundRecord.WinnerId) : null;
        }

        private bool ValidateBet(int betAmount, TurnManager turnManager)
        {
            return turnManager.CurrentLLMPlayer.CanAffordBet(betAmount) && Pot + betAmount <= TotalCoinsInPlay;
        }

        private bool ProcessBet(int betAmount, TurnManager turnManager, PlayerManager playerManager)
        {
            if (!ValidateBet(betAmount, turnManager))
            {
                Debug.LogError(
                    $"Invalid bet: Player {turnManager.CurrentLLMPlayer.AuthPlayerData.PlayerID}, Amount: {betAmount}, Player Coins: {turnManager.CurrentLLMPlayer.Coins}");
                return false;
            }

            turnManager.CurrentLLMPlayer.Bet(betAmount);
            Pot += betAmount;
            CurrentBet = betAmount;
            bool verificationResult = VerifyTotalCoins(playerManager);
            return verificationResult;
        }

        private bool ProcessBetOnBlind(int betAmount, TurnManager turnManager, PlayerManager playerManager)
        {
            if (!ValidateBet(betAmount, turnManager))
            {
                return false;
            }

            turnManager.CurrentLLMPlayer.BetOnBlind(betAmount);
            Pot += betAmount;
            CurrentBet = betAmount;
            return VerifyTotalCoins(playerManager);
        }

        private bool VerifyTotalCoins(PlayerManager playerManager)
        {
            int currentTotal = Pot;
            foreach (LLMPlayer player in playerManager.GetAllPlayers())
            {
                currentTotal += player.Coins;
            }

            bool isValid = currentTotal == TotalCoinsInPlay;
            if (!isValid)
            {
                Debug.LogError($"Total coins mismatch. Current: {currentTotal}, Expected: {TotalCoinsInPlay}");
            }

            return isValid;
        }

        private void HandleVerificationFailure(string methodName)
        {
            Debug.LogError($"Total coins verification failed in {methodName}. Game state might be corrupted.");
        }

        #endregion
    }
}