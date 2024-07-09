using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [ShowInInspector, Required]

        private CanvasGroup CanvasGroup { get;  set; }

        [ShowInInspector, Required]

        private Vector2 OriginalPosition { get; set; }

        [ShowInInspector,Required]
        private RectTransform RectTransform { get; set; }

        [ShowInInspector, Required]
        private UIController UIController { get; set; }


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
            CanvasGroup = FindObjectOfType<CanvasGroup>();
            RectTransform = GetComponent<RectTransform>();
            UIController = FindObjectOfType<UIController>();

        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            OriginalPosition = RectTransform.anchoredPosition;
            CanvasGroup.blocksRaycasts = false;
            UIController.OnPickFromFloor();
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform.anchoredPosition += eventData.delta / RectTransform.localScale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CanvasGroup.blocksRaycasts = true;
            if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<DropZone>() == null)
            {
                RectTransform.anchoredPosition = OriginalPosition;
            }

          //  UIController.OnEndPickFromFloor();
        }
    }
}