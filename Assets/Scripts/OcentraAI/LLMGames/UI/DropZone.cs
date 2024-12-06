using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.UI;
using OcentraAI.LLMGames.UI.Managers;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    [RequireComponent(typeof(BoxCollider))]
    public class DropZone : MonoBehaviour
    {
        [ShowInInspector][Required] private CardView CardView { get; set; }

        private void OnValidate()
        {
            Init();
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (CardView == null)
            {
                CardView = GetComponent<CardView>();
            }

            // Ensure the BoxCollider is set up correctly
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
            }
            else
            {
                Debug.LogError($"DropZone on {gameObject.name} is missing a BoxCollider component!");
            }
        }

        public async void OnDrop(Draggable draggable)
        {

            bool isValidDraggable = draggable != null && draggable.CardView != null && draggable.CardView.Card != null;

            if (isValidDraggable && CardView != null)
            {
                CardView.SetHighlight(false, CardView.OriginalHighlightColor);

                string draggedCard = CardUtility.GetRankSymbol(draggable.CardView.Card.Suit, draggable.CardView.Card.Rank, false);
                string cardInHand = CardUtility.GetRankSymbol(CardView.Card.Suit, CardView.Card.Rank, false);
                PlayerDecisionEvent eventToPublish = new PlayerDecisionPickAndSwapEvent(PlayerDecision.PickAndSwap, cardInHand, draggedCard);
                bool success = await EventBus.Instance.PublishAsync(eventToPublish);

            }

            await UniTask.Yield();
        }

        public void OnDraggableOver(Color color)
        {
            CardView.SetHighlight(true, color);
        }

        private void OnDestroy()
        {
            CardView = null;
        }
    }
}