using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;

namespace ThreeCardBrag
{
    public class LeftPanelController : MonoBehaviour
    {
        public RectTransform Panel;
        [Required]
        public RectTransform Middle;
        [Required]
        public LayoutElement MiddleLayoutElement;
        [Required]
        public Transform FloorCardsHolder;
        [Required]
        public GridLayoutGroup FloorCardsGrid;
        [Required]
        public Button ShowAllFloorCards;
        public List<GameObject> FloorCards = new List<GameObject>();
        public float MinHeight = 700;
        public float MaxHeight = 70000;
        public bool IsExpanded = false;
        [Required]
        public GameObject CardViewPrefab;

        void Start()
        {
            Init();
            ShowAllFloorCards.onClick.AddListener(ToggleExpand);
        }

        void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            IsExpanded = false;
            Panel = GetComponent<RectTransform>();
            Middle = transform.FindChildRecursively<RectTransform>(nameof(Middle));
            MiddleLayoutElement = Middle.GetComponent<LayoutElement>();
            FloorCardsHolder = transform.FindChildRecursively<Transform>(nameof(FloorCardsHolder));
            FloorCardsGrid = FloorCardsHolder.GetComponent<GridLayoutGroup>();
            ShowAllFloorCards = transform.FindChildRecursively<Button>(nameof(ShowAllFloorCards));

            CardViewPrefab = Resources.Load<GameObject>($"Prefabs/{nameof(CardViewPrefab)}");
            CollapsePanel();
        }

        public void AddCard(Card card)
        {
            if (CardViewPrefab != null && FloorCardsHolder != null)
            {
                GameObject newCardViewObject = Instantiate(CardViewPrefab, FloorCardsHolder);
                CardView newCardView = newCardViewObject.GetComponentInChildren<CardView>();

                if (newCardView != null)
                {
                    newCardView.SetCard(card);
                    newCardView.SetActive(true);
                    newCardView.UpdateCardView();

                    for (int i = 0; i < FloorCards.Count; i++)
                    {
                        FloorCards[i].transform.SetSiblingIndex(i);
                    }

                    if (!IsExpanded && FloorCards.Count > 8)
                    {
                        for (int i = 8; i < FloorCards.Count; i++)
                        {
                            FloorCards[i].SetActive(false);
                        }
                    }
                }

                FloorCards.Insert(0, newCardViewObject);

            }
            else
            {
                Debug.LogError($" error adding cards CardViewPrefab null ? {CardViewPrefab == null} FloorCardsHolder null ? {FloorCardsHolder == null}");
            }

        }

        [Button]
        void ToggleExpand()
        {
            if (IsExpanded)
            {
                CollapsePanel();
            }
            else
            {
                ShowAllFloorCards.gameObject.SetActive(false);

                ExpandPanel();

                StartCoroutine(CollapseAfterDelay(15f));
            }

            IsExpanded = !IsExpanded;
        }

        IEnumerator CollapseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            CollapsePanel();

            ShowAllFloorCards.gameObject.SetActive(true);

            IsExpanded = false;
        }

        void ExpandPanel()
        {
            MiddleLayoutElement.preferredHeight = MaxHeight;
            FloorCardsGrid.cellSize = new Vector2(75, 125);

            foreach (GameObject go in FloorCards)
            {
                CardView cardView = go.GetComponent<CardView>();
                if (cardView != null)
                {
                    cardView.SetActive(true);

                }
            }
        }

        void CollapsePanel()
        {
            MiddleLayoutElement.preferredHeight = MinHeight;
            FloorCardsGrid.cellSize = new Vector2(142, 227);

            if (FloorCards.Count > 8)
            {
                for (int i = 8; i < FloorCards.Count; i++)
                {
                    GameObject floorCard = FloorCards[i];
                    CardView cardView = floorCard.GetComponentInChildren<CardView>();
                    cardView.SetActive(false);
                }
            }
        }

        public void ResetView()
        {
            foreach (Transform child in FloorCardsHolder)
            {
                Destroy(child.gameObject);
            }

            FloorCards.Clear();
        }
    }
}
