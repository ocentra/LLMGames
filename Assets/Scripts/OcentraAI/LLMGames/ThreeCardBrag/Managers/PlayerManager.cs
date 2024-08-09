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

        // Players
        [ShowInInspector, ReadOnly] public HumanPlayer HumanPlayer { get; set; }
        [ShowInInspector, ReadOnly] public ComputerPlayer ComputerPlayer { get; set; }
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
            Player player = default;
            switch (playerType)
            {
                case PlayerType.Human:
                    player = new HumanPlayer(playerData, GameMode.InitialPlayerCoins);
                    HumanPlayer = (HumanPlayer)player; // todo temp for now 2 player setup need to think once multiplayer
                    break;
                case PlayerType.Computer:
                    player = new ComputerPlayer(playerData, GameMode.InitialPlayerCoins);
                    ComputerPlayer = (ComputerPlayer)player; // todo temp for now 2 player setup need to think once multiplayer
                    break;
            }

            if (player != default && !Players.Contains(player))
            {
                Players.Add(player);
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

            foreach (Player player in Players)
            {
                if (DevModeManager.Instance != null)
                {
                    DevModeManager.Instance.ApplyDevHandToPlayer(player, DeckManager);
                }
                else
                {
                    player.ResetForNewRound(DeckManager);
                }
            }
        }


        public bool FoldPlayer()
        {
            TurnManager.CurrentPlayer.Fold();
            return foldedPlayers.Add(TurnManager.CurrentPlayer.Id);
        }

        public List<Player> GetActivePlayers()
        {
            return Players.Where(p => !foldedPlayers.Contains(p.Id)).ToList();
        }

        public Player GetPlayerById(string playerId)
        {
            return Players.FirstOrDefault(p => p.Id == playerId);
        }

        public bool IsRoundOver()
        {
            return GetActivePlayers().Count <= 1;
        }

        public bool GetHumanPlayer(string playerId, out HumanPlayer player)
        {
            player = default;

            foreach (Player p in Players)
            {
                if (p.Type == PlayerType.Human && p.PlayerData.PlayerID == playerId && p is HumanPlayer humanPlayer)
                {
                    player = humanPlayer;
                    return true;
                }
            }

            return false;
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