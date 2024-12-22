using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.String;

namespace OcentraAI.LLMGames.Screens
{
    public class LobbyCreationScreen : UI3DScreen<LobbyCreationScreen>
    {
        [SerializeField][Required] private TMP_Dropdown createLobbyGameModeDropdown;
        [SerializeField][Required] private TMP_InputField createLobbyMaxPlayersField;
        [SerializeField][Required] private TMP_InputField createLobbyNameField;
        [SerializeField][Required] private TMP_InputField createLobbyPasswordField;
        [SerializeField][Required] private Toggle createLobbyPrivateToggle;
        [SerializeField][Required] private Button exitButton;
        [SerializeField][Required] private Button createLobbyButton;



        private bool ValidateMaxPlayers(out int maxPlayers)
        {
            maxPlayers = 0;
            if (!int.TryParse(createLobbyMaxPlayersField.text, out int parsedValue))
            {
                return false;
            }

            if (parsedValue is < 1 or > 10)
            {
                return false;
            }

            maxPlayers = parsedValue;
            return true;
        }

        #region Event Subscriptions

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            createLobbyPrivateToggle.onValueChanged.AddListener(OnCreateLobbyPrivateToggle);
            exitButton.onClick.AddListener(ExitLobby);
            createLobbyButton.onClick.AddListener(CreateLobby);
            createLobbyNameField.onEndEdit.AddListener(OnLobbyNameFieldChanged);
            createLobbyNameField.onValueChanged.AddListener(OnLobbyNameFieldChanged);
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            createLobbyPrivateToggle.onValueChanged.RemoveListener(OnCreateLobbyPrivateToggle);
            exitButton.onClick.RemoveListener(ExitLobby);
            createLobbyButton.onClick.RemoveListener(CreateLobby);
            createLobbyNameField.onEndEdit.RemoveListener(OnLobbyNameFieldChanged);
            createLobbyNameField.onValueChanged.RemoveListener(OnLobbyNameFieldChanged);
        }

        private void OnLobbyNameFieldChanged(string lobbyName)
        {
            if (!IsNullOrEmpty(lobbyName))
            {
                createLobbyButton.enabled = true;
            }
        }

        private void CreateLobby()
        {
            int maxPlayers = 10; 
            if (ValidateMaxPlayers(out int value))
            {
                maxPlayers = value;
            }

            LobbyOptions lobbyOptions = new LobbyOptions(
                createLobbyNameField.text,
                createLobbyGameModeDropdown.options[createLobbyGameModeDropdown.value].text,
                maxPlayers,
                createLobbyPrivateToggle.isOn,
                createLobbyPasswordField.text
            );

            EventBus.Instance.Publish(new CreateLobbyEvent(lobbyOptions));

        }
        public void OnCreateLobbyPrivateToggle(bool value)
        {
            createLobbyPasswordField.gameObject.SetActive(value);
        }

        private void ExitLobby()
        {
            EventBus.Instance.Publish(new ShowScreenEvent(LobbyListScreen.Instance.ScreenName));
        }

        #endregion

        [Button]
        protected override void Init(bool startEnabled)
        {
            transform.FindChildWithComponent(ref createLobbyGameModeDropdown, nameof(createLobbyGameModeDropdown));
            transform.FindChildWithComponent(ref createLobbyMaxPlayersField, nameof(createLobbyMaxPlayersField));
            transform.FindChildWithComponent(ref createLobbyNameField, nameof(createLobbyNameField));
            transform.FindChildWithComponent(ref createLobbyPasswordField, nameof(createLobbyPasswordField));
            transform.FindChildWithComponent(ref createLobbyPrivateToggle, nameof(createLobbyPrivateToggle));
            transform.FindChildWithComponent(ref exitButton, nameof(exitButton));
            transform.FindChildWithComponent(ref createLobbyButton, nameof(createLobbyButton));
           
            base.Init(StartEnabled);
        }

        public override void ShowScreen()
        {
            base.ShowScreen();

            InitializeScreen();
        }

        private void InitializeScreen()
        {
            if (createLobbyNameField != null)
            {
                createLobbyNameField.text = Empty;

            }

            if (createLobbyPasswordField != null)
            {
                createLobbyPasswordField.text = Empty;
            }

            if (createLobbyMaxPlayersField != null)
            {
                createLobbyMaxPlayersField.text = "1";
            }

            if (createLobbyPrivateToggle != null)
            {
                createLobbyPrivateToggle.isOn = false;
            }

            if (createLobbyButton != null)
            {
                createLobbyButton.enabled = false;
            }
        }
    }
}