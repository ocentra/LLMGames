using OcentraAI.LLMGames.Screens;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI

{
    public class BragSelectable : Selectable, ISubmitHandler, ICancelHandler, IPointerClickHandler
    {
        public Button Button;
        public UnityEvent OnBragCancel;
        public UnityEvent OnBragDeselected;

        public UnityEvent OnBragSelected;
        public UnityEvent OnBragSubmit;

        [ReadOnly] public UIScreen ParentScreen;

        public bool SelectOnPointerEnter = false;
        public bool SendButtonClickToSubmit = false;
        public Slider Slider;
        public float SliderSensitivity = 1f;
        public Toggle Toggle;

        public void OnCancel(BaseEventData eventData)
        {
            if (ParentScreen != null && !ParentScreen.Interactable)
            {
                return;
            }

            OnBragCancel?.Invoke();
            if (ParentScreen != null)
            {
                ParentScreen.PlayBackGroundSound();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ParentScreen != null && !ParentScreen.Interactable)
            {
                return;
            }

            if (SendButtonClickToSubmit)
            {
                if (Button != null)
                {
                    Button.onClick.Invoke();
                }

                OnBragSubmit?.Invoke();
                if (ParentScreen != null)
                {
                    ParentScreen.PlaySelectionSound();
                }
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (ParentScreen != null && !ParentScreen.Interactable)
            {
                return;
            }

            if (Button != null)
            {
                Button.onClick.Invoke();
            }

            OnBragSubmit?.Invoke();
            if (ParentScreen != null)
            {
                ParentScreen.PlaySelectionSound();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (Toggle != null)
            {
                Toggle.interactable = false;
            }

            if (Button != null)
            {
                Button.interactable = false;
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (Toggle != null)
            {
                Toggle.isOn = true;
            }

            OnBragSelected?.Invoke();
            if (ParentScreen != null)
            {
                ParentScreen.PlaySelectionSound();
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            if (Toggle != null)
            {
                Toggle.isOn = false;
            }

            OnBragDeselected?.Invoke();
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            if (ParentScreen != null && !ParentScreen.Interactable)
            {
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                case MoveDirection.Right:
                    MoveSlider(eventData.moveVector.x);
                    break;
            }

            if (ParentScreen != null)
            {
                ParentScreen.PlayNavigationSound();
            }
        }

        private void MoveSlider(float value)
        {
            if (Slider == null)
            {
                return;
            }

            Slider.value += value * SliderSensitivity * Time.deltaTime;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (SelectOnPointerEnter)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}