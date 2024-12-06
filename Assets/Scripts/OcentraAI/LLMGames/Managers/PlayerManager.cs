using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager.Authentication;
using OcentraAI.LLMGames.Players;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.Manager
{
    public class PlayerManager : ManagerBase<PlayerManager>
    {
        [ShowInInspector] [ReadOnly] private readonly HashSet<string> foldedPlayers = new HashSet<string>();

        [ShowInInspector] [ReadOnly] private List<LLMPlayer> Players { get; set; } = new();

        [ShowInInspector] [ReadOnly] public int NumberOfPlayers => Players.Count;

        public void AddPlayer(AuthPlayerData authPlayerData, PlayerType playerType, int index, int initialPlayerCoins)
        {
            switch (playerType)
            {
                case PlayerType.Human:

                    HumanLLMPlayer humanLLMPlayer = new HumanLLMPlayer(authPlayerData, initialPlayerCoins, index);
                    if (!Players.Contains(humanLLMPlayer))
                    {
                        Players.Add(humanLLMPlayer);
                    }

                    break;

                case PlayerType.Computer:

                    ComputerLLMPlayer computerLLMPlayer = new ComputerLLMPlayer(authPlayerData, initialPlayerCoins, index);
                    if (!Players.Contains(computerLLMPlayer))
                    {
                        Players.Add(computerLLMPlayer);
                    }

                    break;
            }
        }

        public async UniTask<bool> ResetForNewGame(int initialPlayerCoins)
        {

            try
            {
                foldedPlayers.Clear();

                foreach (LLMPlayer player in Players)
                {
                    if (player != null)
                    {
                        player.SetInitialCoins(initialPlayerCoins);
                    }
                    else
                    {
                        LogError("Encountered a null player while resetting for new game.", this);
                    }
                }

                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewGame: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        public async UniTask<bool> ResetForNewRound(DeckManager deckManager)
        {
            

            try
            {
                foldedPlayers.Clear();

                if (TryGetHumanPlayer(out HumanLLMPlayer humanLLMPlayer) && TryGetComputerPlayer(out ComputerLLMPlayer computerLLMPlayer))
                {
                    if (Application.isEditor && GameSettings.Instance.DevModeEnabled)
                    {
#if UNITY_EDITOR

                        bool devModeHandled = DevTools.DevModeManager.Instance.InitializeDevModeHands(this, deckManager, humanLLMPlayer, computerLLMPlayer);

                        if (!devModeHandled || !DevTools.DevModeManager.Instance.IsPlayerHandInitialized(humanLLMPlayer))
                        {
                            LogError("Human player's hand not initialized in Dev Mode; initializing manually.", this);
                            humanLLMPlayer.ResetForNewRound(deckManager);
                        }

                        if (!devModeHandled || !DevTools.DevModeManager.Instance.IsPlayerHandInitialized(computerLLMPlayer))
                        {
                            LogError("Computer player's hand not initialized in Dev Mode; initializing manually.", this);
                            computerLLMPlayer.ResetForNewRound(deckManager);
                        }
#endif
                    }

                    foreach (LLMPlayer player in Players)
                    {
                        if (player != null && player != humanLLMPlayer && player != computerLLMPlayer)
                        {
                            player.ResetForNewRound(deckManager);
                        }
                    }
                }
                else
                {
                    LogError("Failed to retrieve HumanLLMPlayer or ComputerLLMPlayer for resetting the round.", this);  
                    return false;
                }

                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in ResetForNewRound: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        public bool TryInitializePlayerHand(LLMPlayer llmPlayer, Hand customHand, DeckManager deckManager)
        {
            if (customHand != null && customHand.VerifyHand(GameManager.Instance.GameMode,
                    GameManager.Instance.GameMode.NumberOfCards))
            {
                llmPlayer.ResetForNewRound(deckManager, customHand);
                return true;
            }

            return false;
        }


        public bool FoldPlayer(TurnManager turnManager)
        {
            turnManager.CurrentLLMPlayer.Fold();
            return foldedPlayers.Add(turnManager.CurrentLLMPlayer.AuthPlayerData.PlayerID);
        }

        public List<LLMPlayer> GetActivePlayers()
        {
            List<LLMPlayer> activePlayers = new List<LLMPlayer>();
            foreach (LLMPlayer player in Players)
            {
                if (!foldedPlayers.Contains(player.AuthPlayerData.PlayerID))
                {
                    activePlayers.Add(player);
                }
            }

            return activePlayers;
        }


        public LLMPlayer GetPlayerById(string playerId)
        {
            foreach (LLMPlayer player in Players)
            {
                if (player.AuthPlayerData.PlayerID == playerId)
                {
                    return player;
                }
            }

            return null;
        }


        public bool IsRoundOver()
        {
            int activePlayerCount = 0;
            foreach (LLMPlayer player in Players)
            {
                if (!foldedPlayers.Contains(player.AuthPlayerData.PlayerID))
                {
                    activePlayerCount++;
                }
            }

            return activePlayerCount <= 1;
        }


        public bool TryGetHumanPlayer(out HumanLLMPlayer humanPlayer)
        {
            humanPlayer = null;

            try
            {
                if (AuthenticationManager.Instance == null)
                {
                    LogError("AuthenticationManager instance is null.", this);
                    return false;
                }

                AuthPlayerData authPlayerData = AuthenticationManager.Instance.GetAuthPlayerData();
                if (authPlayerData == null)
                {
                    LogError("AuthPlayerData is null.", this);
                    return false;
                }

                foreach (LLMPlayer player in Players)
                {
                    if (player != null &&
                        player.AuthPlayerData != null &&
                        player.Type == PlayerType.Human &&
                        player.AuthPlayerData.PlayerID == authPlayerData.PlayerID &&
                        player is HumanLLMPlayer foundHumanPlayer)
                    {
                        humanPlayer = foundHumanPlayer;
                        return true;
                    }
                }

                LogError("No matching HumanLLMPlayer found.", this);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error in TryGetHumanPlayer: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }

        public bool TryGetComputerPlayer(out ComputerLLMPlayer computerPlayer)
        {
            computerPlayer = null;

            try
            {
                foreach (LLMPlayer player in Players)
                {
                    if (player != null &&
                        player.Type == PlayerType.Computer &&
                        player is ComputerLLMPlayer foundComputerPlayer)
                    {
                        computerPlayer = foundComputerPlayer;
                        return true;
                    }
                }

                LogError("No matching ComputerLLMPlayer found.", this);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error in TryGetComputerPlayer: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        public ComputerLLMPlayer GetComputerPlayer()
        {
            // todo code for when there are more than 1 computer player

            foreach (LLMPlayer player in Players)
            {
                if (player.Type == PlayerType.Computer && player is ComputerLLMPlayer computerPlayer)
                {
                    return computerPlayer;
                }
            }

            return default;
        }

        public List<LLMPlayer> GetAllPlayers()
        {
            return Players;
        }

        public void ShowHand(bool showHand, bool isRoundEnd = false)
        {
            foreach (LLMPlayer player in Players)
            {
                player.ShowHand(isRoundEnd && showHand);
            }
        }
    }
}