using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.String;


namespace OcentraAI.LLMGames.Screens
{
    public class MessageHolderScreen : UI3DScreen<MessageHolderScreen>
    {


        [Required, ShowInInspector] private Button AcceptButton { get; set; }
        [Required, ShowInInspector] private TMP_Text AcceptButtonText { get; set; }
        [Required, ShowInInspector] private TMP_Text Message { get; set; }
        [ShowInInspector] private string ButtonName { get; set; } = "Accept";

        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        protected void OnValidate()
        {

            InitReferences();
        }
        

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<UIMessageEvent>(OnMessage);
        }

        private async UniTask OnMessage(UIMessageEvent e)
        {

            ShowScreen();
            string message = e.Message;
            float delay = e.Delay;
            ButtonName = e.ButtonName;

            if (Message != null)
            {
                Message.text = message;
            }

            if (AcceptButtonText != null)
            {
                AcceptButtonText.text = ButtonName;
            }

            if (ButtonName != PlayerDecision.Fold.Name && ButtonName != PlayerDecision.NewGame.Name)
            {
                StartCoroutine(HideMessageAfterDelay(delay));
            }

            await UniTask.Yield();
        }
        private void InitReferences()
        {
            if (Message == null)
            {
                Message = transform.FindChildRecursively<TMP_Text>(nameof(Message));
            }

            if (AcceptButton == null)
            {
                AcceptButton = transform.FindChildRecursively<Button>(nameof(AcceptButton));
            }

            if (AcceptButton != null)
            {
                AcceptButtonText = transform.FindChildRecursively<TMP_Text>(nameof(AcceptButtonText));
                AcceptButton.onClick.AddListener(OnButtonClick);
            }
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
            if (Message != null)
            {
                Message.text = Empty;

            }
        }


        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideScreen();
        }

        private void OnButtonClick()
        {
            if (ButtonName == PlayerDecision.Fold.Name)
            {
                PlayerDecisionEvent playerDecisionEvent = new PlayerDecisionBettingEvent(PlayerDecision.Fold);
                EventBus.Instance.Publish(playerDecisionEvent);
            }

            if (ButtonName == PlayerDecision.NewGame.Name)
            {
                EventBus.Instance.Publish(new PlayerActionStartNewGameEvent());
            }

            ButtonName = "Accept";

            if (AcceptButtonText != null)
            {
                AcceptButtonText.text = ButtonName;
            }

            if (Message != null)
            {
                Message.text = Empty;

            }

            HideScreen();
        }


    }
}