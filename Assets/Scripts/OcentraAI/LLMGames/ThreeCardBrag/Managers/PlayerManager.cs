using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    public class PlayerManager
    {
        [ShowInInspector] private List<Player> Players { get; set; } = new();

        private readonly HashSet<string> foldedPlayers = new HashSet<string>();

        // Players
        [ShowInInspector] public HumanPlayer HumanPlayer { get; set; }
        [ShowInInspector] public ComputerPlayer ComputerPlayer { get; set; }
        private DeckManager DeckManager => GameManager.Instance.DeckManager;
        private ScoreManager ScoreManager=> GameManager.Instance.ScoreManager;
        private TurnManager TurnManager => GameManager.Instance.TurnManager;
        public PlayerManager()
        {

        }

        public void Init()
        {

        }
        public void AddPlayer(PlayerData playerData,PlayerType playerType)
        {
            Player player = default;
            switch (playerType)
            {
                case PlayerType.Human:
                    player = new HumanPlayer(playerData,ScoreManager.InitialCoins);
                    HumanPlayer = (HumanPlayer)player; // todo temp for now 2 player setup need to think once multiplayer
                    break;
                case PlayerType.Computer:
                    player = new ComputerPlayer(playerData, ScoreManager.InitialCoins);
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
            foreach (var player in Players)
            {
                player.SetInitialCoins(ScoreManager.InitialCoins);
            }
        }

        public void ResetForNewRound()
        {
            foldedPlayers.Clear();
            foreach (var player in Players)
            {
                player.ResetForNewRound(DeckManager);
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

        public bool GetHumanPlayer(string playerId ,out HumanPlayer player)
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
                if (p.Type == PlayerType.Computer && p is ComputerPlayer computerPlayer )
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