using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using static System.String;

namespace OcentraAI.LLMGames.Screens
{
    public class LobbyListScreen : UI3DScreen<LobbyListScreen>
    {
        [SerializeField][Required] private Button createNewLobby;
        [SerializeField][Required] private TMP_InputField searchLobbyNameInputField;
        [SerializeField][Required] private Transform lobbyContentParent;
        [ShowInInspector][Required] private GameObject LobbyItemPrefab { get; set; }
        private CancellationTokenSource updateLobbyCancellationTokenSource;

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if (searchLobbyNameInputField != null)
            {
                searchLobbyNameInputField.onEndEdit.AddListener(OnSearchLobbyNameInputFieldChanged);
            }

            if (createNewLobby != null)
            {
                createNewLobby.onClick.AddListener(OnCreateNewLobby);
            }

            EventRegistrar.Subscribe<UpdateLobbyListEvent>(OnUpdateLobby);
            EventRegistrar.Subscribe<ProfileCreatedEvent>(OnProfileCreated);
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
           

            if (searchLobbyNameInputField != null)
            {
                searchLobbyNameInputField.onEndEdit.RemoveListener(OnSearchLobbyNameInputFieldChanged);
            }

            if (createNewLobby != null)
            {
                createNewLobby.onClick.RemoveListener(OnCreateNewLobby);
            }
            
        }

        private void OnCreateNewLobby()
        {
            EventBus.Instance.Publish(new ShowScreenEvent(LobbyCreationScreen.Instance.ScreenName));
        }

        private void OnSearchLobbyNameInputFieldChanged(string searchString)
        {
            PublishUpdateLobby(searchString);
        }

        private void PublishUpdateLobby(string searchString = null)
        {
            if (!lobbyContentParent.gameObject.activeInHierarchy) { return; }

            updateLobbyCancellationTokenSource?.Cancel();
            updateLobbyCancellationTokenSource?.Dispose();

            updateLobbyCancellationTokenSource = new CancellationTokenSource();
            EventBus.Instance.PublishAsync(new UpdateLobbyEvent(updateLobbyCancellationTokenSource, searchString ?? Empty)).Forget();
        }

        private async void OnUpdateLobby(UpdateLobbyListEvent updateLobbyListEvent)
        {
            if(!lobbyContentParent.gameObject.activeInHierarchy){ return;}

            bool hasLobbies = updateLobbyListEvent.Lobbies != null && updateLobbyListEvent.Lobbies.Any();
            GameObject noLobbyItem = null;

            foreach (Transform child in lobbyContentParent)
            {
                if (child.name == "NoLobbyItem")
                {
                    noLobbyItem = child.gameObject;
                    break;
                }
            }

            if (!hasLobbies)
            {
                foreach (Transform child in lobbyContentParent)
                {
                    if (child.name != "NoLobbyItem")
                    {
                        Destroy(child.gameObject);
                    }
                }

                if (noLobbyItem == null)
                {
                    noLobbyItem = Instantiate(LobbyItemPrefab, lobbyContentParent);
                    noLobbyItem.name = "NoLobbyItem";
                    LobbyItemUI lobbyItemUI = noLobbyItem.GetComponent<LobbyItemUI>();
                    if (lobbyItemUI != null)
                    {
                        lobbyItemUI.InitLobby(null);
                    }
                }
            }
            else
            {
                if (noLobbyItem != null)
                {
                    Destroy(noLobbyItem);
                }

                foreach (Lobby lobby in updateLobbyListEvent.Lobbies)
                {
                    if (!LobbyItemExists(lobby))
                    {
                        GameObject newLobbyItem = Instantiate(LobbyItemPrefab, lobbyContentParent);
                        LobbyItemUI lobbyItemUI = newLobbyItem.GetComponent<LobbyItemUI>();
                        if (lobbyItemUI != null)
                        {
                            lobbyItemUI.InitLobby(lobby);
                        }
                    }
                }
            }

            await UniTask.Yield();

            bool LobbyItemExists(Lobby lobby)
            {
                foreach (Transform child in lobbyContentParent)
                {
                    LobbyItemUI lobbyItemUI = child.GetComponent<LobbyItemUI>();
                    if (lobbyItemUI != null && lobbyItemUI.Lobby.Id == lobby.Id)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected override void Init(bool startEnabled)
        {
            transform.FindChildWithComponent(ref searchLobbyNameInputField, nameof(searchLobbyNameInputField));
            transform.FindChildWithComponent(ref lobbyContentParent, nameof(lobbyContentParent));
            transform.FindChildWithComponent(ref createNewLobby, nameof(createNewLobby));
            LobbyItemPrefab = Resources.Load<GameObject>($"Prefabs/{nameof(LobbyItemPrefab)}");
            base.Init(StartEnabled);
        }

        private void OnProfileCreated(ProfileCreatedEvent obj)
        {
            ShowScreen();
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
            InitializeScreen();
        }

        private void InitializeScreen()
        {
            PublishUpdateLobby();
        }

        public override void HideScreen()
        {

            base.HideScreen();
        }
    }
}
