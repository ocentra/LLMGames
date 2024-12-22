using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using OcentraAI.LLMGames.UI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using static System.String;

namespace OcentraAI.LLMGames.Screens
{
    [ExecuteAlways]
    public class ErrorPanel : UI3DScreen<ErrorPanel>
    {
      
        [FoldoutGroup("Expanded"),SerializeField, Required] private Transform mainPanelExpanded;
        [FoldoutGroup("Expanded"), SerializeField, Required] private Button3D collapsedButton;
        [FoldoutGroup("Expanded"), SerializeField, Required] private Button3D expandedPanelOkButton;
        [FoldoutGroup("Expanded"), SerializeField, Required] private TextMeshProUGUI expandedPanelMessageText;

       
        [FoldoutGroup("Collapsed"),SerializeField, Required] private Transform mainPanelCollapsed;
        [FoldoutGroup("Collapsed"), SerializeField, Required] private Button3D expandButton;
        [FoldoutGroup("Collapsed"), SerializeField, Required] private Button3D collapsedPanelOkButton;
        [FoldoutGroup("Collapsed"), SerializeField, Required] private TextMeshProUGUI collapsedPanelMessageText;
      
    
        [FoldoutGroup("Common"), SerializeField] private bool isExpanded = false;



        protected override void Init(bool startEnabled)
        {
            transform.FindChildWithComponent(ref mainPanelExpanded, nameof(mainPanelExpanded));
            mainPanelExpanded.transform.FindChildWithComponent(ref expandedPanelMessageText, nameof(expandedPanelMessageText));
            transform.FindChildWithComponent(ref collapsedButton, nameof(collapsedButton));
            mainPanelExpanded.transform.FindChildWithComponent(ref expandedPanelOkButton, nameof(expandedPanelOkButton));

            transform.FindChildWithComponent(ref mainPanelCollapsed, nameof(mainPanelCollapsed));
            transform.FindChildWithComponent(ref expandButton, nameof(expandButton));
            mainPanelCollapsed.transform.FindChildWithComponent(ref collapsedPanelMessageText, nameof(collapsedPanelMessageText));
            mainPanelCollapsed.transform.FindChildWithComponent(ref collapsedPanelOkButton, nameof(collapsedPanelOkButton));
            base.Init(StartEnabled);
        }



        public override void ShowScreen()
        {
            base.ShowScreen();
            ClearMessage();
            SetPanelState(false);
        }

        public override void HideScreen()
        {
            base.HideScreen();
            ClearMessage();
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if (collapsedPanelOkButton != null)
            {
                collapsedPanelOkButton.onClick.AddListener(OnOkClicked);
            }

            if (expandedPanelOkButton != null)
            {
                expandedPanelOkButton.onClick.AddListener(OnOkClicked);
            }

            if (expandButton != null)
            {
                expandButton.onClick.AddListener(() => SetPanelState(true));
            }

            if (collapsedButton != null)
            {
                collapsedButton.onClick.AddListener(() => SetPanelState(false));
            }

            EventRegistrar.Subscribe<AuthenticationErrorUIEvent>(OnAuthenticationErrorEvent);
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();

            if (collapsedPanelOkButton != null)
            {
                collapsedPanelOkButton.onClick.RemoveListener(OnOkClicked);
            }

            if (expandedPanelOkButton != null)
            {
                expandedPanelOkButton.onClick.RemoveListener(OnOkClicked);
            }

            if (expandButton != null)
            {
                expandButton.onClick.RemoveListener(() => SetPanelState(true));
            }

            if (collapsedButton != null)
            {
                collapsedButton.onClick.RemoveListener(() => SetPanelState(false));
            }

        }

        private void SetPanelState(bool expanded)
        {
            if (mainPanelExpanded != null && mainPanelCollapsed != null)
            {
                mainPanelExpanded.gameObject.SetActive(expanded);
                mainPanelCollapsed.gameObject.SetActive(!expanded);

                if (expandButton != null)
                {
                    expandButton.gameObject.SetActive(!expanded);
                }

                if (collapsedButton != null)
                {
                    collapsedButton.gameObject.SetActive(expanded);
                }

               
               

                isExpanded = !isExpanded;
            }
        }

        private void OnAuthenticationErrorEvent(AuthenticationErrorUIEvent evt)
        {
            ShowErrorMessage(evt.Message);
        }

        private void OnOkClicked()
        {
            HideScreen();
        }

        public void ShowErrorMessage(string message)
        {
            ShowScreen();


            if (collapsedPanelMessageText != null)
            {
                collapsedPanelMessageText.text += $"\n + {message}";
               
            }

            if (expandedPanelMessageText != null)
            {
                expandedPanelMessageText.text += $"\n + {message}";

            }

            GameLoggerScriptable.LogError(message, null);
        }

        private void ClearMessage()
        {
            if (collapsedPanelMessageText != null)
            {
                collapsedPanelMessageText.text = Empty;
            }

            if (expandedPanelMessageText != null)
            {
                expandedPanelMessageText.text = Empty;
            }

        }
    }
}