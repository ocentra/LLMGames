using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.Screens
{
    public class PurchaseScreen : UIScreen<PurchaseScreen>
    {
        [ShowInInspector, Required]
        public Button AuthenticateButton { get; private set; }

        [ShowInInspector, Required]
        public Button PurchaseCoinsButton { get; private set; }

        [ShowInInspector, Required]
        public Button PurchaseCardSkinButton { get; private set; }

        [ShowInInspector, Required]
        public Button PurchaseUpgradesButton { get; private set; }

        [ShowInInspector, Required]
        public TextMeshProUGUI CurrentCoinsText { get; private set; }

        [ShowInInspector, Required]
        public Button BackButton { get; private set; }

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
            AuthenticateButton = transform.FindChildRecursively<Button>(nameof(AuthenticateButton));
            PurchaseCoinsButton = transform.FindChildRecursively<Button>(nameof(PurchaseCoinsButton));
            PurchaseCardSkinButton = transform.FindChildRecursively<Button>(nameof(PurchaseCardSkinButton));
            PurchaseUpgradesButton = transform.FindChildRecursively<Button>(nameof(PurchaseUpgradesButton));
            CurrentCoinsText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(CurrentCoinsText));
            BackButton = transform.FindChildRecursively<Button>(nameof(BackButton));
        }

        public override void OnShowScreen(bool first)
        {
            base.OnShowScreen(first);
            InitializePurchaseScreen();
        }

        private void InitializePurchaseScreen()
        {
            UpdateCurrentCoins();
            AuthenticateButton.onClick.AddListener(AuthenticateUser);
            PurchaseCoinsButton.onClick.AddListener(PurchaseCoins);
            PurchaseCardSkinButton.onClick.AddListener(PurchaseCardSkin);
            PurchaseUpgradesButton.onClick.AddListener(PurchaseUpgrades);
            BackButton.onClick.AddListener(GoBack);
        }

        private void UpdateCurrentCoins()
        {
           // CurrentCoinsText.text = $"Current Coins: {GameManager.Instance.HumanPlayer.Coins}";
        }

        private void AuthenticateUser()
        {
            PlaySelectionSound();
            // todo Implement Unity authentication service logic 
        }

        private void PurchaseCoins()
        {
            PlaySelectionSound();
            //todo Implement coin purchase logic 
        }

        private void PurchaseCardSkin()
        {
            PlaySelectionSound();
            // todo Implement card skin purchase logic
        }

        private void PurchaseUpgrades()
        {
            PlaySelectionSound();
            // todo Implement upgrades purchase logic 
        }


    }
}
