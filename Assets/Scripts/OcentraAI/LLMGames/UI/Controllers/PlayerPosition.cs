using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Players;
using OcentraAI.LLMGames.Players.UI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    [ExecuteAlways]
    public class PlayerPosition : MonoBehaviour
    {
        [SerializeField] [ReadOnly] [ShowInInspector]
        private int currentPlayerCount = 2;

        [SerializeField] [ReadOnly] public bool IsActive;

        [SerializeField] [HideInInspector] private Vector3 lastLocalPosition;

        [SerializeField] [ReadOnly] [ShowInInspector]
        public int PlayerIndex;

        [SerializeField] [TableList] private Vector3[] playerPositions = new Vector3[9];

        [ShowInInspector, ReadOnly]  protected IPlayerUI PlayerUI;


        [ReadOnly] [ShowInInspector] public bool HasPlayerUI => transform.childCount > 0;

        private void Awake()
        {
            Init();
        }

        private void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            if (PlayerUI == null)
            {
                PlayerUI = transform.GetComponentInChildren<IPlayerUI>();
            }

            if (playerPositions == null)
            {
                playerPositions = new Vector3[9];
            }

            SetPlayerUIIndex();
        }

        public void SetPlayerUIIndex()
        {
            if (PlayerUI == null)
            {
                IPlayerUI[] playerUIArray = GetComponentsInChildren<IPlayerUI>(true);
                foreach (IPlayerUI playerUI in playerUIArray)
                {
                    PlayerUI = playerUI;
                    break;
                }
            }

            if (PlayerUI != null)
            {
                PlayerUI.SetPlayerIndex(PlayerIndex);
            }
        }

        public void HandlePlayerCountChanged(int playerCount)
        {
            currentPlayerCount = playerCount;
            if (playerCount is >= 2 and <= 10)
            {
                int index = playerCount - 2;
                transform.localPosition = playerPositions[index];
            }
        }

        private void Update()
        {
            UpdatePlayerPosition();
        }

        [Button]
        private void UpdatePlayerPosition()
        {
#if UNITY_EDITOR

            if (transform.localPosition != lastLocalPosition)
            {
                EditorUtility.SetDirty(this);
                if (currentPlayerCount < 2)
                {
                    currentPlayerCount = 2;
                }

                if (currentPlayerCount > 10)
                {
                    currentPlayerCount = 10;
                }

                if (currentPlayerCount is >= 2 and <= 10)
                {
                    int index = currentPlayerCount - 2;
                    playerPositions[index] = transform.localPosition;
                    EditorUtility.SetDirty(this);
                }

                lastLocalPosition = transform.localPosition;
            }

#endif
        }

#if UNITY_EDITOR


        [Button("Copy Previous Position")]
        [ShowIf(
            "@currentPlayerCount > 2 && currentPlayerCount <= 10 && playerPositions[currentPlayerCount - 2] != playerPositions[currentPlayerCount - 3]")]
        public void CopyPreviousPosition()
        {
            if (currentPlayerCount is > 2 and <= 10)
            {
                int currentIndex = currentPlayerCount - 2;
                int previousIndex = currentIndex - 1;
                playerPositions[currentIndex] = playerPositions[previousIndex];
                transform.localPosition = playerPositions[currentIndex];
                lastLocalPosition = transform.localPosition;
                EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogWarning("Cannot copy previous position. Current player count must be between 3 and 10.");
            }
        }


#endif
    }
}