using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
using ThreeCardBrag.Authentication;
using System.Collections.Generic;
using ThreeCardBrag.Extensions;
using Unity.Services.Authentication;
using System.Threading.Tasks;

namespace ThreeCardBrag.GameScreen
{
    public class LoginScreen : BragScreen<LoginScreen>
    {
        [ShowInInspector, Required] public GameObject AuthenticationUI { get; private set; }
        [ShowInInspector, Required] public GameObject UserAndEmailRegistrationUI { get; private set; }

        [ShowInInspector, Required] public Button SignInWithName { get; private set; }
        [ShowInInspector, Required] public Button SignInWithUnity { get; private set; }
        [ShowInInspector, Required] public Button Create { get; private set; }
        [ShowInInspector, Required] public Button OkButton { get; private set; }
        [ShowInInspector, Required] public Button SubmitNameAndEmail { get; private set; }
        [ShowInInspector, Required] public Button ShowPassword { get; private set; }
        [ShowInInspector, Required] public Button HidePassword { get; private set; }
        [ShowInInspector, Required] public Button ShowVerifyPassword { get; private set; }
        [ShowInInspector, Required] public Button HideVerifyPassword { get; private set; }

        [ShowInInspector, Required] public TMP_InputField UserNameInputField { get; private set; }
        [ShowInInspector, Required] public TMP_InputField PasswordInputField { get; private set; }
        [ShowInInspector, Required] public TMP_InputField PasswordInputVerifyField { get; private set; }
        [ShowInInspector, Required] public TMP_InputField EmailInputField { get; private set; }
        [ShowInInspector, Required] public TMP_InputField SubmitEmailInputField { get; private set; }
        [ShowInInspector, Required] public TMP_InputField SubmitUserNameInputField { get; private set; }

        [ShowInInspector, Required] public TextMeshProUGUI ErrorMessage { get; private set; }
        [ShowInInspector, Required] public TextMeshProUGUI FieldValidator { get; private set; }
        [ShowInInspector, Required] public GameObject ErrorPanel { get; private set; }

        [ReadOnly]
        public static Regex NameRegex => new Regex(@"^[a-zA-Z0-9.\-@_]{3,20}$");

        [ReadOnly]
        public static Regex EmailRegex => new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        [ReadOnly]
        public static Regex PasswordRegex => new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,30}$");

        protected override void Awake()
        {
            base.Awake();
            InitReferences();
        }

        void OnValidate()
        {
            InitReferences();
        }

        private void InitReferences()
        {
            AuthenticationUI = transform.RecursiveFindChildGameObject(nameof(AuthenticationUI));
            UserAndEmailRegistrationUI = transform.RecursiveFindChildGameObject(nameof(UserAndEmailRegistrationUI));

            SignInWithName = transform.FindChildRecursively<Button>(nameof(SignInWithName));
            SignInWithUnity = transform.FindChildRecursively<Button>(nameof(SignInWithUnity));
            Create = transform.FindChildRecursively<Button>(nameof(Create));
            SubmitNameAndEmail = transform.FindChildRecursively<Button>(nameof(SubmitNameAndEmail));
            ShowPassword = transform.FindChildRecursively<Button>(nameof(ShowPassword));
            HidePassword = transform.FindChildRecursively<Button>(nameof(HidePassword));
            ShowVerifyPassword = transform.FindChildRecursively<Button>(nameof(ShowVerifyPassword));
            HideVerifyPassword = transform.FindChildRecursively<Button>(nameof(HideVerifyPassword));

            UserNameInputField = transform.FindChildRecursively<TMP_InputField>(nameof(UserNameInputField));
            PasswordInputField = transform.FindChildRecursively<TMP_InputField>(nameof(PasswordInputField));
            PasswordInputVerifyField = transform.FindChildRecursively<TMP_InputField>(nameof(PasswordInputVerifyField));
            EmailInputField = transform.FindChildRecursively<TMP_InputField>(nameof(EmailInputField));
            SubmitEmailInputField = transform.FindChildRecursively<TMP_InputField>(nameof(SubmitEmailInputField));
            SubmitUserNameInputField = transform.FindChildRecursively<TMP_InputField>(nameof(SubmitUserNameInputField));

            ErrorMessage = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ErrorMessage));
            FieldValidator = transform.FindChildRecursively<TextMeshProUGUI>(nameof(FieldValidator));
            ErrorPanel = transform.RecursiveFindChildGameObject(nameof(ErrorPanel));

            OkButton = transform.FindChildRecursively<Button>(nameof(OkButton));
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeLoginScreen();
        }

        private void InitializeLoginScreen()
        {
            if (UserNameInputField != null)
            {
                UserNameInputField.onValueChanged.AddListener(arg0 => UserNameChanged(arg0, UserNameInputField));
            }

            if (SubmitUserNameInputField != null)
            {
                SubmitUserNameInputField.onValueChanged.AddListener(arg0 => UserNameChanged(arg0, SubmitUserNameInputField));
            }

            if (EmailInputField != null)
            {
                EmailInputField.onValueChanged.AddListener(arg0 => EmailChanged(arg0, EmailInputField));
            }

            if (SubmitEmailInputField != null)
            {
                SubmitEmailInputField.onValueChanged.AddListener(arg0 => EmailChanged(arg0, SubmitEmailInputField));
            }

            if (PasswordInputField != null)
            {
                PasswordInputField.onValueChanged.AddListener(unknown => PasswordChanged(PasswordInputField));
            }

            if (PasswordInputVerifyField != null)
            {
                PasswordInputVerifyField.onValueChanged.AddListener(typedPassword => PasswordVerifyChanged(PasswordInputVerifyField));
            }

            SignInWithName.onClick.AddListener(async () => await AuthenticationManager.Instance.PerformAuthenticationAsync(AuthenticationService.Instance.SignInWithUsernamePasswordAsync, UserNameInputField.text, PasswordInputField.text, ShowErrorMessage));
            Create.onClick.AddListener(async () => await AuthenticationManager.Instance.PerformAuthenticationAsync(AuthenticationService.Instance.SignUpWithUsernamePasswordAsync, UserNameInputField.text, PasswordInputField.text, ShowErrorMessage));
            SignInWithUnity.onClick.AddListener(async () => await AuthenticationManager.Instance.StartSignInAsync(ShowErrorMessage));
            OkButton.onClick.AddListener(() => { ErrorMessage.text = string.Empty; ErrorPanel.SetActive(false); });

            HidePassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Password));
            ShowPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Standard));
            HideVerifyPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Password));
            ShowVerifyPassword.onClick.AddListener(() => SwitchInputType(TMP_InputField.ContentType.Standard));

            ErrorPanel.SetActive(false);
        }



        private void SwitchInputType(TMP_InputField.ContentType inputType)
        {
            PasswordInputField.contentType = inputType;
            PasswordInputVerifyField.contentType = inputType;
            PasswordInputField.ForceLabelUpdate();
            PasswordInputVerifyField.ForceLabelUpdate();
        }

        private void UserNameChanged(string _, TMP_InputField userNameInputField)
        {
            string username = userNameInputField.text;
            userNameInputField.textComponent.color = string.IsNullOrEmpty(username) || !NameRegex.IsMatch(username) ? Color.red : Color.black;

            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(username))
            {
                FieldValidator.text = "Username cannot be empty.";
                return; // If empty, no need to check further criteria.
            }

            if (username.Length < 3)
                errors.Add("be at least 3 characters long");

            if (username.Length > 20)
                errors.Add("not exceed 20 characters");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9.\-@_]+$"))
                errors.Add("only contain letters, numbers, dots, hyphens, at symbols, and underscores");

            if (errors.Count == 0)
            {
                FieldValidator.text = "";
            }
            else
            {
                FieldValidator.text = "Username must " + string.Join(", ", errors) + ".";
            }

            SignInWithName.enabled = ValidateUserAndPassword();
            Create.enabled = ValidateUserAndPassword();
        }

        private void EmailChanged(string _, TMP_InputField inputField)
        {
            string email = inputField.text;
            inputField.textComponent.color = string.IsNullOrEmpty(email) || !EmailRegex.IsMatch(email) ? Color.red : Color.black;

            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(email))
            {
                FieldValidator.text = "Email cannot be empty.";
                return;
            }

            if (email.Length < 3)
                errors.Add("be at least 3 characters long");

            if (email.Length > 254)
                errors.Add("not exceed 254 characters");

            if (!email.Contains("@"))
                errors.Add("contain @");

            if (!email.Contains("."))
                errors.Add("contain dot (.)");

            if (email.IndexOf('@') != email.LastIndexOf('@'))
                errors.Add("contain only one @ symbol");

            if (email.Contains(".."))
                errors.Add("not have consecutive dots (.)");

            if (email.StartsWith("."))
                errors.Add("not start with a dot (.)");

            if (email.EndsWith("."))
                errors.Add("not end with a dot (.)");

            if (email.IndexOf('@') > email.LastIndexOf('.'))
                errors.Add("have the @ symbol before the dot (.) in the domain");

            if (errors.Count == 0)
            {
                FieldValidator.text = "";
            }
            else
            {
                FieldValidator.text = "email must " + string.Join(", ", errors) + ".";
            }
        }

        private void PasswordChanged(TMP_InputField tmpInputField)
        {
            string password = tmpInputField.text;
            tmpInputField.textComponent.color = string.IsNullOrEmpty(password) || !PasswordRegex.IsMatch(password) ? Color.red : Color.black;

            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                FieldValidator.text = "Password cannot be empty.";
                return; // If empty, no need to check further criteria.
            }

            if (password.Length < 8)
                errors.Add("be at least 8 characters long");

            if (password.Length > 30)
                errors.Add("not exceed 30 characters");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("one lowercase letter");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("one uppercase letter");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("one digit");

            if (!Regex.IsMatch(password, @"[^\da-zA-Z]"))
                errors.Add("one symbol");

            if (errors.Count == 0)
            {
                FieldValidator.text = "";
            }
            else
            {
                FieldValidator.text = "Password must contain at least " + string.Join(',', errors) + ".";
            }
            SignInWithName.enabled = ValidateUserAndPassword();
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
            passwordInputVerifyField.textComponent.color = PasswordInputField.text == passwordInputVerifyField.text ? Color.black : Color.red;
            SignInWithName.enabled = ValidateUserAndPassword();
            Create.enabled = ValidateUserPasswordAndVerifyPassword();
        }

        public bool ValidateUserAndPassword()
        {
            return !string.IsNullOrEmpty(UserNameInputField.text)
                   && NameRegex.IsMatch(UserNameInputField.text)
                   && !string.IsNullOrEmpty(PasswordInputField.text)
                   && PasswordRegex.IsMatch(PasswordInputField.text);
        }

        private bool ValidateUserPasswordAndVerifyPassword()
        {
            return !string.IsNullOrEmpty(UserNameInputField.text)
                   && NameRegex.IsMatch(UserNameInputField.text)
                   && !string.IsNullOrEmpty(PasswordInputField.text)
                   && PasswordRegex.IsMatch(PasswordInputField.text)
                   && !string.IsNullOrEmpty(PasswordInputVerifyField.text)
                   && PasswordRegex.IsMatch(PasswordInputVerifyField.text);
        }


        public Task<(string userName, string email)> ShowUserNameAndEmailUI()
        {
            UserAndEmailRegistrationUI.SetActive(true);
            AuthenticationUI.SetActive(false);

            var tcs = new TaskCompletionSource<(string userName, string email)>();

            void OnClick()
            {
                string userName = SubmitUserNameInputField.text;
                string email = SubmitEmailInputField.text;

                SubmitNameAndEmail.onClick.RemoveListener(OnClick);
                UserAndEmailRegistrationUI.SetActive(false);
                tcs.SetResult((userName, email));
            }

            SubmitNameAndEmail.onClick.AddListener(OnClick);

            return tcs.Task;
        }

        public void ShowErrorMessage(string message)
        {
            if (ErrorMessage != null)
            {
                ErrorMessage.text += $"\n + {message}";
            }

            if (ErrorPanel != null)
            {
                ErrorPanel.SetActive(true);
            }
        }
        public void UpdateButtonInteractability(bool interactable)
        {
            SignInWithName.interactable = interactable;
            Create.interactable = interactable;
            SignInWithUnity.interactable = interactable;
        }
        public void OnAuthenticationCompleted()
        {


            if (AuthenticationUI != null)
            {
                AuthenticationUI.SetActive(false);
            }

            AuthenticationManager.Instance.OnAuthenticationCompleted();
        }
    }
}
