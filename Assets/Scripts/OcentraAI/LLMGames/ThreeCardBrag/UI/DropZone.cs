using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    public class DropZone : MonoBehaviour, IDropHandler
    {
        [ShowInInspector, Required]
        private CardView CardView { get; set; }

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
            CardView = GetComponent<CardView>();
            UIController = FindObjectOfType<UIController>();

        }

        public void OnDrop(PointerEventData eventData)
        {
            CardView cardView = eventData.pointerDrag.GetComponent<CardView>();
            if (cardView != null)
            {
                UIController.OnDiscardCardSet(cardView);
            }
        }
    }
}