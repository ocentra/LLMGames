using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class JoinedLobbyScreen : UI3DScreen<JoinedLobbyScreen>
    {
        [SerializeField][Required] private Button joinedLobbyStartButton;
        [SerializeField][Required] private Transform playerListParent;
        [SerializeField][Required] private TextMeshProUGUI joinedLobbyGamemodeText;
        [SerializeField][Required] private TextMeshProUGUI joinedLobbyNameText;
        [SerializeField][Required] private static GameObject PlayerItemPrefab { get; set; }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventBus.Instance.SubscribeAsync<JoinedLobbyEvent>(OnJoinedLobby);
            EventBus.Instance.SubscribeAsync<UpdateLobbyPlayerListEvent>(OnUpdateLobbyPlayerList);
            joinedLobbyStartButton.onClick.AddListener(OnLobbyStart);
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            EventBus.Instance.UnsubscribeAsync<JoinedLobbyEvent>(OnJoinedLobby);
            EventBus.Instance.UnsubscribeAsync<UpdateLobbyPlayerListEvent>(OnUpdateLobbyPlayerList);
            joinedLobbyStartButton.onClick.RemoveListener(OnLobbyStart);
        }

        private void OnLobbyStart()
        {
          //  EventBus.Instance.Publish(new StartLobbyAsHostEvent());
        }

        private async UniTask OnUpdateLobbyPlayerList(UpdateLobbyPlayerListEvent updateLobbyPlayerList)
        {
            Lobby lobby = updateLobbyPlayerList.Lobby;
            DataObject dataObject = lobby.Data["GameMode"];
            string gameMode = dataObject.Value;

            if (joinedLobbyStartButton != null)
            {
                joinedLobbyStartButton.gameObject.SetActive(AuthenticationService.Instance.PlayerId == lobby.HostId);
            }

            if (joinedLobbyNameText != null)
            {
                joinedLobbyNameText.text = lobby.Name;
            }

            if (joinedLobbyGamemodeText != null)
            {
                joinedLobbyGamemodeText.text = gameMode;
            }

            UpdatePlayerList(lobby);
            await UniTask.Yield();
        }

        private void UpdatePlayerList(Lobby lobby)
        {
            if (playerListParent == null) return;

            HashSet<string> existingPlayerIds = new HashSet<string>();

            foreach (Transform child in playerListParent)
            {
                var playerItemUI = child.GetComponent<PlayerItemUI>();

                if (playerItemUI != null)
                {
                    if (!lobby.Players.Exists(p => p == playerItemUI.Player))
                    {
                        Destroy(child.gameObject);
                    }
                    else if (!existingPlayerIds.Add(playerItemUI.Player.Id))
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            foreach (Player player in lobby.Players)
            {
                if (!existingPlayerIds.Contains(player.Id))
                {
                    GameObject newPlayerItem = Instantiate(PlayerItemPrefab, playerListParent);
                    PlayerItemUI playerItemUI = newPlayerItem.GetComponent<PlayerItemUI>();
                    if (playerItemUI != null)
                    {
                        playerItemUI.InitLobby(player, lobby.HostId == player.Id ? "Owner" : "User");
                        existingPlayerIds.Add(player.Id);
                    }
                }
            }
        }


        private async UniTask OnJoinedLobby(JoinedLobbyEvent joinedLobbyEvent)
        {
            if (joinedLobbyEvent.HasJoined)
            {
                HideScreen();
            }
            else
            {
                ShowScreen();
            }

            await UniTask.Yield();
        }

        protected override void Init(bool startEnabled)
        {
            transform.FindChildWithComponent(ref playerListParent, nameof(playerListParent));
            transform.FindChildWithComponent(ref joinedLobbyGamemodeText, nameof(joinedLobbyGamemodeText));
            transform.FindChildWithComponent(ref joinedLobbyNameText, nameof(joinedLobbyNameText));
            transform.FindChildWithComponent(ref joinedLobbyStartButton, nameof(joinedLobbyStartButton));
            if (PlayerItemPrefab == null)
            {
                PlayerItemPrefab = Resources.Load<GameObject>("Prefabs/PlayerItemPrefab");
            }
            base.Init(startEnabled);
        }
    }
}
