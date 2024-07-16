
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [ShowInInspector, Required]
        private CanvasGroup CanvasGroup { get; set; }
       
        [ShowInInspector, Required]
        private Vector2 OriginalPosition { get; set; }
       
        [ShowInInspector, Required]
        private RectTransform RectTransform { get; set; }
       
        [ShowInInspector, Required]
        private UIController UIController { get; set; }
        
        [ShowInInspector, Required]
        public CardView CardView { get; set; }
       
        [ShowInInspector, Required]
        public Image CardHighlight { get; set; }


        [ShowInInspector, Required]
        public Transform PickFromFloor { get; set; }

        [ShowInInspector] public float MaxScaleDownFactor { get; set; } = 0.5f;
        [ShowInInspector] public float MaxDragDistance { get; set; } = 500f;
        private bool isOverValidDropZone = false;
        private Vector3 originalScale;
        void OnValidate()
        {
            Init();
        }
        void Start()
        {
            Init();
        }
        private void Init()
        {
           
            CanvasGroup = GetComponent<CanvasGroup>();
            UIController = FindObjectOfType<UIController>();
            CardView = GetComponentInChildren<CardView>();
            if (CardView != null)
            {
                CardHighlight = CardView.transform.FindChildRecursively<Image>();
            }

            PickFromFloor = transform.FindChildRecursively<Transform>(nameof(PickFromFloor));
            RectTransform = PickFromFloor.GetComponent<RectTransform>();
            originalScale = PickFromFloor.localScale;
        }


        

        public void OnBeginDrag(PointerEventData eventData)
        {
            OriginalPosition = RectTransform.anchoredPosition;
            CanvasGroup.blocksRaycasts = false;
            CardHighlight.color = Color.cyan;
        }
        public void OnDrag(PointerEventData eventData)
        {
            float distance = Vector2.Distance(RectTransform.anchoredPosition, OriginalPosition);
            float scaleFactor = Mathf.Lerp(1f, MaxScaleDownFactor, distance / MaxDragDistance);
            PickFromFloor.localScale = originalScale * scaleFactor;
            RectTransform.anchoredPosition += eventData.delta;

            //RectTransform.anchoredPosition += eventData.delta / RectTransform.localScale.x;

            CardHighlight.color = isOverValidDropZone ? Color.green : Color.red;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            CanvasGroup.blocksRaycasts = true;
            RectTransform.anchoredPosition = OriginalPosition;
            CardHighlight.color = Color.clear;
            ResetScale();

        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CardHighlight != null && !eventData.dragging)
            {
                CardHighlight.color = Color.green;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (CardHighlight != null && !eventData.dragging)
            {
                CardHighlight.color = Color.clear;
            }
        }
        public void SetOverValidDropZone(bool isOver)
        {
            isOverValidDropZone = isOver;
            if (!CanvasGroup.blocksRaycasts)
            {
                CardHighlight.color = isOver ? Color.green : Color.red;
            }
        }
        private void ResetScale()
        {
            PickFromFloor.localScale = originalScale;
        }
    }
}