using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Screens3D;
using OcentraAI.LLMGames.UI;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.String;

namespace OcentraAI.LLMGames.Screens
{
    [ExecuteAlways]
    public class LoginScreen : UI3DScreen<LoginScreen>
    {
        private bool IsCreateFrameActive { get; set; }
        private bool IsLoginFrameActive { get; set; }

        // Group: AuthenticationUI
        [FoldoutGroup("AuthenticationUI"), OdinSerialize, ShowInInspector, Required] private Transform AuthenticationUI { get; set; }
        [FoldoutGroup("AuthenticationUI"), OdinSerialize, ShowInInspector, Required] private Transform AuthenticationUITabs { get; set; }
        [FoldoutGroup("AuthenticationUI"), OdinSerialize, ShowInInspector, Required] private Transform AuthenticationUIFrameCreate { get; set; }
        [FoldoutGroup("AuthenticationUI"), OdinSerialize, ShowInInspector, Required] private Transform AuthenticationUIFrameLogin { get; set; }
        [FoldoutGroup("AuthenticationUI"), OdinSerialize, ShowInInspector, Required] private Transform UserAndEmailRegistrationUI { get; set; }
        [FoldoutGroup("AuthenticationUI"), OdinSerialize, ShowInInspector, Required] private Transform GuestLogInUI { get; set; }
        [FoldoutGroup("AuthenticationUI"), Header("FieldValidator"), OdinSerialize, ShowInInspector, Required] private TextMeshPro FieldValidator { get; set; }

        // Group: Tabs
        [FoldoutGroup("Tabs"), OdinSerialize, ShowInInspector, Required] private Button3D CreateNewTab { get; set; }
        [FoldoutGroup("Tabs"), OdinSerialize, ShowInInspector, Required] private Button3D LogInTab { get; set; }

        // Group: Create - UserName Section
        [FoldoutGroup("Create"), Header("UserName"), OdinSerialize, ShowInInspector, Required] private Transform UserNameHolder { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private TMP_InputField UserNameInputField { get; set; }

        // Group: Create - Email Section
        [FoldoutGroup("Create"), Header("Email"), OdinSerialize, ShowInInspector, Required] private Transform EmailHolder { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private TMP_InputField EmailInputField { get; set; }

        // Group: Create - Password Section
        [FoldoutGroup("Create"), Header("Password"), OdinSerialize, ShowInInspector, Required] private Transform PasswordHolder { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private TMP_InputField PasswordInputField { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private Button ShowPassword { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private Button HidePassword { get; set; }

        // Group: Create - PasswordVerify Section
        [FoldoutGroup("Create"), Header("PasswordVerify"), OdinSerialize, ShowInInspector, Required] private Transform PasswordVerifyHolder { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private TMP_InputField PasswordInputVerifyField { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private Button ShowVerifyPassword { get; set; }
        [FoldoutGroup("Create"), OdinSerialize, ShowInInspector, Required] private Button HideVerifyPassword { get; set; }

        // Group: Create - Button Section
        [FoldoutGroup("Create"), Header("Button"), OdinSerialize, ShowInInspector, Required] private Button3D Create { get; set; }

        // Group: LogIn - UserName Section
        [FoldoutGroup("LogIn"), Header("UserName"), OdinSerialize, ShowInInspector, Required] private Transform LogInUserNameHolder { get; set; }
        [FoldoutGroup("LogIn"), OdinSerialize, ShowInInspector, Required] private TMP_InputField LogInUserNameInputField { get; set; }

        // Group: LogIn - Password Section
        [FoldoutGroup("LogIn"), Header("Password"), OdinSerialize, ShowInInspector, Required] private Transform LogInPasswordHolder { get; set; }
        [FoldoutGroup("LogIn"), OdinSerialize, ShowInInspector, Required] private TMP_InputField LogInPasswordInputField { get; set; }
        [FoldoutGroup("LogIn"), OdinSerialize, ShowInInspector, Required] private Button LogInShowPassword { get; set; }
        [FoldoutGroup("LogIn"), OdinSerialize, ShowInInspector, Required] private Button LogInHidePassword { get; set; }

        // Group: LogIn - Button Section
        [FoldoutGroup("LogIn"), Header("Button"), OdinSerialize, ShowInInspector, Required] private Button3D LogIn { get; set; }

        // Group: Account Creation Choice
        [FoldoutGroup("Account Creation Choice"), OdinSerialize, ShowInInspector, Required] private Button3D SignInWithUnity { get; set; }
        [FoldoutGroup("Account Creation Choice"), OdinSerialize, ShowInInspector, Required] private Button3D SignInWithGoogle { get; set; }
        [FoldoutGroup("Account Creation Choice"), OdinSerialize, ShowInInspector, Required] private Button3D SignInWithFacebook { get; set; }
        [FoldoutGroup("Account Creation Choice"), OdinSerialize, ShowInInspector, Required] private Button3D SignInWithAsGuest { get; set; }
        [FoldoutGroup("Account Creation Choice"), OdinSerialize, ShowInInspector, Required] private Button3D SignInWithSteam { get; set; }
        // Group: UserAndEmail
        [FoldoutGroup("UserAndEmail"), OdinSerialize, ShowInInspector, Required] private Button3D SubmitNameAndEmail { get; set; }
        [FoldoutGroup("UserAndEmail"), OdinSerialize, ShowInInspector, Required] private TMP_InputField SubmitEmailInputField { get; set; }
        [FoldoutGroup("UserAndEmail"), OdinSerialize, ShowInInspector, Required] private TMP_InputField SubmitUserNameInputField { get; set; }

        [FoldoutGroup("Guest"), OdinSerialize, ShowInInspector, Required] private Button3D SubmitNickName { get; set; }
        [FoldoutGroup("Guest"), OdinSerialize, ShowInInspector, Required] private TMP_InputField NickNameInputField { get; set; }

        [ReadOnly] public static Regex NameRegex => new Regex(@"^[a-zA-Z0-9.\-@_]{3,20}$");

        [ReadOnly] public static Regex EmailRegex => new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        [ReadOnly]
        public static Regex PasswordRegex => new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,30}$");

        [SerializeField] private bool IsLogInTabActive { get; set; } = false;
        


        protected override void Init(bool startEnabled)
        {

            FieldValidator = transform.FindChildRecursively<TextMeshPro>(nameof(FieldValidator));

            SignInWithUnity = transform.FindChildRecursively<Button3D>(nameof(SignInWithUnity));
            SignInWithGoogle = transform.FindChildRecursively<Button3D>(nameof(SignInWithGoogle));
            SignInWithFacebook = transform.FindChildRecursively<Button3D>(nameof(SignInWithFacebook));
            SignInWithAsGuest = transform.FindChildRecursively<Button3D>(nameof(SignInWithAsGuest));
            SignInWithSteam = transform.FindChildRecursively<Button3D>(nameof(SignInWithSteam));

            AuthenticationUI = transform.FindChildRecursively(nameof(AuthenticationUI));
            AuthenticationUITabs = transform.FindChildRecursively(nameof(AuthenticationUITabs));

            AuthenticationUIFrameCreate = transform.FindChildRecursively(nameof(AuthenticationUIFrameCreate));
            AuthenticationUIFrameLogin = transform.FindChildRecursively(nameof(AuthenticationUIFrameLogin));

            UserAndEmailRegistrationUI = transform.FindChildRecursively(nameof(UserAndEmailRegistrationUI));
            GuestLogInUI = transform.FindChildRecursively(nameof(GuestLogInUI));

            if (AuthenticationUIFrameLogin != null)
            {
                AuthenticationUIFrameLogin.gameObject.SetActive(false);
            }

            CreateNewTab = transform.FindChildRecursively<Button3D>(nameof(CreateNewTab));
            if (CreateNewTab != null)
            {
                CreateNewTab.SetInteractable(false);
            }

            LogInTab = transform.FindChildRecursively<Button3D>(nameof(LogInTab));
            if (LogInTab != null)
            {
                LogInTab.SetInteractable(true);
            }


            UserNameHolder = transform.FindChildRecursively(nameof(UserNameHolder));
            if (UserNameHolder != null)
            {
                UserNameInputField = UserNameHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));
            }

            PasswordHolder = transform.FindChildRecursively(nameof(PasswordHolder));
            if (PasswordHolder != null)
            {
                PasswordInputField = PasswordHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));
                ShowPassword = PasswordHolder.transform.FindChildRecursively<Button>(nameof(ShowPassword));
                HidePassword = PasswordHolder.transform.FindChildRecursively<Button>(nameof(HidePassword));
            }

            EmailHolder = transform.FindChildRecursively(nameof(EmailHolder));
            if (EmailHolder != null)
            {
                EmailInputField = EmailHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));
            }



            PasswordVerifyHolder = transform.FindChildRecursively(nameof(PasswordVerifyHolder));
            if (PasswordVerifyHolder != null)
            {
                PasswordInputVerifyField = PasswordVerifyHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));
                ShowVerifyPassword = PasswordVerifyHolder.transform.FindChildRecursively<Button>(nameof(ShowPassword));
                HideVerifyPassword = PasswordVerifyHolder.transform.FindChildRecursively<Button>(nameof(HidePassword));
            }

            LogInUserNameHolder = transform.FindChildRecursively(nameof(LogInUserNameHolder));
            if (LogInUserNameHolder != null)
            {
                LogInUserNameInputField = LogInUserNameHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));
            }

            LogInPasswordHolder = transform.FindChildRecursively(nameof(LogInPasswordHolder));
            if (LogInPasswordHolder != null)
            {
                LogInPasswordInputField = LogInPasswordHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));
                LogInShowPassword = LogInPasswordHolder.transform.FindChildRecursively<Button>(nameof(ShowPassword));
                LogInHidePassword = LogInPasswordHolder.transform.FindChildRecursively<Button>(nameof(HidePassword));
            }



            if (UserAndEmailRegistrationUI != null)
            {
                SubmitNameAndEmail = UserAndEmailRegistrationUI.transform.FindChildRecursively<Button3D>(nameof(SubmitNameAndEmail));
                Transform userNameHolder = UserAndEmailRegistrationUI.transform.FindChildRecursively(nameof(UserNameHolder));
                if (userNameHolder != null)
                {


                    SubmitUserNameInputField = userNameHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));

                }

                Transform emailHolder = UserAndEmailRegistrationUI.transform.FindChildRecursively(nameof(EmailHolder));
                if (emailHolder != null)
                {
                    SubmitEmailInputField = emailHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));

                }

                UserAndEmailRegistrationUI.gameObject.SetActive(false);
            }


            if (GuestLogInUI != null)
            {
                SubmitNickName = GuestLogInUI.transform.FindChildRecursively<Button3D>(nameof(SubmitNickName));
                Transform userNameHolder = GuestLogInUI.transform.FindChildRecursively(nameof(UserNameHolder));
                if (userNameHolder != null)
                {
                    NickNameInputField = userNameHolder.transform.FindChildRecursively<TMP_InputField>(nameof(InputField));

                }
                GuestLogInUI.gameObject.SetActive(false);

            }

            Create = transform.FindChildRecursively<Button3D>(nameof(Create));
            LogIn = transform.FindChildRecursively<Button3D>(nameof(LogIn));

            IsCreateFrameActive = AuthenticationUIFrameCreate.gameObject.activeSelf;
            IsLoginFrameActive = AuthenticationUIFrameLogin.gameObject.activeSelf;

            base.Init(StartEnabled);

        }

        public override void ShowScreen()
        {
            base.ShowScreen();

            if (AuthenticationUI != null)
            {
                AuthenticationUI.gameObject.SetActive(true);
            }

            if (AuthenticationUIFrameCreate != null)
            {
                AuthenticationUIFrameCreate.gameObject.SetActive(true);
            }

            if (AuthenticationUIFrameLogin != null)
            {
                AuthenticationUIFrameLogin.gameObject.SetActive(false);
            }

            if (UserAndEmailRegistrationUI != null)
            {
                UserAndEmailRegistrationUI.gameObject.SetActive(false);
            }

            if (GuestLogInUI != null)
            {
                GuestLogInUI.gameObject.SetActive(false);
            }

            if (AuthenticationUITabs != null)
            {
                AuthenticationUITabs.gameObject.SetActive(AuthenticationUIFrameLogin != null && AuthenticationUIFrameCreate != null && (AuthenticationUIFrameCreate.gameObject.activeInHierarchy || AuthenticationUIFrameLogin.gameObject.activeInHierarchy));
            }


            if (Create != null)
            {
                Create.SetInteractable(ValidateUserAndPassword(UserNameInputField, PasswordInputField));
            }

            if (LogIn != null)
            {
                LogIn.SetInteractable(ValidateUserAndPassword(LogInUserNameInputField, LogInPasswordInputField));
            }

            if (HideVerifyPassword != null && ShowVerifyPassword != null)
            {
                SwitchInputType(TMP_InputField.ContentType.Password, HideVerifyPassword, ShowVerifyPassword);

            }

            if (HidePassword != null && ShowPassword != null)
            {
                SwitchInputType(TMP_InputField.ContentType.Password, HidePassword, ShowPassword);
            }

            if (LogInHidePassword != null && LogInShowPassword != null)
            {
                SwitchInputType(TMP_InputField.ContentType.Password, LogInHidePassword, LogInShowPassword);
            }
        }


        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            if (UserNameInputField != null)
            {
                UserNameInputField.onValueChanged.AddListener(_ => UserNameChanged(UserNameInputField));
            }

            if (SubmitUserNameInputField != null)
            {
                SubmitUserNameInputField.onValueChanged.AddListener(_ => UserNameChanged(SubmitUserNameInputField));
            }

            if (PasswordInputField != null)
            {
                PasswordInputField.onValueChanged.AddListener(_ => PasswordChanged(PasswordInputField));
            }

            if (EmailInputField != null)
            {
                EmailInputField.onValueChanged.AddListener(_ => EmailChanged(EmailInputField));
            }

            if (SubmitEmailInputField != null)
            {
                SubmitEmailInputField.onValueChanged.AddListener(_ => EmailChanged(SubmitEmailInputField));
            }



            if (PasswordInputVerifyField != null)
            {
                PasswordInputVerifyField.onValueChanged.AddListener(_ => PasswordVerifyChanged(PasswordInputVerifyField));
            }

            if (LogInUserNameInputField != null)
            {
                LogInUserNameInputField.onValueChanged.AddListener(_ => UserNameChanged(LogInUserNameInputField));
            }

            if (LogInPasswordInputField != null)
            {
                LogInPasswordInputField.onValueChanged.AddListener(_ => PasswordChanged(LogInPasswordInputField));
            }


            if (LogIn != null)
            {
                LogIn.onClick.AddListener(OnLogIn);
            }

            if (Create != null)
            {
                Create.onClick.AddListener(OnCreateAccount);
            }

            if (CreateNewTab != null)
            {
                CreateNewTab.onClick.AddListener(() => OnTabSwitch(CreateNewTab));
            }

            if (LogInTab != null)
            {
                LogInTab.onClick.AddListener(() => OnTabSwitch(LogInTab));
            }

            if (SignInWithUnity != null)
            {
                SignInWithUnity.onClick.AddListener(OnSignInWithUnity);
            }

            if (SignInWithGoogle != null)
            {
                SignInWithGoogle.onClick.AddListener(OnSignInWithGoogle);
            }

            if (SignInWithFacebook != null)
            {
                SignInWithFacebook.onClick.AddListener(OnSignInWithFacebook);
            }

            if (SignInWithAsGuest != null)
            {
                SignInWithAsGuest.onClick.AddListener(OnSignInWithAsGuest);
            }

            if (SignInWithSteam != null)
            {
                SignInWithSteam.onClick.AddListener(OnSignInWithSteam);
            }


            if (HidePassword != null && ShowPassword != null)
            {
                HidePassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Password, HidePassword, ShowPassword));
                ShowPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Standard, ShowPassword, HidePassword));

            }
            if (LogInHidePassword != null && LogInShowPassword != null)
            {
                LogInHidePassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Password, LogInHidePassword, LogInShowPassword));
                LogInShowPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Standard, LogInShowPassword, LogInHidePassword));
            }

            if (HideVerifyPassword != null && ShowVerifyPassword != null)
            {
                HideVerifyPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Password, HideVerifyPassword, ShowVerifyPassword));
                ShowVerifyPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Standard, ShowVerifyPassword, HideVerifyPassword));

            }




            EventRegistrar.Subscribe<RequestUserCredentialsEvent>(HandleRequestUserCredentials);

            EventRegistrar.Subscribe<RequestAdditionalUserInfoEvent>(HandleRequestAdditionalUserInfo);

            EventRegistrar.Subscribe<AuthenticationStatusEvent>(OnAuthenticationStatus);

            EventRegistrar.Subscribe<UpdateUIInteractabilityEvent>(OnUpdateUIInteractability);

            EventRegistrar.Subscribe<ProfileCreatedEvent>(OnProfileCreated);
        }



        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();

            if (UserNameInputField != null)
            {
                UserNameInputField.onValueChanged.RemoveListener(_ => UserNameChanged(UserNameInputField));
            }

            if (SubmitUserNameInputField != null)
            {
                SubmitUserNameInputField.onValueChanged.RemoveListener(_ => UserNameChanged(SubmitUserNameInputField));
            }

            if (EmailInputField != null)
            {
                EmailInputField.onValueChanged.RemoveListener(_ => EmailChanged(EmailInputField));
            }

            if (SubmitEmailInputField != null)
            {
                SubmitEmailInputField.onValueChanged.RemoveListener(_ => EmailChanged(SubmitEmailInputField));
            }

            if (PasswordInputField != null)
            {
                PasswordInputField.onValueChanged.RemoveListener(_ => PasswordChanged(PasswordInputField));
            }

            if (PasswordInputVerifyField != null)
            {
                PasswordInputVerifyField.onValueChanged.RemoveListener(_ => PasswordVerifyChanged(PasswordInputVerifyField));
            }

            if (LogInUserNameInputField != null)
            {
                LogInUserNameInputField.onValueChanged.RemoveListener(_ => UserNameChanged(LogInUserNameInputField));
            }

            if (LogInPasswordInputField != null)
            {
                LogInPasswordInputField.onValueChanged.RemoveListener(_ => PasswordChanged(LogInPasswordInputField));
            }

            if (LogIn != null)
            {
                LogIn.onClick.RemoveListener(OnLogIn);
            }

            if (Create != null)
            {
                Create.onClick.RemoveListener(OnCreateAccount);
            }
            if (CreateNewTab != null)
            {
                CreateNewTab.onClick.RemoveListener(() => OnTabSwitch(CreateNewTab));
            }

            if (LogInTab != null)
            {
                LogInTab.onClick.RemoveListener(() => OnTabSwitch(LogInTab));
            }
            if (SignInWithUnity != null)
            {
                SignInWithUnity.onClick.RemoveListener(OnSignInWithUnity);
            }

            if (SignInWithGoogle != null)
            {
                SignInWithGoogle.onClick.RemoveListener(OnSignInWithGoogle);
            }

            if (SignInWithFacebook != null)
            {
                SignInWithFacebook.onClick.RemoveListener(OnSignInWithFacebook);
            }

            if (SignInWithAsGuest != null)
            {
                SignInWithAsGuest.onClick.RemoveListener(OnSignInWithAsGuest);
            }

            if (SignInWithSteam != null)
            {
                SignInWithSteam.onClick.RemoveListener(OnSignInWithSteam);
            }

            if (HidePassword != null && ShowPassword != null)
            {
                HidePassword.onClick.RemoveListener(() => SwitchInputType(TMP_InputField.ContentType.Password, HidePassword, ShowPassword));
                ShowPassword.onClick.RemoveListener(() => SwitchInputType(TMP_InputField.ContentType.Standard, ShowPassword, HidePassword));
            }


            if (HideVerifyPassword != null && ShowVerifyPassword != null)
            {
                HideVerifyPassword.onClick.RemoveListener(() => SwitchInputType(TMP_InputField.ContentType.Password, HideVerifyPassword, ShowVerifyPassword));
                ShowVerifyPassword.onClick.RemoveListener(() => SwitchInputType(TMP_InputField.ContentType.Standard, ShowVerifyPassword, HideVerifyPassword));
            }

            if (LogInHidePassword != null && LogInShowPassword != null)
            {
                LogInHidePassword.onClick.RemoveListener(() => SwitchInputType(TMP_InputField.ContentType.Password, LogInHidePassword, LogInShowPassword));
                LogInShowPassword.onClick.RemoveListener(() => SwitchInputType(TMP_InputField.ContentType.Standard, LogInShowPassword, LogInHidePassword));
            }


            EventRegistrar.UnsubscribeAll();
        }




        [Button]
        private void OnTabSwitch(Button3D button)
        {

            IsLogInTabActive = string.Equals(button.name, nameof(LogInTab), StringComparison.OrdinalIgnoreCase);

            if (IsLogInTabActive)
            {
                AuthenticationUIFrameCreate.gameObject.SetActive(false);
                AuthenticationUIFrameLogin.gameObject.SetActive(true);
                EmailHolder.gameObject.SetActive(false);
                PasswordVerifyHolder.gameObject.SetActive(false);

                if (CreateNewTab != null)
                {
                    CreateNewTab.SetInteractable(true);
                }

                if (LogInTab != null)
                {
                    LogInTab.SetInteractable(false);
                }

            }
            else
            {
                AuthenticationUIFrameCreate.gameObject.SetActive(true);
                AuthenticationUIFrameLogin.gameObject.SetActive(false);
                EmailHolder.gameObject.SetActive(true);
                PasswordVerifyHolder.gameObject.SetActive(true);

                if (CreateNewTab != null)
                {
                    CreateNewTab.SetInteractable(false);
                }

                if (LogInTab != null)
                {
                    LogInTab.SetInteractable(true);
                }
            }

            IsCreateFrameActive = AuthenticationUIFrameCreate.gameObject.activeSelf;
            IsLoginFrameActive = AuthenticationUIFrameLogin.gameObject.activeSelf;

        }

        private void OnUpdateUIInteractability(UpdateUIInteractabilityEvent e)
        {
            Interactable = e.IsInteractable;
            UpdateButtonInteractability(e.IsInteractable);
        }

        private void OnAuthenticationStatus(AuthenticationStatusEvent e)
        {
            switch (e.Result.ResultAuthStatus)
            {
                case AuthStatus.Success:
                    GameLoggerScriptable.Log($"Authentication Status {e.Result.Message}", this);
                    break;
                case AuthStatus.Authenticated:
                    OnAuthenticationCompleted(e.Result.AuthPlayerData);
                    break;
                case AuthStatus.Failure:
                    ShowErrorMessage(e.Result.Message);
                    break;
                case AuthStatus.Pending:
                    GameLoggerScriptable.Log($"Authentication in progress: {e.Result.Message}", this);
                    // todo show a loading indicator or update UI to show pending status
                    break;
            }
        }

        private async void HandleRequestAdditionalUserInfo(RequestAdditionalUserInfoEvent e)
        {
            string userName = UserNameInputField.text;
            string email = EmailInputField.text;

            if (IsNullOrEmpty(userName) || IsNullOrEmpty(email))
            {
                (userName, email) = await ShowAdditionalUI(e.IsGuest);

                if (IsNullOrEmpty(userName) || IsNullOrEmpty(email))
                {
                    e.CompletionSource.TrySetException(new InvalidOperationException("Username or email is empty"));
                    return;
                }
            }

            e.CompletionSource.TrySetResult((userName, email));
            UpdateButtonInteractability(true);

        }

        private void HandleRequestUserCredentials(RequestUserCredentialsEvent e)
        {
            bool isValid = ValidateUserAndPassword(UserNameInputField, PasswordInputField);
            UserCredentials credentials = new UserCredentials(
                UserNameInputField.text,
                PasswordInputField.text,
                isValid
            );
            e.CompletionSource.TrySetResult(credentials);
        }


        private async void OnSignInWithUnity()
        {
            SignInWithUnityEvent signInWithUnityEvent = new SignInWithUnityEvent();
            bool published = await EventBus.Instance.PublishAsync(signInWithUnityEvent);
            if (published)
            {
                AuthResult result = await signInWithUnityEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                HandleAuthResult(AuthResult.Failure("Failed to publish sign-in with Unity event"));
            }
        }

        private async void OnSignInWithGoogle()
        {
            SignInWithGoogleEvent signInWithGoogleEvent = new SignInWithGoogleEvent();
            bool published = await EventBus.Instance.PublishAsync(signInWithGoogleEvent);
            if (published)
            {
                AuthResult result = await signInWithGoogleEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                HandleAuthResult(AuthResult.Failure("Failed to publish sign-in with Google event"));
            }
        }

        private async void OnSignInWithFacebook()
        {
            SignInWithFacebookEvent signInWithFacebookEvent = new SignInWithFacebookEvent();
            bool published = await EventBus.Instance.PublishAsync(signInWithFacebookEvent);
            if (published)
            {
                AuthResult result = await signInWithFacebookEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                HandleAuthResult(AuthResult.Failure("Failed to publish sign-in with Facebook event"));
            }
        }

        private async void OnSignInWithAsGuest()
        {
            SignInAsGuestEvent signInAsGuestEvent = new SignInAsGuestEvent();
            bool published = await EventBus.Instance.PublishAsync(signInAsGuestEvent);
            if (published)
            {
                AuthResult result = await signInAsGuestEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                HandleAuthResult(AuthResult.Failure("Failed to publish sign-in with AsGuest event"));
            }
        }
        private async void OnSignInWithSteam()
        {
            SignInWithSteamEvent signInWithSteamEvent = new SignInWithSteamEvent();
            bool published = await EventBus.Instance.PublishAsync(signInWithSteamEvent);
            if (published)
            {
                AuthResult result = await signInWithSteamEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                HandleAuthResult(AuthResult.Failure("Failed to publish sign-in with Steam event"));
            }
        }
        private async void OnCreateAccount()
        {
            string username = UserNameInputField.text;
            string password = PasswordInputField.text;
            if (IsNullOrEmpty(username) || IsNullOrEmpty(password))
            {
                return;
            }

            CreateAccountEvent createAccountEvent = new CreateAccountEvent(username, password);
            bool published = await EventBus.Instance.PublishAsync(createAccountEvent);
            if (published)
            {
                AuthResult result = await createAccountEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                ShowErrorMessage("Failed To Create Lobby Account");
            }
        }

        private async void OnLogIn()
        {
            SignInWithUserPasswordEvent signInEvent = new SignInWithUserPasswordEvent(LogInUserNameInputField.text, LogInPasswordInputField.text);
            bool published = await EventBus.Instance.PublishAsync(signInEvent);
            if (published)
            {
                AuthResult result = await signInEvent.CompletionSource.Task;
                HandleAuthResult(result);
            }
            else
            {
                ShowErrorMessage("Failed To LogIn");
            }
        }

        private void HandleAuthResult(AuthResult result)
        {
            if (result.IsSuccess)
            {
                GameLoggerScriptable.Log(result.Message, null);
            }
            else
            {
                ShowErrorMessage(result.Message);
            }
        }
        private void SwitchInputType(TMP_InputField.ContentType inputType, Button buttonToHide, Button buttonToShow)
        {
            buttonToHide.gameObject.SetActive(false);
            buttonToShow.gameObject.SetActive(true);
            LogInPasswordInputField.contentType = inputType;
            PasswordInputField.contentType = inputType;
            PasswordInputVerifyField.contentType = inputType;
            PasswordInputField.ForceLabelUpdate();
            PasswordInputVerifyField.ForceLabelUpdate();
            LogInPasswordInputField.ForceLabelUpdate();
        }

        private void UserNameChanged(TMP_InputField tmpInputField)
        {
            string username = tmpInputField.text;
            tmpInputField.textComponent.color = IsNullOrEmpty(username) || !NameRegex.IsMatch(username)
                ? Color.red
                : Color.black;

            List<string> errors = new List<string>();

            if (IsNullOrEmpty(username))
            {
                FieldValidator.text = "Username cannot be empty.";
                return; // If empty, no need to check further criteria.
            }

            if (username.Length < 3)
            {
                errors.Add("be at least 3 characters long");
            }

            if (username.Length > 20)
            {
                errors.Add("not exceed 20 characters");
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9.\-@_]+$"))
            {
                errors.Add("only contain letters, numbers, dots, hyphens, at symbols, and underscores");
            }

            if (errors.Count == 0)
            {
                FieldValidator.text = "";
            }
            else
            {
                FieldValidator.text = "Username must " + Join(", ", errors) + ".";
            }

            LogIn.SetInteractable(ValidateUserAndPassword(LogInUserNameInputField, LogInPasswordInputField));
            Create.SetInteractable(ValidateUserAndPassword(UserNameInputField, PasswordInputField));
        }

        private void EmailChanged(TMP_InputField inputField)
        {
            string email = inputField.text;
            inputField.textComponent.color =
                IsNullOrEmpty(email) || !EmailRegex.IsMatch(email) ? Color.red : Color.black;

            List<string> errors = new List<string>();

            if (IsNullOrEmpty(email))
            {
                FieldValidator.text = "Email cannot be empty.";
                return;
            }

            if (email.Length < 3)
            {
                errors.Add("be at least 3 characters long");
            }

            if (email.Length > 254)
            {
                errors.Add("not exceed 254 characters");
            }

            if (!email.Contains("@"))
            {
                errors.Add("contain @");
            }

            if (!email.Contains("."))
            {
                errors.Add("contain dot (.)");
            }

            if (email.IndexOf('@') != email.LastIndexOf('@'))
            {
                errors.Add("contain only one @ symbol");
            }

            if (email.Contains(".."))
            {
                errors.Add("not have consecutive dots (.)");
            }

            if (email.StartsWith("."))
            {
                errors.Add("not start with a dot (.)");
            }

            if (email.EndsWith("."))
            {
                errors.Add("not end with a dot (.)");
            }

            if (email.IndexOf('@') > email.LastIndexOf('.'))
            {
                errors.Add("have the @ symbol before the dot (.) in the domain");
            }

            if (errors.Count == 0)
            {
                FieldValidator.text = "";
            }
            else
            {
                FieldValidator.text = "email must " + Join(", ", errors) + ".";
            }
        }

        private void PasswordChanged(TMP_InputField tmpInputField)
        {
            string password = tmpInputField.text;
            tmpInputField.textComponent.color = IsNullOrEmpty(password) || !PasswordRegex.IsMatch(password)
                ? Color.red
                : Color.black;

            List<string> errors = new List<string>();

            if (IsNullOrEmpty(password))
            {
                FieldValidator.text = "Password cannot be empty.";
                return; // If empty, no need to check further criteria.
            }

            if (password.Length < 8)
            {
                errors.Add("be at least 8 characters long");
            }

            if (password.Length > 30)
            {
                errors.Add("not exceed 30 characters");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errors.Add("one lowercase letter");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errors.Add("one uppercase letter");
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                errors.Add("one digit");
            }

            if (!Regex.IsMatch(password, @"[^\da-zA-Z]"))
            {
                errors.Add("one symbol");
            }

            if (errors.Count == 0)
            {
                FieldValidator.text = "";
            }
            else
            {
                FieldValidator.text = "Password must contain at least " + Join(',', errors) + ".";
            }

            LogIn.SetInteractable(ValidateUserAndPassword(LogInUserNameInputField, LogInPasswordInputField));
        }

        private void PasswordVerifyChanged(TMP_InputField passwordInputVerifyField)
        {
            if (PasswordInputField.text == passwordInputVerifyField.text)
            {
                FieldValidator.text = "";
                FieldValidator.color = Color.black;
            }
            else
            {
                FieldValidator.text = "Password Don't Match!";
                FieldValidator.color = Color.red;
            }

            passwordInputVerifyField.textComponent.color =
                PasswordInputField.text == passwordInputVerifyField.text ? Color.black : Color.red;
            LogIn.SetInteractable(ValidateUserAndPassword(LogInUserNameInputField, LogInPasswordInputField));
            Create.SetInteractable(ValidateUserPasswordAndVerifyPassword(UserNameInputField, PasswordInputField, PasswordInputVerifyField));
        }

        public bool ValidateUserAndPassword(TMP_InputField userNameInputField, TMP_InputField tmpInputField)
        {
            return !IsNullOrEmpty(userNameInputField.text)
                   && NameRegex.IsMatch(userNameInputField.text)
                   && !IsNullOrEmpty(tmpInputField.text)
                   && PasswordRegex.IsMatch(tmpInputField.text);
        }

        private bool ValidateUserPasswordAndVerifyPassword(TMP_InputField userNameInputField, TMP_InputField tmpInputField, TMP_InputField passwordInputVerifyField)
        {
            return !IsNullOrEmpty(userNameInputField.text)
                   && NameRegex.IsMatch(userNameInputField.text)
                   && !IsNullOrEmpty(tmpInputField.text)
                   && PasswordRegex.IsMatch(tmpInputField.text)
                   && !IsNullOrEmpty(passwordInputVerifyField.text)
                   && PasswordRegex.IsMatch(passwordInputVerifyField.text);
        }


        public async UniTask<(string userName, string email)> ShowAdditionalUI(bool isGuest)
        {


            UserAndEmailRegistrationUI.gameObject.SetActive(!isGuest);
            GuestLogInUI.gameObject.SetActive(isGuest);

            AuthenticationUIFrameCreate.gameObject.SetActive(false);
            AuthenticationUIFrameLogin.gameObject.SetActive(false);
            AuthenticationUITabs.gameObject.SetActive(false);

            UniTaskCompletionSource<(string userName, string email)> completionSource = new UniTaskCompletionSource<(string userName, string email)>();

            void OnClick()
            {
                if (!isGuest)
                {
                    string userName = SubmitUserNameInputField.text.Trim();
                    string email = SubmitEmailInputField.text.Trim();

                    if (!IsNullOrEmpty(userName) && !IsNullOrEmpty(email))
                    {
                        SubmitNameAndEmail.onClick.RemoveListener(OnClick);
                        UserAndEmailRegistrationUI.gameObject.SetActive(false);
                        AuthenticationUIFrameCreate.gameObject.SetActive(IsCreateFrameActive);
                        AuthenticationUIFrameLogin.gameObject.SetActive(IsLoginFrameActive);
                        AuthenticationUITabs.gameObject.SetActive(true);
                        completionSource.TrySetResult((userName, email));
                    }
                }
                else
                {
                    string nickName = NickNameInputField.text.Trim();

                    if (!IsNullOrEmpty(nickName))
                    {
                        SubmitNickName.onClick.RemoveListener(OnClick);
                        GuestLogInUI.gameObject.SetActive(false);
                        AuthenticationUIFrameCreate.gameObject.SetActive(IsCreateFrameActive);
                        AuthenticationUIFrameLogin.gameObject.SetActive(IsLoginFrameActive);
                        AuthenticationUITabs.gameObject.SetActive(true);
                        completionSource.TrySetResult((nickName, "guest@guest.com"));
                    }
                }
            }

            if (!isGuest)
            {
                SubmitNameAndEmail.onClick.AddListener(OnClick);
            }
            else
            {
                SubmitNickName.onClick.AddListener(OnClick);
            }

            (string userName, string email) result = await completionSource.Task;

            return result;
        }


        public void ShowErrorMessage(string message)
        {
            EventBus.Instance.Publish(new AuthenticationErrorUIEvent(message));
        }

        public void UpdateButtonInteractability(bool interactable)
        {
            LogIn.SetInteractable(interactable);
            Create.SetInteractable(interactable);
            SignInWithUnity.SetInteractable(interactable);
            SignInWithGoogle.SetInteractable(interactable);
            SignInWithAsGuest.SetInteractable(interactable);
            SignInWithFacebook.SetInteractable(interactable);
        }

        private void OnProfileCreated(ProfileCreatedEvent obj)
        {
            HideScreen();
        }

        public void OnAuthenticationCompleted(IAuthPlayerData authPlayer)
        {

            EventBus.Instance.Publish(new CreateProfileEvent(authPlayer));

        }


    }
}