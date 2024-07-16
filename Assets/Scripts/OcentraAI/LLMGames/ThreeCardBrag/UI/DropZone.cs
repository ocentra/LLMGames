using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [ShowInInspector, Required]
        private CardView CardView { get; set; }

        [ShowInInspector, Required]
        private UIController UIController { get; set; }

        [ShowInInspector, Required]
        public Image CardHighlight { get; set; }

        private Color originalColor;

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
            if (CardView != null)
            {
                CardHighlight = CardView.transform.FindChildRecursively<Image>();
                originalColor = CardHighlight.color; 
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

            if (draggable != null)
            {
                EventBus.Publish(new PlayerActionPickAndSwap(typeof(HumanPlayer), pickCard: draggable.CardView.Card, swapCard: CardView.Card));
                draggable.SetOverValidDropZone(false);
                CardHighlight.color = originalColor; 
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Draggable draggable = eventData.pointerDrag?.GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.SetOverValidDropZone(true);
                originalColor = CardHighlight.color; 
                CardHighlight.color = Color.cyan; 
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Draggable draggable = eventData.pointerDrag?.GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.SetOverValidDropZone(false);
                CardHighlight.color = originalColor; 
            }
        }
    }
}
