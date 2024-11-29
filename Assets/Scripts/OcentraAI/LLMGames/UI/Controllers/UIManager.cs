using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        #region UI Elements

        [ShowInInspector] protected IPlayerData Player { get; set; }
        bool IsPlayerTurn => Player is {IsPlayerTurn: {Value: true}};
        [Required][ShowInInspector] private Button3D ShowPlayerHand { get; set; }

        [Required][ShowInInspector] private Button3D PlayBlind { get; set; }
        [Required][ShowInInspector] private Button3D RaiseBet { get; set; }
        [Required][ShowInInspector] private Button3D Fold { get; set; }
        [Required][ShowInInspector] private Button3D Bet { get; set; }
        [Required][ShowInInspector] private Button3D DrawFromDeck { get; set; }
        [Required][ShowInInspector] private Button3D ShowCall { get; set; }

        [Required][ShowInInspector] private Button3D PurchaseCoins { get; set; }


        [Required][ShowInInspector] private Transform MessageHolder { get; set; }
        [Required][ShowInInspector] private TMP_InputField RaiseAmount { get; set; }
        [Required][ShowInInspector] private TextMeshProUGUI Message { get; set; }
        [Required][ShowInInspector] private TextMeshPro Pot { get; set; }
        [Required][ShowInInspector] private TextMeshPro CurrentBet { get; set; }
        [ShowInInspector][Required] public GameObject MainTable { get; set; }




        [Required][ShowInInspector] private CardView FloorCardView { get; set; }
        [Required][ShowInInspector] private CardView TrumpCardView { get; set; }
        [Required][ShowInInspector] private CardView MagicCard0 { get; set; }
        [Required][ShowInInspector] private CardView MagicCard1 { get; set; }
        [Required][ShowInInspector] private CardView MagicCard2 { get; set; }
        [Required][ShowInInspector] private CardView MagicCard3 { get; set; }

        [Required][ShowInInspector] public CardView[] LocalPlayerCardViews { get; set; }
        [Required][ShowInInspector] public LeftPanelController LeftPanelController { get; set; }


        [Required, ShowInInspector, DictionaryDrawerSettings]
        public Dictionary<string, Button3D> BettingButtons { get; set; } = new Dictionary<string, Button3D>();

        [Required, ShowInInspector, DictionaryDrawerSettings]
        public Dictionary<string, CardView> TopCardViews { get; set; } = new Dictionary<string, CardView>();

        #endregion

        #region Unity Lifecycle Methods

        private void OnValidate()
        {
            Init();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            Init();

        }

        #endregion

        #region Event Subscription

        public void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<UIMessageEvent>(OnMessage);
            EventBus.Instance.Subscribe<RegisterLocalPlayerEvent>(OnRegisterLocalPlayerEvent);
            EventBus.Instance.Subscribe<TimerStartEvent>(OnTimerStartEvent);
            EventBus.Instance.Subscribe<TimerStopEvent>(OnTimerStopEvent);
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<UIMessageEvent>(OnMessage);
            EventBus.Instance.Unsubscribe<RegisterLocalPlayerEvent>(OnRegisterLocalPlayerEvent);
            EventBus.Instance.Unsubscribe<TimerStartEvent>(OnTimerStartEvent);
            EventBus.Instance.Unsubscribe<TimerStopEvent>(OnTimerStopEvent);

        }

        private void OnTimerStartEvent(TimerStartEvent arg)
        {
            if (IsPlayerTurn)
            {
                foreach (Button3D button in BettingButtons.Values)
                {

                    button.SetInteractable(false);
                }

                foreach (CardView cardView in TopCardViews.Values)
                {
                    cardView.SetActive(false);
                }

                if (ShowPlayerHand != null)
                {
                    if (Player.HasSeenHand.Value)
                    {
                        ShowPlayerHand.SetInteractable(false, false);
                    }
                    else
                    {
                        ShowPlayerHand.SetInteractable(true);
                    }

                }
            }
        }

        private void OnTimerStopEvent(TimerStopEvent obj)
        {
            if (IsPlayerTurn)
            {
                foreach (Button3D button in BettingButtons.Values)
                {

                    button.SetInteractable(false);
                }

                foreach (CardView cardView in TopCardViews.Values)
                {
                    cardView.SetActive(false);
                }

                if (ShowPlayerHand != null)
                {
                    ShowPlayerHand.SetInteractable(false, false);
                }
            }
        }


        private void OnRegisterLocalPlayerEvent(RegisterLocalPlayerEvent arg)
        {
            Player = arg.LocalPlayer;
            SetupInitialUIState();
        }

        #endregion
        #region Initialization

        private void Init()
        {
            FindUIComponents();
          
        }




        private T FindAndAssign<T>(string objectName) where T : Component
        {
            GameObject foundObject = GameObject.Find(objectName);
            if (foundObject == null)
            {
                Debug.LogWarning($"GameObject with name '{objectName}' not found.");
                return null;
            }

            T component = foundObject.GetComponent<T>();
            if (component == null)
            {
                Debug.LogWarning($"Component '{typeof(T).Name}' not found on GameObject '{objectName}'.");
            }

            if (component is Button3D button3D)
            {
                BettingButtons.TryAdd(objectName, button3D);
            }

            if (component is CardView cardView)
            {
                TopCardViews.TryAdd(objectName, cardView);
            }

            return component;
        }


        private void FindUIComponents()
        {
            MessageHolder = transform.FindChildRecursively<Transform>(nameof(MessageHolder));

            ShowPlayerHand = GameObject.Find(nameof(ShowPlayerHand)).GetComponent<Button3D>();

            DrawFromDeck = FindAndAssign<Button3D>(nameof(DrawFromDeck));
            PlayBlind = FindAndAssign<Button3D>(nameof(PlayBlind));
            RaiseBet = FindAndAssign<Button3D>(nameof(RaiseBet));
            Fold = FindAndAssign<Button3D>(nameof(Fold));
            Bet = FindAndAssign<Button3D>(nameof(Bet));
            ShowCall = FindAndAssign<Button3D>(nameof(ShowCall));

            PurchaseCoins = GameObject.Find(nameof(PurchaseCoins)).GetComponent<Button3D>();

            if (ShowPlayerHand != null)
            {
                LocalPlayerCardViews = ShowPlayerHand.GetComponentsInChildren<CardView>(true);
            }

            FloorCardView = GameObject.Find(nameof(FloorCardView)).GetComponent<CardView>();

            TrumpCardView = FindAndAssign<CardView>(nameof(TrumpCardView));
            MagicCard0 = FindAndAssign<CardView>(nameof(MagicCard0));
            MagicCard1 = FindAndAssign<CardView>(nameof(MagicCard1));
            MagicCard2 = FindAndAssign<CardView>(nameof(MagicCard2));
            MagicCard3 = FindAndAssign<CardView>(nameof(MagicCard3));

            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));

            Pot = transform.FindChildRecursively<TextMeshPro>(nameof(Pot));
            CurrentBet = transform.FindChildRecursively<TextMeshPro>(nameof(CurrentBet));


            MainTable = GameObject.Find(nameof(MainTable));

            RaiseAmount = GameObject.Find(nameof(RaiseAmount)).GetComponent<TMP_InputField>();

            LeftPanelController = FindAnyObjectByType<LeftPanelController>();
        }


        #endregion

        public bool TryGetRaiseAmount(out int raiseAmount)
        {
            raiseAmount = 0;

            if (int.TryParse(RaiseAmount.text, out raiseAmount) && raiseAmount > 0)
            {
                return true;
            }
            else
            {
                ShowMessage("Please enter a valid raise amount.", 3f);
                raiseAmount = 0;
                return false;
            }
        }


        #region Event Handlers



        private void OnMessage(UIMessageEvent e)
        {
            ShowMessage(e.Message, e.Delay);
        }

        #endregion

        private void SetupInitialUIState()
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
            }

            if (ShowPlayerHand != null)
            {
                ShowPlayerHand.SetInteractable(false, false);
            }

            if (LocalPlayerCardViews is { Length: > 0 })
            {
                foreach (CardView cardView in LocalPlayerCardViews)
                {
                    cardView.SetCard(null);
                    cardView.UpdateCardView();
                }
            }

            foreach (Button3D button in BettingButtons.Values)
            {
               
                button.SetInteractable(IsPlayerTurn);
            }

            foreach (CardView cardView in TopCardViews.Values)
            {
                cardView.SetActive(IsPlayerTurn);
            }
        }

        private void ResetAllCardViews()
        {
            foreach (CardView cardView in LocalPlayerCardViews)
            {
                cardView.ResetCardView();
            }

            LeftPanelController.ResetView();
            ShowPlayerHand.SetInteractable(true);
        }

        private void ShowMessage(string message, float delay = 5f)
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(true);
                if (Message != null)
                {
                    Message.text = message;
                }

                // StartCoroutine(HideMessageAfterDelay(delay));
            }
        }

        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideMessage();
        }

        private void HideMessage()
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
                if (Message != null)
                {
                    Message.text = "";
                }
            }
        }
    }
}