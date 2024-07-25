using OcentraAI.LLMGames.Authentication;
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
        [ShowInInspector] private int TotalCoinsInPlay { get; set; }
        [ShowInInspector] public int Pot { get; private set; }

        public int HumanTotalWins => GetPlayerWins(AuthenticationManager.Instance.PlayerData.PlayerID);
        public int ComputerTotalWins => GetPlayerWins(PlayerManager.GetComputerPlayer().Id);

        private Dictionary<string, int> PlayerWins { get; set; } = new Dictionary<string, int>();
        private Dictionary<string, int> PlayerTotalWinnings { get; set; } = new Dictionary<string, int>();

        private PlayerManager PlayerManager =>GameManager.Instance.PlayerManager;
        private TurnManager TurnManager => GameManager.Instance.TurnManager;

        #endregion

        #region Initialization

        public ScoreManager() { }

        public void Init()
        {
 
        }

        public void ResetForNewGame()
        {
            TotalCoinsInPlay = InitialCoins * PlayerManager.GetActivePlayers().Count;
            Pot = 0;
            CurrentBet = BaseBet;
            BlindMultiplier = 1;
            PlayerWins.Clear();
            PlayerTotalWinnings.Clear();
            foreach (var player in PlayerManager.GetActivePlayers())
            {
                PlayerWins[player.Id] = 0;
                PlayerTotalWinnings[player.Id] = 0;
            }
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
            PlayerWins[winner.Id]++;
            PlayerTotalWinnings[winner.Id] += potAmount;
            Pot = 0;

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardPotToWinner - After");
                return false;
            }
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
                PlayerTotalWinnings[player.Id] += splitAmount;
            }

            int remainder = Pot % tiedPlayers.Count;
            if (remainder > 0)
            {
                tiedPlayers[0].AdjustCoins(remainder);
                PlayerTotalWinnings[tiedPlayers[0].Id] += remainder;
            }

            Pot = 0;

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AwardTiedPot - After");
                return false;
            }
            return true;
        }

        public bool AddToRoundScores(Player winner, int potAmount)
        {
            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AddToRoundScores - Before");
                return false;
            }

            PlayerWins[winner.Id]++;
            PlayerTotalWinnings[winner.Id] += potAmount;

            if (!VerifyTotalCoins())
            {
                HandleVerificationFailure("AddToRoundScores - After");
                return false;
            }
            return true;
        }

        public int GetPlayerWins(string playerId)
        {
            return PlayerWins.GetValueOrDefault(playerId, 0);
        }

        public int GetPlayerTotalWinnings(string playerId)
        {
            return PlayerTotalWinnings.GetValueOrDefault(playerId, 0);
        }

        public (string WinnerId, int WinCount) GetOverallWinner()
        {
            string winnerId = null;
            int maxWins = -1;
            foreach (var kvp in PlayerWins)
            {
                if (kvp.Value > maxWins)
                {
                    maxWins = kvp.Value;
                    winnerId = kvp.Key;
                }
            }
            return (winnerId, maxWins);
        }

        public List<(string PlayerId, int Wins, int TotalWinnings)> GetLeaderboard()
        {
            List<(string PlayerId, int Wins, int TotalWinnings)> leaderboard = new List<(string, int, int)>();
            foreach (var playerId in PlayerWins.Keys)
            {
                leaderboard.Add((playerId, PlayerWins[playerId], PlayerTotalWinnings[playerId]));
            }
            leaderboard.Sort((a, b) =>
            {
                int winComparison = b.Wins.CompareTo(a.Wins);
                if (winComparison != 0) return winComparison;
                return b.TotalWinnings.CompareTo(a.TotalWinnings);
            });
            return leaderboard;
        }

        #endregion

        #region Private Helper Methods

        private bool ValidateBet(int betAmount)
        {
            return TurnManager.CurrentPlayer.CanAffordBet(betAmount) && (Pot + betAmount <= TotalCoinsInPlay);
        }

        private bool ProcessBet(int betAmount)
        {
           // Debug.Log($"Processing bet: Player {TurnManager.CurrentPlayer.Id}, Amount: {betAmount}, Current Pot: {Pot}");
            if (!ValidateBet(betAmount))
            {
                Debug.LogError($"Invalid bet: Player {TurnManager.CurrentPlayer.Id}, Amount: {betAmount}, Player Coins: {TurnManager.CurrentPlayer.Coins}");
                return false;
            }
            TurnManager.CurrentPlayer.Bet(betAmount);
            Pot += betAmount;
            CurrentBet = betAmount;
            bool verificationResult = VerifyTotalCoins();
          //  Debug.Log($"Bet processed. New Pot: {Pot}, Verification Result: {verificationResult}");
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
}