using OcentraAI.LLMGames.GameScreen;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace OcentraAI.LLMGames.UI
{
    public class BragSelectable : Selectable, ISubmitHandler, ICancelHandler, IPointerClickHandler
    {
        public Toggle Toggle;
        public Button Button;
        public Slider Slider;
        public float SliderSensitivity = 1f;
        public bool SelectOnPointerEnter = false;
        public bool SendButtonClickToSubmit = false;

        public UnityEvent OnBragSelected;
        public UnityEvent OnBragDeselected;
        public UnityEvent OnBragSubmit;
        public UnityEvent OnBragCancel;

        [ReadOnly]
        public BragScreen ParentScreen;

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
                return;

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
                return;
            
            Slider.value += value * SliderSensitivity * Time.deltaTime;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (ParentScreen != null && !ParentScreen.Interactable)
                return;

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

        public void OnCancel(BaseEventData eventData)
        {
            if (ParentScreen != null && !ParentScreen.Interactable)
                return;

            OnBragCancel?.Invoke();
            if (ParentScreen != null)
            {
                ParentScreen.PlayBackGroundSound();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ParentScreen != null && !ParentScreen.Interactable)
                return;

            if (SendButtonClickToSubmit)
            {
                if(Button != null)
                    Button.onClick.Invoke();
                OnBragSubmit?.Invoke();
                if (ParentScreen != null)
                {
                    ParentScreen.PlaySelectionSound();
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (SelectOnPointerEnter)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }
        }
    }
}




