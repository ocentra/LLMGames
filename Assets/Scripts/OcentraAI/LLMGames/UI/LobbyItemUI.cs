using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.UI;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class LobbyItemUI : MonoBehaviour
    {
        [Required][SerializeField] private JoinLobbyButton joinLobbyButton;
        [Required] [SerializeField] private TextMeshProUGUI lobbyName;
        [Required][SerializeField] private TextMeshProUGUI gameMode;
        [Required][SerializeField] private TextMeshProUGUI playerCount;
        [ShowInInspector] public Lobby Lobby { get; private set; }
        void OnValidate()
        {
            Init();
        }
        void Awake()
        {
            Init();
        }
        public void Init()
        {
            if (joinLobbyButton == null)
            {
                joinLobbyButton = GetComponent<JoinLobbyButton>();

            }

            transform.FindChildWithComponent(ref lobbyName, nameof(lobbyName));
            transform.FindChildWithComponent(ref gameMode, nameof(gameMode));
            transform.FindChildWithComponent(ref playerCount, nameof(playerCount));
        }
        public void InitLobby(Lobby lobby)
        {
            Lobby = lobby;

            if (Lobby == null)
            {
                if (lobbyName != null)
                {
                    lobbyName.text = "No lobbies found";
                }
                if (gameMode != null)
                {
                    gameMode.text = "";
                }
                if (playerCount != null)
                {
                    playerCount.text = "";
                }
                if (joinLobbyButton != null)
                {
                    joinLobbyButton.Init(null);
                }
            }
            else
            {
                if (joinLobbyButton != null)
                {
                    joinLobbyButton.Init(Lobby);
                }
                if (lobbyName != null)
                {
                    lobbyName.text = Lobby.Name;
                }
                if (gameMode != null && Lobby.Data.TryGetValue("GameMode", out DataObject dataObject))
                {
                    gameMode.text = dataObject.Value;
                }
                if (playerCount != null)
                {
                    playerCount.text = $"{Lobby.Players.Count} of {Lobby.MaxPlayers}";
                }
            }
        }

    }
}
