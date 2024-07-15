using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.Utilities;
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
            Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

            if (draggable != null)
            {
                EventBus.Publish(new PlayerActionPickAndSwap(typeof(HumanPlayer), pickCard: draggable.CardView.Card,swapCard: CardView.Card));

            }
        }
    }
}