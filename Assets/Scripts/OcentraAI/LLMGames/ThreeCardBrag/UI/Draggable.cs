using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            if (CanvasGroup == null)
                CanvasGroup = GetComponent<CanvasGroup>();

            if (UIController == null)
                UIController = FindObjectOfType<UIController>();

            if (CardView == null)
            {
                CardView = GetComponentInChildren<CardView>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OriginalPosition = RectTransform.anchoredPosition;
            CanvasGroup.blocksRaycasts = false;
            UIController.OnPickFromFloor();
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform.anchoredPosition += eventData.delta / RectTransform.localScale.x;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CanvasGroup.blocksRaycasts = true;
            RectTransform.anchoredPosition = OriginalPosition;


        }
    }
}