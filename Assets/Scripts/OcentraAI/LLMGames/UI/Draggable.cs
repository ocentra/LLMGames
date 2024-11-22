using OcentraAI.LLMGames.UI.Controllers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI
{
    public class Draggable : MonoBehaviour
    {
        [ShowInInspector] private readonly float maxDragDistance = 5f;
        [ShowInInspector] private readonly float maxScaleDownFactor = 0.25f;

        [ShowInInspector] private readonly float offsetY = 0.1f;
        [ShowInInspector] [ReadOnly] private float appliedScaleFactor;

        [ShowInInspector] [Required] [ReadOnly]
        private BoxCollider boxCollider;

        [ShowInInspector] [ReadOnly] private DropZone currentDropZone;
        [ShowInInspector] [ReadOnly] private Color currentGlowColor;
        [ShowInInspector] [ReadOnly] private float dragDistance;
        [ShowInInspector] private float dragThreshold = 0.5f;

        private bool isDragging;

        [ColorUsage(true, true)] [SerializeField]
        private readonly Color onMouseDown = Color.cyan;

        [ColorUsage(true, true)] [SerializeField]
        private readonly Color onMouseOverInValidZone = Color.red;

        [ColorUsage(true, true)] [SerializeField]
        private readonly Color onMouseOverValidZone = Color.green;

        [ShowInInspector] [ReadOnly] private Vector3 originalPosition;
        [ShowInInspector] [ReadOnly] private Vector3 originalScale;

        [ShowInInspector]
        [Required]
        [ReadOnly]
        public CardView CardView { get; set; }

        [ShowInInspector]
        [Required]
        [ReadOnly]
        private UIController UIController { get; set; }

        [ShowInInspector]
        [Required]
        [ReadOnly]
        private Camera MainCamera { get; set; }

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
            CardView = GetComponent<CardView>();
            originalScale = transform.localScale;
            MainCamera = Camera.main;
            boxCollider = GetComponent<BoxCollider>();
            UIController = FindAnyObjectByType<UIController>();
        }

        private void OnMouseDown()
        {
            originalPosition = transform.position;
            isDragging = false;
            dragDistance = 0f;

            SetGlowColor(onMouseDown);

            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }

        private void OnMouseDrag()
        {
            Vector3 currentPosition = GetMouseWorldPosition();
            currentPosition.y = originalPosition.y + offsetY;
            transform.position = currentPosition;

            dragDistance = Vector3.Distance(transform.position, originalPosition);

            if (dragDistance > dragThreshold)
            {
                isDragging = true;
            }

            if (isDragging)
            {
                float t = Mathf.Clamp01(dragDistance / maxDragDistance);
                appliedScaleFactor = Mathf.Lerp(1.0f, maxScaleDownFactor, t);
                transform.localScale = originalScale * appliedScaleFactor;

                CheckDropZone();

                if (currentDropZone != null)
                {
                    SetGlowColor(onMouseOverValidZone);
                    currentDropZone.OnDraggableOver(currentGlowColor);
                }
                else
                {
                    SetGlowColor(onMouseOverInValidZone);
                }
            }
            else
            {
                SetGlowColor(onMouseDown);
            }
        }

        private void OnMouseUp()
        {
            ResetPositionAndScale();
            isDragging = false;

            SetGlowColor(CardView.originalHighlightColor);
            CardView.HighlightCard.enabled = true;

            if (currentDropZone != null)
            {
                currentDropZone.OnDrop(this);
                CardView.HighlightCard.enabled = false;
                currentDropZone = null;
            }
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = MainCamera.WorldToScreenPoint(transform.position).z;
            return MainCamera.ScreenToWorldPoint(mousePoint);
        }

        private void ResetPositionAndScale()
        {
            transform.localScale = originalScale;
            transform.position = originalPosition;
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
            }

            dragDistance = 0;
            appliedScaleFactor = 0;
        }

        private void CheckDropZone()
        {
            foreach (CardView cardView in UIController.LocalPlayerCardViews)
            {
                cardView.SetHighlight(false);
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
            {
                DropZone dropZone = hit.collider.GetComponent<DropZone>();
                currentDropZone = dropZone;
            }
            else
            {
                currentDropZone = null;
            }
        }

        private void SetGlowColor(Color newColor)
        {
            if (currentGlowColor != newColor)
            {
                currentGlowColor = newColor;
                CardView.SetHighlight(true, currentGlowColor);
            }
        }
    }
}