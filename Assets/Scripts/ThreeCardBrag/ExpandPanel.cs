using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace ThreeCardBrag
{
    public class ExpandPanel : MonoBehaviour
    {
        public RectTransform Panel;
        public RectTransform Middle;
        public LayoutElement MiddleLayoutElement;
        public Transform FloorCardsHolder;
        public GridLayoutGroup FloorCardsGrid;
        public Button ShowAllFloorCards;
        public CardView[] FloorCards;
        public float MinHeight = 700;
        public float MaxHeight = 70000;
        public bool IsExpanded = false;

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
            FloorCards = FloorCardsHolder.GetComponentsInChildren<CardView>();
            // Collapse the panel
            CollapsePanel();
        }

        [Button]
        void ToggleExpand()
        {
            if (IsExpanded)
            {
                // Collapse the panel
                CollapsePanel();
            }
            else
            {
                // Expand the panel
                ExpandPanelAndCards();
            }

            IsExpanded = !IsExpanded;
        }

        void ExpandPanelAndCards()
        {
            MiddleLayoutElement.preferredHeight = MaxHeight;
            FloorCardsGrid.cellSize = new Vector2(75, 125);

            foreach (CardView cardView in FloorCards)
            {
                cardView.SetActive(true);
            }
        }

        void CollapsePanel()
        {
            MiddleLayoutElement.preferredHeight = MinHeight;
            FloorCardsGrid.cellSize = new Vector2(142, 227);

            if (FloorCards.Length > 8)
            {
                for (int i = 8; i < FloorCards.Length; i++)
                {
                    FloorCards[i].SetActive(false);
                }
            }
        }
    }
}
