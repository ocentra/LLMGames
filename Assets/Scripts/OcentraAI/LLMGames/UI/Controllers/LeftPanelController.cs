using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.UI.Controllers
{
    [ExecuteAlways]
    public class LeftPanelController : SerializedMonoBehaviour, IEventHandler
    {
        [SerializeField, ShowInInspector] protected float ExpandDuration = 15f;

        [SerializeField, ShowInInspector] protected int MaxVisibleCards = 12;
        [Required, ShowInInspector, ReadOnly] private Canvas CardHolder { get; set; }
        [Required, ShowInInspector, ReadOnly] private GameObject CardHolder3D { get; set; }
        [Required, ShowInInspector, ReadOnly] public GameObject CardViewPrefab { get; set; }
        [ShowInInspector, ReadOnly] private Vector4 PanelSize { get; set; }
        [ShowInInspector, ReadOnly] private bool IsExpanded { get; set; }

        [DictionaryDrawerSettings(KeyLabel = "Players"), SerializeField, ShowInInspector]
        protected Dictionary<int, Vector4> PlayerCountBlendShapes;
        [Required, ShowInInspector, ReadOnly] private ScrollRect ScrollView { get; set; }
        [Required, ShowInInspector, ReadOnly] private RectTransform ScrollViewContent { get; set; }
        [Required, ShowInInspector, ReadOnly] private GridLayoutGroup ScrollViewGridLayoutGroup { get; set; }
        [Required, ShowInInspector, ReadOnly] private Button3D ShowAllFloorCards { get; set; }
        [Required, ShowInInspector, ReadOnly] private SkinnedMeshRenderer SkinnedMeshRenderer { get; set; }

        [ShowInInspector, ReadOnly] protected float RemainingTime = 0;
        [ShowInInspector, ReadOnly] private GameObject Counter { get; set; }
        [ShowInInspector, ReadOnly] private TextMeshPro CountdownText { get; set; }
        [ShowInInspector, ReadOnly] private GameObject RingRed { get; set; }
        [ShowInInspector, ReadOnly] private MeshRenderer RingRedRenderer { get; set; }
        [ShowInInspector] private Material RingRedMaterial { get; set; }

        [ShowInInspector] public int NumberOfPlayers { get; set; } = 2;

        protected CancellationTokenSource ToggleCancellationTokenSource;

        [ShowInInspector, ReadOnly]
        private Dictionary<Card, GameObject> FloorCardMap { get; } = new Dictionary<Card, GameObject>();

        [ShowInInspector, Required] public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();


        private void Awake()
        {
            SubscribeToEvents();
            Init();
        }

        private void OnValidate()
        {
            Init();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        [Button("Copy Previous Value", ButtonSizes.Small), EnableIf("@NumberOfPlayers > 2 && NumberOfPlayers <= 10")]
        private void CopyPreviousValue()
        {

            if (PlayerCountBlendShapes.TryGetValue(NumberOfPlayers - 1, out Vector4 value))
            {

                PlayerCountBlendShapes[NumberOfPlayers] = value;
            }

        }


        private void Init()
        {
            if (PlayerCountBlendShapes == null || PlayerCountBlendShapes.Count == 0)
            {
                PlayerCountBlendShapes = new Dictionary<int, Vector4>
                {
                    { 2, new Vector4(16, 30,105,160) },
                    { 3, new Vector4(16, 30,105,160) },
                    { 4, new Vector4(16, 30, 105, 160) },
                    { 5, new Vector4(16, 30, 105, 160) },
                    { 6, new Vector4(16, 30, 105, 160) },
                    { 7, new Vector4(16, 30, 105, 160) },
                    { 8, new Vector4(16, 30, 105, 160) },
                    { 9, new Vector4(16, 30, 105, 160) },
                    { 10, new Vector4(16, 30, 105, 160) }
                };
            }

            FindComponents();
            SetInitialState();
            LoadCardViewPrefab();
        }

        public void SubscribeToEvents()
        {
            EventRegistrar.Subscribe<UpdateFloorCardListEvent<Card>>(OnUpdateFloorCardList);
            EventRegistrar.Subscribe<ShowAllFloorCardEvent>(OnShowAllFloorCardEvent);
            EventRegistrar.Subscribe<RegisterPlayerListEvent>(OnRegisterPlayerListEvent);
        }

        public void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();

        }

        public void UpdatePanelSize()
        {
            if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                if (IsExpanded)
                {
                    if (PlayerCountBlendShapes.TryGetValue(NumberOfPlayers, out Vector4 value))
                    {
                        PanelSize = value;
                    }
                }
                else
                {
                    PanelSize = new Vector4(0, 0, 65, 100);
                }

                SkinnedMeshRenderer.SetBlendShapeWeight(0, PanelSize.y);
                SkinnedMeshRenderer.SetBlendShapeWeight(1, PanelSize.x);

                if (CardHolder != null)
                {
                    RectTransform cardHolderRect = CardHolder.GetComponent<RectTransform>();
                    cardHolderRect.sizeDelta = new Vector2(PanelSize.z, PanelSize.w);

                    if (ScrollView != null)
                    {
                        RectTransform scrollViewRect = ScrollView.GetComponent<RectTransform>();
                        scrollViewRect.anchorMin = Vector2.zero;
                        scrollViewRect.anchorMax = Vector2.one;
                        scrollViewRect.offsetMin = Vector2.zero;
                        scrollViewRect.offsetMax = Vector2.zero;
                    }
                }
            }
        }


        private void FindComponents()
        {
            CountdownText = transform.FindChildRecursively<TextMeshPro>(nameof(CountdownText), true);

            RingRed = transform.RecursiveFindChildGameObject(nameof(RingRed), true);
            if (RingRed != null)
            {
                RingRedRenderer = RingRed.GetComponent<MeshRenderer>();
                if (RingRedRenderer != null)
                {
                    RingRedMaterial = RingRedRenderer.sharedMaterial;
                }
            }


            Counter = transform.RecursiveFindChildGameObject(nameof(Counter), true);

            if (Counter != null)
            {
                Counter.SetActive(false);

                if (CountdownText != null)
                {
                    CountdownText.text = Mathf.CeilToInt(ExpandDuration).ToString();
                }
            }

            CardHolder3D = transform.FindChildRecursively<Transform>(nameof(CardHolder3D), true).gameObject;
            CardHolder = transform.FindChildRecursively<Canvas>(nameof(CardHolder), true);

            if (CardHolder != null)
            {
                ScrollView = CardHolder.transform.FindChildRecursively<ScrollRect>(nameof(ScrollView), true);
                ScrollViewGridLayoutGroup = CardHolder.transform.FindChildRecursively<GridLayoutGroup>();
                if (ScrollViewGridLayoutGroup != null)
                {
                    ScrollViewContent = ScrollViewGridLayoutGroup.GetComponent<RectTransform>();
                }
            }

            ShowAllFloorCards = transform.FindChildRecursively<Button3D>(nameof(ShowAllFloorCards), true);

            if (CardHolder3D != null && SkinnedMeshRenderer == null)
            {
                SkinnedMeshRenderer = CardHolder3D.GetComponent<SkinnedMeshRenderer>();
            }
        }

        private void SetInitialState()
        {
            IsExpanded = false;
            if (CardHolder3D != null)
            {
                CardHolder3D.SetActive(true);
            }

            FloorCardMap.Clear();
            UpdateCardVisibility();
        }

        private void LoadCardViewPrefab()
        {
            CardViewPrefab = Resources.Load<GameObject>($"Prefabs/{nameof(CardViewPrefab)}");
            if (CardViewPrefab == null)
            {
                Debug.LogError("Failed to load CardViewPrefab from Resources.");
            }
        }


        private void UpdateCardVisibility()
        {
            LeftPanelCardView[] objects = ScrollViewContent.GetComponentsInChildren<LeftPanelCardView>(true);
            for (int i = 0; i < objects.Length; i++)
            {
                LeftPanelCardView cardObject = objects[i];
                cardObject.gameObject.SetActive(IsExpanded || i < MaxVisibleCards);
            }

            ScrollView.vertical = IsExpanded;
        }



        private void OnRegisterPlayerListEvent(RegisterPlayerListEvent registerPlayerListEvent)
        {
            NumberOfPlayers = registerPlayerListEvent.Players.Count;
        }


        [Button("Toggle OnShowAllFloorCardEvent")]
        private void OnShowAllFloorCardEvent(ShowAllFloorCardEvent obj)
        {
            ToggleExpand();
        }

        [Button("Toggle Expand")]
        private async void ToggleExpand(bool expand = true)
        {
            IsExpanded = expand;

            ToggleCancellationTokenSource?.Cancel();
            ToggleCancellationTokenSource = new CancellationTokenSource();

            if (CardHolder3D != null)
            {
                UpdatePanelSize();
            }

            if (Counter != null)
            {
                Counter.SetActive(IsExpanded);

                if (CountdownText != null)
                {
                    string countdownTextText = Mathf.CeilToInt(ExpandDuration).ToString();
                    CountdownText.text = countdownTextText;
                }

                if (RingRed != null)
                {
                    RingRed.SetActive(IsExpanded);
                }
            }

            ShowAllFloorCards.gameObject.SetActive(!IsExpanded);
            UpdateCardVisibility();

            LayoutRebuilder.ForceRebuildLayoutImmediate(CardHolder.GetComponent<RectTransform>());

            if (IsExpanded)
            {

                try
                {
                    await CollapseAfterDelay(ToggleCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    RemainingTime = ExpandDuration;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An error occurred during CollapseAfterDelay: {ex}");
                }
            }
        }


        private async UniTask CollapseAfterDelay(CancellationToken cancellationToken)
        {
            float startTime = Time.realtimeSinceStartup;
            RemainingTime = ExpandDuration;

            while (Time.realtimeSinceStartup - startTime < ExpandDuration)
            {

                RemainingTime = Mathf.Max(0, ExpandDuration - (Time.realtimeSinceStartup - startTime));
                string countdownTextText = Mathf.CeilToInt(RemainingTime).ToString();

                if (CountdownText != null)
                {
                    CountdownText.text = countdownTextText;
                }

                if (RingRedMaterial != null && RingRedRenderer != null)
                {
                    float fillAmount = 1 - (RemainingTime / ExpandDuration);
                    MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                    RingRedRenderer.GetPropertyBlock(propBlock);
                    propBlock.SetFloat("_FillAmount", fillAmount);
                    RingRedRenderer.SetPropertyBlock(propBlock);
                }

                await UniTask.Delay(100, cancellationToken: cancellationToken);
            }



            ToggleExpand(false);
        }



        private void OnUpdateFloorCardList(UpdateFloorCardListEvent<Card> updateFloorCardListEvent)
        {
            if (updateFloorCardListEvent.Reset)
            {
                ResetView();
                return;
            }

            List<Card> cards = updateFloorCardListEvent.FloorCards;

            if (cards == null || CardViewPrefab == null || ScrollViewContent == null)
            {
                Debug.LogError($"Error adding cards. Cards list, prefab, or scroll view is null.");
                return;
            }

            foreach (var card in cards)
            {
                if (card == null)
                {
                    Debug.LogWarning("Null card detected in FloorCards list, skipping.");
                    continue;
                }

                if (FloorCardMap.TryGetValue(card, out GameObject existingCardObject))
                {
                    existingCardObject.transform.SetSiblingIndex(0);
                }
                else
                {
                    GameObject newCardViewObject = Instantiate(CardViewPrefab, ScrollViewContent);
                    newCardViewObject.name = $"{card.name}";
                    LeftPanelCardView newCardView = newCardViewObject.GetComponentInChildren<LeftPanelCardView>();

                    if (newCardView != null)
                    {
                        newCardView.SetCard(card);
                        FloorCardMap[card] = newCardViewObject;

                        newCardViewObject.transform.SetSiblingIndex(0);
                    }
                    else
                    {
                        Destroy(newCardViewObject);
                        Debug.LogError("Failed to find CardView component in the instantiated prefab.");
                    }
                }
            }

            UpdateCardVisibility();
        }


        public void ResetView()
        {

            LeftPanelCardView[] objects = ScrollViewContent.GetComponentsInChildren<LeftPanelCardView>(true);
            for (int i = 0; i < objects.Length; i++)
            {
                LeftPanelCardView cardObject = objects[i];
                Destroy(cardObject);
            }

            FloorCardMap.Clear();
            UpdateCardVisibility();
        }




    }
}