using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class ElementPosition : MonoBehaviour
    {
        [SerializeField] [ReadOnly] [ShowInInspector]
        private int currentPlayerCount = 2;

        [SerializeField] [TableList] private Vector3[] elementPositions = new Vector3[9];

        [SerializeField] [HideInInspector] private Vector3 lastLocalPosition;


        public void SetPlayerCount(int playerCount)
        {
            currentPlayerCount = playerCount;
        }

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
            if (elementPositions == null)
            {
                elementPositions = new Vector3[9];
            }
        }


        public void HandlePlayerCountChanged()
        {
            if (currentPlayerCount is >= 2 and <= 10)
            {
                int index = currentPlayerCount - 2;
                transform.localPosition = elementPositions[index];
            }
        }


        private void Update()
        {
            UpdateElementPosition();
        }

        [Button]
        private void UpdateElementPosition()
        {
#if UNITY_EDITOR
            if (transform.localPosition != lastLocalPosition)
            {
               

                EditorUtility.SetDirty(this);
                if (currentPlayerCount is >= 2 and <= 10)
                {
                    int index = currentPlayerCount - 2;
                    elementPositions[index] = transform.localPosition;
                    EditorUtility.SetDirty(this);
                }

                lastLocalPosition = transform.localPosition;
            }
#endif
        }

#if UNITY_EDITOR
        [Button("Copy Previous Position")]
        [ShowIf(nameof(CanCopyPreviousPosition))]
        private void CopyPreviousPosition()
        {

            int currentIndex = currentPlayerCount - 2;
            int previousIndex = currentIndex - 1;
            if (currentPlayerCount is > 2 and <= 10)
            {
                elementPositions[currentIndex] = elementPositions[previousIndex];
                transform.localPosition = elementPositions[currentIndex];
                lastLocalPosition = transform.localPosition;
                EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogWarning("Cannot copy previous position. Player count must be between 3 and 10.");
            }
        }

        private bool CanCopyPreviousPosition()
        {
            if (currentPlayerCount is <= 2 or > 10)
            {
                return false;
            }

            int currentIndex = currentPlayerCount - 2;
            int previousIndex = currentIndex - 1;

            return elementPositions[currentIndex] != elementPositions[previousIndex];
        }
#endif
    }
}