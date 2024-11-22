using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System;
using OcentraAI.LLMGames.Utilities;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.UI
{
    [RequireComponent(typeof(Button))]
    public class JoinLobbyButton : MonoBehaviour
    {
        [Required] [SerializeField] private Button button;
        [ShowInInspector] public string LobbyId { get; private set; }
        [ShowInInspector] public bool IsPasswordProtected { get; private set; }

        private void OnValidate()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            JoinLobbyAsync().Forget();
        }

        private async UniTask JoinLobbyAsync()
        {
            try
            {
                bool lobbyJoinState = await EventBus.Instance.PublishAsync(new JoinLobbyEvent(LobbyId, IsPasswordProtected));
                button.interactable = false;
              // todo Will do more with the state later
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to join lobby: {ex.Message}");
                // Optionally, show an error message to the user
                // LobbyScreen.Instance.ShowErrorMessage($"Failed to join lobby: {ex.Message}");
            }
            finally
            {
                button.interactable = true;
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }

        public void Init(Lobby lobby)
        {
            if (lobby != null)
            {
                LobbyId = lobby.Id;
                IsPasswordProtected = lobby.HasPassword;
                button.enabled = true;
            }
            else
            {
                LobbyId = "";
                IsPasswordProtected = false;
                button.enabled = false;
            }
        }
    }
}