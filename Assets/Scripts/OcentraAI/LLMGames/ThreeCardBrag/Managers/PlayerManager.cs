using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public class PlayerManager : ManagerBase<PlayerManager>
    {

        [ShowInInspector, ReadOnly] private List<Player> Players { get; set; } = new();

        [ShowInInspector, ReadOnly] private readonly HashSet<string> foldedPlayers = new HashSet<string>();


        private DeckManager DeckManager => DeckManager.Instance;
        private ScoreManager ScoreManager => ScoreManager.Instance;
        private TurnManager TurnManager => TurnManager.Instance;
        private GameManager GameManager => GameManager.Instance;
        private GameMode GameMode => GameManager.GameMode;

        protected override void Awake()
        {
            base.Awake();

        }


        public void AddPlayer(PlayerData playerData, PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Human:

                    HumanPlayer humanPlayer = new HumanPlayer(playerData, GameMode.InitialPlayerCoins);
                    if (!Players.Contains(humanPlayer))
                    {
                        Players.Add(humanPlayer);
                    }

                    break;

                case PlayerType.Computer:

                    ComputerPlayer computerPlayer = new ComputerPlayer(playerData, GameMode.InitialPlayerCoins);
                    if (!Players.Contains(computerPlayer))
                    {
                        Players.Add(computerPlayer);
                    }

                    break;
            }


        }

        public void ResetForNewGame()
        {
            foldedPlayers.Clear();
            foreach (Player player in Players)
            {
                player.SetInitialCoins(GameMode.InitialPlayerCoins);
            }
        }



        public void ResetForNewRound()
        {
            foldedPlayers.Clear();

            HumanPlayer humanPlayer = GetHumanPlayer();
            ComputerPlayer computerPlayer = GetComputerPlayer();

#if UNITY_EDITOR
            
            bool devModeHandled = DevModeManager.Instance != null && DevModeManager.Instance.InitializeDevModeHands(this, humanPlayer, computerPlayer);

            // Check if each player was initialized in DevModeManager, and if not, initialize them here
            if (!devModeHandled || !DevModeManager.Instance.IsPlayerHandInitialized(humanPlayer))
            {
                humanPlayer?.ResetForNewRound(DeckManager);
            }

            if (!devModeHandled || !DevModeManager.Instance.IsPlayerHandInitialized(computerPlayer))
            {
                computerPlayer?.ResetForNewRound(DeckManager);
            }

#endif

            // Continue with the normal reset process for other players
            foreach (Player player in Players)
            {
                if (player != null && player != humanPlayer && player != computerPlayer)
                {
                    player.ResetForNewRound(DeckManager);
                }
            }
        }


        public bool TryInitializePlayerHand(Player player, Hand customHand)
        {
            if (customHand != null && customHand.VerifyHand(GameManager.Instance.GameMode, GameManager.Instance.GameMode.NumberOfCards))
            {
                player.ResetForNewRound(DeckManager, customHand);
                return true;
            }
            return false;
        }


        public bool FoldPlayer()
        {
            TurnManager.CurrentPlayer.Fold();
            return foldedPlayers.Add(TurnManager.CurrentPlayer.PlayerData.PlayerID);
        }

        public List<Player> GetActivePlayers()
        {
            return Players.Where(player => !foldedPlayers.Contains(player.PlayerData.PlayerID)).ToList();
        }

        public Player GetPlayerById(string playerId)
        {
            return Players.FirstOrDefault(player => player.PlayerData.PlayerID == playerId);
        }

        public bool IsRoundOver()
        {
            return GetActivePlayers().Count <= 1;
        }

        public HumanPlayer GetHumanPlayer(string playerId)
        {

            foreach (Player player in Players)
            {
                if (player.Type == PlayerType.Human && player.PlayerData.PlayerID == playerId && player is HumanPlayer humanPlayer)
                {
                    return humanPlayer;
                }
            }

            return null;
        }

        public HumanPlayer GetHumanPlayer()
        {

            foreach (Player player in Players)
            {
                if (player.Type == PlayerType.Human && player.PlayerData.PlayerID == AuthenticationManager.Instance.PlayerData.PlayerID && player is HumanPlayer humanPlayer)
                {
                    return humanPlayer;
                }
            }

            return null;
        }

        public ComputerPlayer GetComputerPlayer()
        {
            // todo code for when there are more than 1 computer player

            foreach (Player p in Players)
            {
                if (p.Type == PlayerType.Computer && p is ComputerPlayer computerPlayer)
                {
                    return computerPlayer;
                }
            }
            return default;
        }

        public List<Player> GetAllPlayers()
        {
            return Players;
        }

        public void ShowHand(bool showHand, bool isRoundEnd = false)
        {
            foreach (Player player in Players)
            {
                player.ShowHand(isRoundEnd && showHand);
            }
        }
    }
}