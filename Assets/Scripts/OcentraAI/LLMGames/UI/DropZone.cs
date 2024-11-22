using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Players;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.UI.Controllers;
using OcentraAI.LLMGames.UI.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    [RequireComponent(typeof(BoxCollider))]
    public class DropZone : MonoBehaviour
    {
        [ShowInInspector][Required] private CardView CardView { get; set; }
        [ShowInInspector][Required] private UIController UIController { get; set; }

#if UNITY_EDITOR
        private bool IsValidObject(Object obj)
        {
            try
            {
                return obj != null && !ReferenceEquals(obj, null) && obj;
            }
            catch
            {
                return false;
            }
        }

        private bool IsSafeToExecute()
        {
            if (Application.isPlaying) return true;
            return IsValidObject(this) && IsValidObject(gameObject);
        }
#endif

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!IsSafeToExecute()) return;
#endif
            Init();
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !IsSafeToExecute()) return;
#endif

            if (CardView == null)
            {
                CardView = GetComponent<CardView>();
            }

            if (UIController == null)
            {
                UIController = FindAnyObjectByType<UIController>();
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

        public void OnDrop(Draggable draggable)
        {
#if UNITY_EDITOR
            if (!IsSafeToExecute()) return;
#endif

            bool isValidDraggable = draggable != null &&
                                  IsValidObject(draggable.CardView) &&
                                  draggable.CardView != null &&
                                  draggable.CardView.Card != null;

            if (isValidDraggable && IsValidObject(CardView))
            {
                EventBus.Instance.Publish(
                    new PlayerActionPickAndSwapEvent<Card>(
                        typeof(HumanLLMPlayer),
                        draggable.CardView.Card,
                        CardView.Card
                    )
                );
            }

            if (IsValidObject(CardView))
            {
                CardView.SetHighlight(false, CardView.originalHighlightColor);
            }

            if (IsValidObject(UIController))
            {
                UIController.SetButtonState(ButtonState.DrawnFromDeck);
            }

            var mainTableUI = MainTableUI.Instance;
            if (IsValidObject(mainTableUI))
            {
                mainTableUI.ShowDrawnCard(false);
            }
        }

        public void OnDraggableOver(Color color)
        {
#if UNITY_EDITOR
            if (!IsSafeToExecute()) return;
#endif

            if (IsValidObject(CardView))
            {
                CardView.SetHighlight(true, color);
            }
        }

        private void OnDestroy()
        {
            CardView = null;
            UIController = null;
        }
    }
}