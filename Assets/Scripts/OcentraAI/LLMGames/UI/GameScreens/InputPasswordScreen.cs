using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class InputPasswordScreen : UI3DScreen<InputPasswordScreen>
    {
        [SerializeField][Required] private Button inputPasswordButton;
        [SerializeField][Required] private Button cancelButton;
        [SerializeField][Required] private TMP_InputField inputPasswordField;
        private TaskCompletionSource<string> taskCompletionSource;



        #region Event Subscriptions
        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventBus.Instance.SubscribeAsync<InputLobbyPasswordEvent>(OnInputLobbyPassword);

            inputPasswordButton.onClick.AddListener(OnSubmitButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            EventBus.Instance.UnsubscribeAsync<InputLobbyPasswordEvent>(OnInputLobbyPassword);

            inputPasswordButton.onClick.RemoveListener(OnSubmitButtonClick);
            cancelButton.onClick.RemoveListener(OnCancelButtonClick);
        }

        private async UniTask OnInputLobbyPassword(InputLobbyPasswordEvent inputLobbyPassword)
        {
            EventBus.Instance.Publish(new ShowScreenEvent(ScreenName));
            taskCompletionSource = new TaskCompletionSource<string>();


            try
            {
                UniTask<string> passwordTask = taskCompletionSource.Task.AsUniTask();
                await passwordTask;

                if (passwordTask.Status == UniTaskStatus.Succeeded)
                {
                    inputLobbyPassword.PasswordSetSource.TrySetResult(await passwordTask);
                }
                else if (passwordTask.Status == UniTaskStatus.Canceled)
                {
                    inputLobbyPassword.PasswordSetSource.TrySetCanceled();
                }
            }
            catch (Exception ex)
            {
                inputLobbyPassword.PasswordSetSource.TrySetException(ex);
            }
            finally
            {

                ShowLastScreen();
            }
        }

        private void OnCancelButtonClick()
        {
            taskCompletionSource.TrySetCanceled();
        }

        private void OnSubmitButtonClick()
        {
            if (!string.IsNullOrEmpty(inputPasswordField.text))
            {
                taskCompletionSource.TrySetResult(inputPasswordField.text);
            }
        }

        #endregion

       
        protected override void Init(bool startEnabled)
        {
            transform.FindChildWithComponent(ref inputPasswordButton, nameof(inputPasswordButton));
            transform.FindChildWithComponent(ref cancelButton, nameof(cancelButton));
            transform.FindChildWithComponent(ref inputPasswordField, nameof(inputPasswordField));
            base.Init(StartEnabled);
        }

        public override void ShowScreen()
        {
            base.ShowScreen();
            InitializeScreen();
        }

        private void InitializeScreen()
        {
            inputPasswordField.text = string.Empty;
        }
    }
}