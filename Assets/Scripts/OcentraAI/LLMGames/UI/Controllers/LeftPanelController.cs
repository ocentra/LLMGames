using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.UI;
using OcentraAI.LLMGames.UI.Managers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers
{
    [ExecuteAlways]
    public class LeftPanelController : MonoBehaviour
    {
        private const int MaxVisibleCards = 8;

        private readonly List<GameObject> floorCards = new List<GameObject>();
        [Required] [SerializeField] private Canvas cardHolder;

        [Required] [SerializeField] private GameObject cardHolder3D;
        [Required] public GameObject CardViewPrefab;
        [ShowInInspector] private float height;
        [SerializeField] [ReadOnly] private bool isExpanded;
        [SerializeField] private List<PlayerBlendShapeSettings> playerCountBlendShapes;
        [Required] [SerializeField] private ScrollRect scrollView;
        [Required] [SerializeField] private RectTransform scrollViewContent;
        [Required] [SerializeField] private GridLayoutGroup scrollViewGridLayoutGroup;
        [Required] [SerializeField] private Button3D showAllFloorCards;

        [Required] [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [ShowInInspector] private float width;


        private void Start()
        {
            Init();
            showAllFloorCards.onClick.AddListener(ToggleExpand);
        }

        private void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            InitializePlayerCountBlendShapes();

            FindComponents();
            SetInitialState();
            LoadCardViewPrefab();
        }

        private void InitializePlayerCountBlendShapes()
        {
            if (playerCountBlendShapes == null || playerCountBlendShapes.Count == 0)
            {
                playerCountBlendShapes = new List<PlayerBlendShapeSettings>
                {
                    new PlayerBlendShapeSettings(2, new Vector2(10, 20)),
                    new PlayerBlendShapeSettings(3, new Vector2(15, 25)),
                    new PlayerBlendShapeSettings(4, new Vector2(20, 30)),
                    new PlayerBlendShapeSettings(5, new Vector2(25, 35)),
                    new PlayerBlendShapeSettings(6, new Vector2(30, 40)),
                    new PlayerBlendShapeSettings(7, new Vector2(35, 45)),
                    new PlayerBlendShapeSettings(8, new Vector2(40, 50)),
                    new PlayerBlendShapeSettings(9, new Vector2(45, 55)),
                    new PlayerBlendShapeSettings(10, new Vector2(50, 60))
                };
            }
        }


        private void UpdateBlendShape()
        {
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                if (isExpanded)
                {
                    PlayerBlendShapeSettings settings =
                        playerCountBlendShapes.Find(x => x.PlayerCount == MainTableUI.Instance.PlayerCount);
                    if (settings != null)
                    {
                        width = settings.BlendShapeValues.x;
                        height = settings.BlendShapeValues.y;
                    }
                }
                else
                {
                    width = 0;
                    height = 0;
                }

                skinnedMeshRenderer.SetBlendShapeWeight(0, width);
                skinnedMeshRenderer.SetBlendShapeWeight(1, height);
            }
        }

        private void FindComponents()
        {
            cardHolder3D = transform.FindChildRecursively<Transform>(nameof(cardHolder3D), true).gameObject;
            cardHolder = transform.FindChildRecursively<Canvas>(nameof(cardHolder), true);

            if (cardHolder != null)
            {
                scrollView = cardHolder.transform.FindChildRecursively<ScrollRect>(nameof(scrollView), true);
                scrollViewGridLayoutGroup = cardHolder.transform.FindChildRecursively<GridLayoutGroup>();
                if (scrollViewGridLayoutGroup != null)
                {
                    scrollViewContent = scrollViewGridLayoutGroup.GetComponent<RectTransform>();
                }
            }

            showAllFloorCards = transform.FindChildRecursively<Button3D>(nameof(showAllFloorCards), true);

            if (cardHolder3D != null && skinnedMeshRenderer == null)
            {
                skinnedMeshRenderer = cardHolder3D.GetComponent<SkinnedMeshRenderer>();
            }
        }

        private void SetInitialState()
        {
            isExpanded = false;
            if (cardHolder3D != null)
            {
                cardHolder3D.SetActive(true);
            }

            floorCards.Clear();
        }

        private void LoadCardViewPrefab()
        {
            CardViewPrefab = Resources.Load<GameObject>($"Prefabs/{nameof(CardViewPrefab)}");
            if (CardViewPrefab == null)
            {
                Debug.LogError("Failed to load CardViewPrefab from Resources.");
            }
        }

        public void AddCard(Card card, bool reset)
        {
            if (reset)
            {
                ResetView();
                return;
            }

            if (CardViewPrefab != null && scrollViewContent != null && card != null)
            {
                GameObject newCardViewObject = Instantiate(CardViewPrefab, scrollViewContent);
                CardView newCardView = newCardViewObject.GetComponentInChildren<CardView>();

                if (newCardView != null)
                {
                    newCardView.SetCard(card);
                    newCardView.SetActive(true);
                    newCardView.UpdateCardView();

                    floorCards.Insert(0, newCardViewObject);
                    newCardViewObject.transform.SetSiblingIndex(0);

                    UpdateCardVisibility();
                }
                else
                {
                    Destroy(newCardViewObject);
                    Debug.LogError("Failed to find CardView component in the instantiated prefab.");
                }
            }
            else
            {
                Debug.LogError(
                    $"Error adding cards. CardViewPrefab null? {CardViewPrefab == null} ScrollViewContent null? {scrollViewContent == null}");
            }
        }

        private void UpdateCardVisibility()
        {
            for (int i = 0; i < floorCards.Count; i++)
            {
                floorCards[i].SetActive(isExpanded || i < MaxVisibleCards);
            }
        }

        [Button("Toggle Expand")]
        private void ToggleExpand()
        {
            isExpanded = !isExpanded;
            if (cardHolder3D != null)
            {
                UpdateBlendShape();
            }

            showAllFloorCards.gameObject.SetActive(!isExpanded);
            RectTransform canvasRect = cardHolder.GetComponent<RectTransform>();

            UpdateCardVisibility();

            if (isExpanded)
            {
                StartCoroutine(CollapseAfterDelay(15f));
            }
        }


        private IEnumerator CollapseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (isExpanded)
            {
                ToggleExpand();
            }
        }

        public void ResetView()
        {
            foreach (Transform child in scrollViewContent)
            {
                Destroy(child.gameObject);
            }

            floorCards.Clear();
            UpdateCardVisibility();
        }

        [Serializable]
        public class PlayerBlendShapeSettings
        {
            [HorizontalGroup("BlendShapeSettings", LabelWidth = 80)] [HideLabel]
            public Vector2 BlendShapeValues;

            [HorizontalGroup("BlendShapeSettings", LabelWidth = 80)] [ReadOnly]
            public int PlayerCount;

            public PlayerBlendShapeSettings(int playerCount, Vector2 blendShapeValues)
            {
                PlayerCount = playerCount;
                BlendShapeValues = blendShapeValues;
            }
        }
    }
}