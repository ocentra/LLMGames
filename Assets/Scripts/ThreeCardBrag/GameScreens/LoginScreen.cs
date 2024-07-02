using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace ThreeCardBrag.GameScreen
{
    public class LoginScreen : BragScreen<LoginScreen>
    {
        [ShowInInspector, Required]
        public TMP_InputField UsernameInput { get; private set; }

        [ShowInInspector, Required]
        public TMP_InputField PasswordInput { get; private set; }

        [ShowInInspector, Required]
        public Button LoginButton { get; private set; }

        [ShowInInspector, Required]
        public Button BackButton { get; private set; }

        [ShowInInspector, Required]
        public TextMeshProUGUI ErrorText { get; private set; }

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
            UsernameInput = transform.FindChildRecursively<TMP_InputField>(nameof(UsernameInput));
            PasswordInput = transform.FindChildRecursively<TMP_InputField>(nameof(PasswordInput));
            LoginButton = transform.FindChildRecursively<Button>(nameof(LoginButton));
            BackButton = transform.FindChildRecursively<Button>(nameof(BackButton));
            ErrorText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ErrorText));
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializeLoginScreen();
        }

        private void InitializeLoginScreen()
        {
            LoginButton.onClick.AddListener(AttemptLogin);
            BackButton.onClick.AddListener(GoBack);
            ErrorText.gameObject.SetActive(false);
        }

        private void AttemptLogin()
        {
            // todo Implement login logic here
            // For now, we'll just simulate a successful login
            PlaySelectionSound();
          //  GameController.Instance.OnLoginSuccess();
            HideScreen();
        }


    }
}
