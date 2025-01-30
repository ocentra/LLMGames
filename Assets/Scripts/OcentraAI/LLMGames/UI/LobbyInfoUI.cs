using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class LobbyInfoUI : MonoBehaviourBase<LobbyInfoUI>
    {
        [SerializeField] protected TMP_Text HeaderText;
        [SerializeField] protected TMP_Text PlayerCountText;
        [SerializeField] protected TMP_Text ButtonText;
        [SerializeField] protected Transform KeyValueSection;
        [SerializeField] protected Transform PlayerListSection;
        [SerializeField] protected Transform PlayerCountSection;
        [SerializeField] protected Transform ShowAllPlayersPanel;
        [SerializeField] protected List<LobbyHolderPlayerUI> LobbyHolderPlayerUIs;
        [SerializeField] protected List<LobbyInfoKeyValueRow> LobbyInfoKeyValueRows;
        [ShowInInspector] protected LobbyHolderUI LobbyHolderUI { get; set; }
        [ShowInInspector] protected GameMode GameMode { get; set; }
        [ShowInInspector] protected Button3DSimple ActionButton { get; set; }
        [ShowInInspector] protected Button3DSimple ShowAllPlayers { get; set; }
        protected override void Awake()
        {
            Init();
            base.Awake();
        }

        protected override void OnValidate()
        {
            Init();
            base.OnValidate();

        }

        protected void Init()
        {
            HeaderText = transform.FindChildRecursively<TMP_Text>(nameof(HeaderText));
            PlayerCountText = transform.FindChildRecursively<TMP_Text>(nameof(PlayerCountText));

            KeyValueSection = transform.FindChildRecursively<Transform>(nameof(KeyValueSection));
            PlayerListSection = transform.FindChildRecursively<Transform>(nameof(PlayerListSection));
            PlayerCountSection = transform.FindChildRecursively<Transform>(nameof(PlayerCountSection));
            ShowAllPlayersPanel = transform.FindChildRecursively<Transform>(nameof(ShowAllPlayersPanel));
            ActionButton = transform.FindChildRecursively<Button3DSimple>(nameof(ActionButton));
            ShowAllPlayers = transform.FindChildRecursively<Button3DSimple>(nameof(ShowAllPlayers));

            LobbyHolderPlayerUIs = transform.FindAllChildrenOfType<LobbyHolderPlayerUI>();

            LobbyInfoKeyValueRows = transform.FindAllChildrenOfType<LobbyInfoKeyValueRow>();

            if (ShowAllPlayersPanel != null)
            {
                ShowAllPlayersPanel.gameObject.SetActive(false);
            }

            if (ActionButton != null)
            {
                ButtonText = ActionButton.transform.FindChildRecursively<TMP_Text>(nameof(ButtonText));
            }
        }


        public override void SubscribeToEvents()
        {
            if (ActionButton != null)
            {
                ActionButton.OnClick.AddListener(OnActionButton);
            }
            EventRegistrar.Subscribe<InfoSubTabStateChangedEvent>(OnInfoSubTabStateChangedEvent);
            base.SubscribeToEvents();
        }



        public override void UnsubscribeFromEvents()
        {
            if (ActionButton != null)
            {
                ActionButton.OnClick.RemoveListener(OnActionButton);
            }
            
            base.UnsubscribeFromEvents();
        }

        private void OnInfoSubTabStateChangedEvent(InfoSubTabStateChangedEvent obj)
        {
            if (ShowAllPlayersPanel != null)
            {
                ShowAllPlayersPanel.gameObject.SetActive(obj.InfoSubEnabled);
            }
        }



        private void OnActionButton()
        {

        }

        public void SetGameMode(LobbyHolderUI lobbyHolderUI)
        {
            LobbyHolderUI = lobbyHolderUI;
            GameMode = lobbyHolderUI.GameMode;
            HeaderText.text = GameMode.GameName;
            SetButton(LobbyHolderUI);
            SetPlayerCount(0, GameMode.MaxPlayers);
        }

        private void SetButton(LobbyHolderUI lobbyHolderUI)
        {
            bool isAvailable = false;

            foreach (LobbyType lobbyType in LobbyType.GetAvailableForCurrentDevice())
            {
                if (lobbyType == lobbyHolderUI.LobbyType)
                {
                    isAvailable = true;
                    break;
                }
            }

            switch (lobbyHolderUI.LobbyType.Name)
            {
                case nameof(LobbyType.DedicatedServer):
                    if (lobbyHolderUI.LobbyActive)
                    {
                        ButtonText.text = lobbyHolderUI.SlotAvailable ? "Join" : "Spectate";
                    }
                    else
                    {
                        ButtonText.text = "Request Lobby";
                    }
                    break;

                case nameof(LobbyType.PlayerLocalLLM):
                case nameof(LobbyType.PlayerLocalAPI):
                case nameof(LobbyType.PlayerRemoteAPI):
                    if (lobbyHolderUI.LobbyActive)
                    {
                        ButtonText.text = lobbyHolderUI.SlotAvailable ? "Join" : "Spectate";
                    }
                    else
                    {
                        ButtonText.text = (lobbyHolderUI.LobbyType.IsPlayerCreatable && isAvailable) ? "Create Lobby" : "Unsupported";
                    }
                    break;

                default:
                    ButtonText.text = "Unavailable";
                    break;
            }
        }



        public void SetPlayerCount(int playerCount, int maxPlayerCount)
        {
            string colorHex = playerCount == maxPlayerCount ? "#00FF00" : "#FFFF00"; // Green if full, Yellow otherwise
            PlayerCountText.text = $"<color=#FFFFFF>Player</color> <color={colorHex}>{playerCount}</color> <color=#FFFFFF>|</color> <color={colorHex}>{maxPlayerCount}</color>";
        }


        public void SetKeyValues(List<LobbyInfoEntry> lobbyInfoEntries)
        {
            int i = 0;

            foreach (LobbyInfoEntry infoEntry in lobbyInfoEntries)
            {
                if (i < LobbyInfoKeyValueRows.Count)
                {
                    LobbyInfoKeyValueRows[i].gameObject.SetActive(true);
                    LobbyInfoKeyValueRows[i].SetData(infoEntry.GetKeyValueTuple());
                }
                else
                {
                    Debug.LogWarning("Not enough rows available in the pool. Consider increasing the pool size.");
                    break;
                }
                i++;
            }

            for (; i < LobbyInfoKeyValueRows.Count; i++)
            {
                LobbyInfoKeyValueRows[i].gameObject.SetActive(false);
            }
        }


        public void SetPlayers(List<(Sprite icon, string name)> players)
        {
            int i = 0;

            foreach ((Sprite icon, string name) player in players)
            {
                if (i < LobbyHolderPlayerUIs.Count)
                {
                    LobbyHolderPlayerUIs[i].gameObject.SetActive(true);
                    LobbyHolderPlayerUIs[i].SetData(player.icon, player.name);
                }
                else
                {
                    Debug.LogWarning("Not enough player UIs available in the pool. Consider increasing the pool size.");
                    break;
                }
                i++;
            }

            for (; i < LobbyHolderPlayerUIs.Count; i++)
            {
                LobbyHolderPlayerUIs[i].gameObject.SetActive(false);
            }

            SetPlayerCount(players.Count, GameMode.MaxPlayers);
        }

        protected override void OnEnable()
        {

            if (LobbyHolderPlayerUIs != null) LobbyHolderPlayerUIs.Clear();
            if (LobbyInfoKeyValueRows != null) LobbyInfoKeyValueRows.Clear();

            Init();
            base.OnEnable();
        }

    }
}