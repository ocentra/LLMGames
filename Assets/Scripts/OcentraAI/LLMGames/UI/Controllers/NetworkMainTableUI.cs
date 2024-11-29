using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.UI;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.UI.Managers
{
    [ExecuteAlways]
    public class NetworkMainTableUI : ManagerBase<NetworkMainTableUI>
    {
        [ShowInInspector][ReadOnly] public IPlayerUI[] AllPlayerUI;

        [ShowInInspector][ReadOnly] private ElementPosition[] elementPositions;

        [field: ShowInInspector]
        [field: SerializeField]
        [field: Range(2, 10)]
        [OnValueChanged(nameof(HandlePlayerCountChanged))]
        private int playerCount = 2;

        [ShowInInspector, SerializeField] private float playerIconSize = 0.85f;

        [ShowInInspector, ReadOnly] private GameObject playerPositionContainer;

        [ShowInInspector, SerializeField, ReadOnly] private PlayerPosition[] playerPositions;

        [ShowInInspector, SerializeField] private GameObject table;

        [ShowInInspector, SerializeField, ReadOnly]
        private AnimationController tableController;

        public int PlayerCount
        {
            get => playerCount;
            set
            {
                if (playerCount != value)
                {
                    playerCount = Mathf.Clamp(value, 2, 10);
                    HandlePlayerCountChanged();
                }
            }
        }


        [ShowInInspector][Required] private GameObject PlayerUIPrefab { get; set; }

        [ShowInInspector][ReadOnly] private AnimationController DeckHolder { get; set; }


        protected override UniTask InitializeAsync()
        {
            Init();
            return base.InitializeAsync();
        }

        protected override void OnValidate()
        {
            Init();
            SetMainTableElements();
        }



        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventBus.Instance.SubscribeAsync<RegisterUIPlayerEvent>(OnRegisterUIPlayerEvent);
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            EventBus.Instance.UnsubscribeAsync<RegisterUIPlayerEvent>(OnRegisterUIPlayerEvent);
        }

        private async UniTask OnRegisterUIPlayerEvent(RegisterUIPlayerEvent arg)
        {
            PlayerCount = arg.Players.Count;

            await UniTask.Yield();
        }

        public void ShowDrawnCard(bool show = true)
        {
            DeckHolder.SetPercentageAnimated(show ? 100 : 0);
        }


        private void Init()
        {
            PlayerUIPrefab = Resources.Load<GameObject>($"Prefabs/{nameof(PlayerUIPrefab)}");
            if (PlayerUIPrefab == null)
            {
                Debug.LogError("PlayerUIPrefab not found in Resources folder!");
            }

            if (table == null)
            {
                table = transform.RecursiveFindChildGameObject(nameof(table));
            }


            if (table != null)
            {
                tableController = table.GetComponent<AnimationController>();
            }

            if (DeckHolder == null)
            {
                DeckHolder = transform.FindChildRecursively<AnimationController>(nameof(DeckHolder));
            }

            AllPlayerUI = transform.GetComponentsInChildren<IPlayerUI>(true);
            elementPositions = FindObjectsByType<ElementPosition>(FindObjectsSortMode.None);

            for (int i = 0; i < elementPositions.Length; i++)
            {
                elementPositions[i].SetPlayerCount(PlayerCount);
            }
        }


        private void UpdatePlayerPositions()
        {
            if (playerPositions is { Length: > 0 })
            {
                for (int i = 0; i < playerPositions.Length; i++)
                {
                    PlayerPosition playerPosition = playerPositions[i];
                    playerPosition.PlayerIndex = i;
                    playerPosition.IsActive = i < PlayerCount;
                    playerPosition.gameObject.SetActive(i < PlayerCount);
                    playerPosition.HandlePlayerCountChanged(PlayerCount);
                }
            }
        }

        private void AdjustTableSize()
        {
            if (tableController == null)
            {
                Debug.LogError("TableController is not assigned or missing.");
                return;
            }

            float stopPercentage = PlayerCount switch
            {
                <= 3 => 0f,
                4 => 0.25f,
                5 => 0.4f,
                6 => 0.5f,
                _ => 1f
            };

            tableController.SetPercentage(stopPercentage);
            SetPlayerUI();
        }

        [Button]
        private void SetPlayerUI()
        {
            for (int i = 0; i < playerPositions.Length; i++)
            {
                PlayerPosition playerPosition = playerPositions[i];


#if UNITY_EDITOR

                CreatePlayerUI(playerPosition);

#endif
                if (playerPosition.transform.childCount == 0)
                {
                    Debug.LogError($"No PlayerUI Found for playerPosition {playerPosition.PlayerIndex}");
                }

                foreach (Transform child in playerPosition.transform)
                {
                    bool playerPositionHasPlayerUI = playerPosition.IsActive && playerPosition.HasPlayerUI;
                    child.gameObject.SetActive(playerPositionHasPlayerUI);

                    if (!playerPositionHasPlayerUI)
                    {
                        continue;
                    }

                    child.localScale = new Vector3(playerIconSize, playerIconSize, playerIconSize);
                    CurvedText playerName = child.GetComponentInChildren<CurvedText>();
                    if (playerName != null)
                    {
                        string player = $"Player_{playerPosition.PlayerIndex}";
                        child.gameObject.name = player;
                        playerName.SetPlayerName(player);
                    }
                }
            }

            AllPlayerUI = transform.GetComponentsInChildren<IPlayerUI>(true);
        }


        private void HandlePlayerCountChanged()
        {
            for (int i = 0; i < elementPositions.Length; i++)
            {
                elementPositions[i].SetPlayerCount(PlayerCount);
                elementPositions[i].HandlePlayerCountChanged();
            }

            UpdatePlayerPositions();
            AdjustTableSize();
        }


        private void SetMainTableElements()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                return;
            }
           

            if (playerPositionContainer == null)
            {
                playerPositions = new PlayerPosition[10];
                playerPositionContainer = GameObject.Find(nameof(playerPositionContainer));
                if (playerPositionContainer == null)
                {
                    playerPositionContainer = new GameObject(nameof(playerPositionContainer));
                    playerPositionContainer.transform.SetParent(transform);
                }
                EditorUtility.SetDirty(this);
            }


            playerPositions = playerPositionContainer.GetComponentsInChildren<PlayerPosition>(true);

            if (playerPositions.Length < 10)
            {
                EditorUtility.SetDirty(this);

                List<PlayerPosition> tempPositions = new List<PlayerPosition>(playerPositions);

                for (int i = tempPositions.Count; i < 10; i++)
                {
                    GameObject playerGo = new GameObject($"PlayerPosition_{i + 1}"); // Name starts at 1
                    playerGo.transform.SetParent(playerPositionContainer.transform);

                    PlayerPosition playerPosition = playerGo.AddComponent<PlayerPosition>();
                    playerPosition.PlayerIndex = i;
                    tempPositions.Add(playerPosition);
                }

                playerPositions = tempPositions.ToArray();
            }
           

            UpdatePlayerPositions();

            AssetDatabase.SaveAssetIfDirty(this);

#endif
        }

        private void CreatePlayerUI(PlayerPosition playerPosition)
        {
#if UNITY_EDITOR

            if (EditorApplication.isPlaying)
            {
                return;
            }


            while (playerPosition.transform.childCount > 1)
            {
                DestroyImmediate(playerPosition.transform.GetChild(playerPosition.transform.childCount - 1).gameObject);
            }

            if (playerPosition.transform.childCount == 1)
            {
                Transform childTransform = playerPosition.transform.GetChild(0);

                IPlayerUI component = childTransform.GetComponent<IPlayerUI>();
                component?.SetPlayerIndex(playerPosition.PlayerIndex);
            }
            else if (playerPosition.transform.childCount == 0)
            {
                CreateNew(playerPosition);
            }

#endif
        }

        private void CreateNew(PlayerPosition playerPosition)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                return;
            }

            GameObject prefab = PrefabUtility.InstantiatePrefab(PlayerUIPrefab) as GameObject;
            if (prefab != null)
            {
                prefab.name = $"Player_{playerPosition.PlayerIndex}";
                prefab.transform.SetParent(playerPosition.transform);
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localScale = Vector3.one * playerIconSize;
                prefab.transform.rotation = new Quaternion(0, 180, 0, 0);

                IPlayerUI component = prefab.GetComponent<IPlayerUI>();
                component?.SetPlayerIndex(playerPosition.PlayerIndex);
            }
#endif
        }


    }
}