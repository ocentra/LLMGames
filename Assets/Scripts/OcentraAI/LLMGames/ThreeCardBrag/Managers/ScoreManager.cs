using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public class ScoreManager
    {
        #region Properties

        [ShowInInspector] public int InitialCoins { get; private set; } = 1000;
        [ShowInInspector, ReadOnly] public int CurrentBet { get; private set; }
        [ShowInInspector, ReadOnly] public int BlindMultiplier { get; private set; } = 1;
        [ShowInInspector, ReadOnly] private int BaseBet { get; set; } = 10;
        [ShowInInspector, ReadOnly] private int TotalCoinsInPlay { get; set; }
        [ShowInInspector, ReadOnly] public int Pot { get; private set; }
        [ShowInInspector, ReadOnly] private List<RoundRecord> RoundRecords { get; set; } = new List<RoundRecord>();


        private PlayerManager PlayerManager => GameManager.Instance.PlayerManager;
        private TurnManager TurnManager => GameManager.Instance.TurnManager;


        #endregion

        #region Initialization

        public ScoreManager() { }

        public void Init() { }

        public void ResetForNewGame()
        {
            TotalCoinsInPlay = InitialCoins * PlayerManager.GetActivePlayers().Count;
            Pot = 0;
            CurrentBet = BaseBet;
            BlindMultiplier = 1;
            RoundRecords.Clear();
        }

        public void ResetForNewRound()
        {
            Pot = 0;
            CurrentBet = BaseBet;
            BlindMultiplier = 1;
        }

        #endregion

        #region Betting Operations

        public (bool Success, string ErrorMessage) ProcessBlindBet()
        {
            int newBet = CurrentBet * BlindMultiplier;
            if (!ValidateBet(newBet))
            {
                return (false, $"Not enough coins ({TurnManager.CurrentPlayer.Coins}). Current bet is {newBet}.");
            }

            if (ProcessBetOnBlind(newBet))
            {
                BlindMultiplier *= 2;
                return (true, string.Empty);
            }
            else
            {
                return (false, "Error processing blind bet");
            }
        }

        public (bool Success, string ErrorMessage) ProcessRegularBet()
        {
            int betAmount = TurnManager.CurrentPlayer.HasSeenHand ? CurrentBet * 2 : CurrentBet;

            if (!ValidateBet(betAmount))
            {
                return (false, $"Not enough coins ({TurnManager.CurrentPlayer.Coins}). Current bet is {betAmount}.");
            }

            if (ProcessBet(betAmount))
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, "Error processing bet");
            }
        }

        public (bool Success, string ErrorMessage) ProcessShowBet()
        {
            int showBetAmount = CurrentBet * 2;

            if (!ValidateBet(showBetAmount))
            {
                return (false, $"Not enough coins ({TurnManager.CurrentPlayer.Coins}) to show. Required bet is {showBetAmount}.");
            }

            if (ProcessBet(showBetAmount))
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, "Error processing show bet");
            }
        }

        public (bool Success, string ErrorMessage) ProcessRaise(int raiseAmount)
        {
            if (!ValidateBet(raiseAmount))
            {
                return (false, $"Not enough coins ({TurnManager.CurrentPlayer.Coins}). Current bet is {CurrentBet}.");
            }

            if (ProcessBet(raiseAmount))
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, "Error processing raise");
            }
        }

        public bool ProcessFold()
        {
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("ProcessFold - Before");
                return false;
            }

            PlayerManager.FoldPlayer();

            if (PlayerManager.GetActivePlayers().Count == 1)
            {
                Player winner = PlayerManager.GetActivePlayers().First();
                return AwardPotToWinner(winner);
            }

            return VerifyTotalCoins();
        }

        #endregion

        #region Player Scoring

        public bool AwardPotToWinner(Player winner)
        {
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardPotToWinner - Before");
                return false;
            }

            int potAmount = Pot;
            winner.AdjustCoins(potAmount);
            Pot = 0;

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardPotToWinner - After");
                return false;
            }

            RecordRound(winner.Id, potAmount);
            return true;
        }

        public bool AwardTiedPot(List<Player> tiedPlayers)
        {
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardTiedPot - Before");
                return false;
            }

            int splitAmount = Pot / tiedPlayers.Count;
            foreach (var player in tiedPlayers)
            {
                player.AdjustCoins(splitAmount);
            }

            int remainder = Pot % tiedPlayers.Count;
            if (remainder > 0)
            {
                tiedPlayers[0].AdjustCoins(remainder);
            }

            Pot = 0;

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardTiedPot - After");
                return false;
            }

            RecordRound(null, Pot);
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
                .Select(g => new { WinnerId = g.Key, WinCount = g.Count() })
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

        private void RecordRound(string winnerId, int potAmount)
        {
            var roundRecord = new RoundRecord
            {
                RoundNumber = TurnManager.CurrentRound,
                WinnerId = winnerId,
                PotAmount = potAmount,
                Players = PlayerManager.GetAllPlayers()

            };

            RoundRecords.Add(roundRecord);
        }

        public List<RoundRecord> GetRoundRecords()
        {
            return RoundRecords;
        }

        public RoundRecord GetLastRound()
        {
            return RoundRecords.Last();
        }

        private bool ValidateBet(int betAmount)
        {
            return TurnManager.CurrentPlayer.CanAffordBet(betAmount) && (Pot + betAmount <= TotalCoinsInPlay);
        }

        private bool ProcessBet(int betAmount)
        {
            if (!ValidateBet(betAmount))
            {
                Debug.LogError($"Invalid bet: Player {TurnManager.CurrentPlayer.Id}, Amount: {betAmount}, Player Coins: {TurnManager.CurrentPlayer.Coins}");
                return false;
            }
            TurnManager.CurrentPlayer.Bet(betAmount);
            Pot += betAmount;
            CurrentBet = betAmount;
            bool verificationResult = VerifyTotalCoins();
            return verificationResult;
        }

        private bool ProcessBetOnBlind(int betAmount)
        {
            if (!ValidateBet(betAmount)) return false;
            TurnManager.CurrentPlayer.BetOnBlind(betAmount);
            Pot += betAmount;
            CurrentBet = betAmount;
            return VerifyTotalCoins();
        }

        private bool VerifyTotalCoins()
        {
            int currentTotal = Pot;
            foreach (var player in PlayerManager.GetAllPlayers())
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
    [System.Serializable]
    public class RoundRecord
    {
        [ShowInInspector, ReadOnly] public int RoundNumber { get; set; }
        [ShowInInspector, ReadOnly] public List<Player> Players { get; set; } = new List<Player>();
        [ShowInInspector, ReadOnly] public string WinnerId { get; set; }
        [ShowInInspector, ReadOnly] public int PotAmount { get; set; }


    }
}