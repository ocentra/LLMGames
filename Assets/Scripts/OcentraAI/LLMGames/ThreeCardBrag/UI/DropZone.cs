using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [ShowInInspector, Required] private CardView CardView { get; set; }

        [ShowInInspector, Required] private UIController UIController { get; set; }

        
        private Color originalEmissionColor;

        private Material originalMaterial;

        [ColorUsage(true, true)]
        public Color HighlightColor;



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
                originalMaterial = CardView.HighlightImage.material;
                if (originalMaterial.HasProperty("_EmissionColor"))
                {
                    originalEmissionColor = originalMaterial.GetColor("_EmissionColor");
                }
            }
            
        }

        public void OnDrop(PointerEventData eventData)
        {
            Draggable draggable = eventData.pointerDrag.GetComponent<Draggable>();

            if (draggable != null)
            {
                EventBus.Publish(new PlayerActionPickAndSwap(typeof(HumanPlayer), floorCard: draggable.CardView.Card, swapCard: CardView.Card));
                draggable.SetOverValidDropZone(false);
                SetGlow(false);
                UIController.SetButtonState(ButtonState.DrawnFromDeck);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Draggable draggable = eventData.pointerDrag?.GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.SetOverValidDropZone(true);
                SetGlow(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Draggable draggable = eventData.pointerDrag?.GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.SetOverValidDropZone(false);
                SetGlow(false);
            }
        }

        private void SetGlow(bool isGlowing)
        {


            if (originalMaterial.HasProperty("_EmissionColor"))
            {


                if (isGlowing)
                {
                    var newMaterial = new Material(originalMaterial);
                    newMaterial.SetColor("_EmissionColor", HighlightColor);

                    originalMaterial.SetColor("_EmissionColor", Color.clear);

                    CardView.HighlightImage.material = newMaterial;
                }
                else
                {
                    originalMaterial.SetColor("_EmissionColor", originalEmissionColor);
                    CardView.HighlightImage.material = originalMaterial;
                }
            }
        }
    }
}
