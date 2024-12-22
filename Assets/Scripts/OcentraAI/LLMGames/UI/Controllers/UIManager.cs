using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.UI;
using OcentraAI.LLMGames.UI.Controllers;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class UIManager : SerializedMonoBehaviour, IUIManager
    {
        #region UI Elements

        [ShowInInspector] protected IHumanPlayerData HumanPlayer { get; set; }
        [ShowInInspector] protected bool IsPlayerTurn => HumanPlayer is { IsPlayerTurn: { Value: true } };
        [ShowInInspector] protected PlayerDecision LastDecision => HumanPlayer is { LastDecision: not null } ? PlayerDecision.FromId(HumanPlayer.LastDecision.Value) : PlayerDecision.None;
        private Button3D ShowPlayerHand { get; set; }

        [Required, ShowInInspector] private TMP_InputField RaiseAmount { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI Message { get; set; }
        [Required, ShowInInspector] public GameObject MainTable { get; set; }

        [Required, ShowInInspector] private TextMeshPro Pot { get; set; }
        [Required, ShowInInspector] private TextMeshPro CurrentBet { get; set; }

        [Required, ShowInInspector] private TextMeshPro RoundNumber { get; set; }
        [Required, ShowInInspector] private TextMeshPro RoundNumberOf { get; set; }

        [Required, ShowInInspector] private CardView FloorCardView { get; set; }
        [Required, ShowInInspector] private Draggable FloorCardDraggable { get; set; }

        [Required, ShowInInspector] public CardView[] LocalPlayerCardViews { get; set; }
        [Required, ShowInInspector] public LeftPanelController LeftPanelController { get; set; }


        [HideInInspector] public Dictionary<PlayerDecision, PlayerDecisionButton> BettingButtons { get; set; } = new Dictionary<PlayerDecision, PlayerDecisionButton>();

        [HideInInspector] public Dictionary<PlayerDecision, TopCardView> TopCardViews { get; set; } = new Dictionary<PlayerDecision, TopCardView>();

        [HideInInspector] public Dictionary<PlayerDecision, PlayerDecisionButton> UIOrientedDecisions { get; set; } = new Dictionary<PlayerDecision, PlayerDecisionButton>();

        [ShowInInspector, Required] public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();


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

            EventRegistrar.Subscribe<RegisterLocalPlayerEvent>(OnRegisterLocalPlayerEvent);
            EventRegistrar.Subscribe<TimerStartEvent>(OnTimerStartEvent);
            EventRegistrar.Subscribe<TimerStopEvent>(OnTimerStopEvent);
            EventRegistrar.Subscribe<UpdateWildCardsEvent<Card>>(OnUpdateWildCards);
            EventRegistrar.Subscribe<UpdateFloorCardEvent<Card>>(OnUpdateFloorCard);
            EventRegistrar.Subscribe<UpdatePlayerHandDisplayEvent>(OnUpdatePlayerHandDisplay);
            EventRegistrar.Subscribe<UpdateScoreDataEvent<INetworkRoundRecord>>(OnUpdateScoreDataEvent);
            EventRegistrar.Subscribe<UpdateWildCardsHighlightEvent>(OnUpdateWildCardsHighlight);
            EventRegistrar.Subscribe<NewRoundStartedEvent>(OnNewRoundStartedEvent);


            foreach (PlayerDecisionButton playerDecisionButton in BettingButtons.Values)
            {
                playerDecisionButton.onClick.AddListener(UpdateMainBettingButtons);
            }

            foreach (PlayerDecisionButton uiDecisionButton in UIOrientedDecisions.Values)
            {
                uiDecisionButton.onClick.AddListener(UpdateUIOrientedButtons);
            }

            foreach (TopCardView topCardView in TopCardViews.Values)
            {
                topCardView.PlayerDecisionButton.onClick.AddListener(UpdateTopCardView);
            }
        }



        public void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();

            foreach (PlayerDecisionButton playerDecisionButton in BettingButtons.Values)
            {
                playerDecisionButton.onClick.RemoveListener(UpdateMainBettingButtons);
            }

            foreach (PlayerDecisionButton uiDecisionButton in UIOrientedDecisions.Values)
            {
                uiDecisionButton.onClick.RemoveListener(UpdateUIOrientedButtons);
            }

            foreach (TopCardView topCardView in TopCardViews.Values)
            {
                topCardView.PlayerDecisionButton.onClick.RemoveListener(UpdateTopCardView);
            }
        }

        private void OnNewRoundStartedEvent(NewRoundStartedEvent obj)
        {
            ResetAllCardViews();
        }


        private void OnUpdateWildCardsHighlight(UpdateWildCardsHighlightEvent updateWildCardsHighlight)
        {
            foreach ((PlayerDecision playerDecision, CardView cardView) in TopCardViews)
            {
                if (playerDecision.DecisionId == updateWildCardsHighlight.WildCardsInHandId)
                {
                    cardView.SetHighlight(true);
                    cardView.UpdateCardView();
                }
            }



        }

        private void OnTimerStartEvent(TimerStartEvent arg)
        {
            UpdateFloorCardDraggable();
            UpdateUIOrientedButtons();
            UpdateTopCardView();
            UpdateMainBettingButtons();
        }



        private void OnTimerStopEvent(TimerStopEvent obj)
        {
            UpdateFloorCardDraggable();
            UpdateUIOrientedButtons();
            UpdateTopCardView();
            UpdateMainBettingButtons();
        }

        private void UpdateMainBettingButtons()
        {
            foreach (PlayerDecisionButton playerDecisionButton in BettingButtons.Values)
            {

                if (playerDecisionButton.PlayerDecision == PlayerDecision.PlayBlind)
                {
                    if (LastDecision == PlayerDecision.PlayBlind)
                    {
                        playerDecisionButton.SetInteractable(false);
                    }
                    else
                    {
                        playerDecisionButton.gameObject.SetActive(!HumanPlayer.HasSeenHand.Value);
                        playerDecisionButton.SetInteractable(!HumanPlayer.HasSeenHand.Value && IsPlayerTurn);
                    }


                }
                else if (playerDecisionButton.PlayerDecision == PlayerDecision.SeeHand)
                {
                    if (LastDecision == PlayerDecision.SeeHand)
                    {
                        playerDecisionButton.SetInteractable(false);
                    }
                    else
                    {
                        if (HumanPlayer.HasSeenHand.Value)
                        {
                            playerDecisionButton.SetInteractable(false, false);
                        }
                        else
                        {
                            playerDecisionButton.SetInteractable(IsPlayerTurn);
                        }
                    }


                }
                else if (playerDecisionButton.PlayerDecision == PlayerDecision.DrawFromDeck)
                {
                    if (LastDecision == PlayerDecision.DrawFromDeck)
                    {
                        playerDecisionButton.SetInteractable(false);
                    }
                    else
                    {
                        playerDecisionButton.SetInteractable(HumanPlayer.HasSeenHand.Value && IsPlayerTurn);
                    }
                }
                else
                {
                    playerDecisionButton.gameObject.SetActive(HumanPlayer.HasSeenHand.Value);
                    playerDecisionButton.SetInteractable(HumanPlayer.HasSeenHand.Value && IsPlayerTurn);
                }
            }
        }

        private void UpdateTopCardView()
        {
            foreach (TopCardView cardView in TopCardViews.Values)
            {
                PlayerDecisionButton cardViewPlayerDecisionButton = cardView.PlayerDecisionButton;

                if (cardViewPlayerDecisionButton != null && cardViewPlayerDecisionButton.PlayerDecision != null)
                {
                    PlayerDecision playerDecision = cardViewPlayerDecisionButton.PlayerDecision;

                    if (LastDecision == playerDecision)
                    {
                        cardViewPlayerDecisionButton.SetInteractable(false);
                    }

                    cardView.SetInteractable(IsPlayerTurn);
                    cardViewPlayerDecisionButton.SetInteractable(IsPlayerTurn);
                }
            }
        }

        private void UpdateUIOrientedButtons()
        {
            foreach (PlayerDecisionButton button in UIOrientedDecisions.Values)
            {
                if (button.PlayerDecision == PlayerDecision.PurchaseCoins)
                {
                    button.gameObject.SetActive(HumanPlayer.Coins.Value <= 0);
                }
                else
                {
                    button.SetInteractable(IsPlayerTurn);
                }
            }
        }


        private void OnUpdateWildCards(UpdateWildCardsEvent<Card> e)
        {
            foreach ((PlayerDecision playerDecision, TopCardView topCardView) in TopCardViews)
            {
                if (e.WildCards.TryGetValue(playerDecision, out Card card))
                {
                    topCardView.SetCard(card);
                    topCardView.UpdateCardView();
                    topCardView.gameObject.SetActive(true);

                }
                else
                {
                    topCardView.SetCard(null);
                    topCardView.UpdateCardView();
                    topCardView.gameObject.SetActive(false);

                }
            }
        }


        private void OnUpdateFloorCard(UpdateFloorCardEvent<Card> e)
        {
            if (FloorCardView != null)
            {
                FloorCardView.SetCard(e.Card);
                FloorCardView.UpdateCardView();
                FloorCardView.SetInteractable(FloorCardView.Card != null);
                UpdateFloorCardDraggable();

            }
        }

        private void UpdateFloorCardDraggable()
        {
            if (FloorCardDraggable != null)
            {
                FloorCardDraggable.IsPlayerTurn = IsPlayerTurn;

            }
        }

        private void OnUpdatePlayerHandDisplay(UpdatePlayerHandDisplayEvent e)
        {
            if (e.Player == HumanPlayer)
            {
                for (int i = 0; i < LocalPlayerCardViews.Length; i++)
                {
                    string handCard = e.Player.GetCard(i);
                    Card card = CardUtility.GetCardFromSymbol(handCard);
                    LocalPlayerCardViews[i].SetCard(card);
                    if (!e.Player.HasSeenHand.Value)
                    {
                        LocalPlayerCardViews[i].ShowBackside();
                    }
                }

            }
        }

        private void OnUpdateScoreDataEvent(UpdateScoreDataEvent<INetworkRoundRecord> scoreDataEvent)
        {
            int pot = scoreDataEvent.Pot;
            int currentBet = scoreDataEvent.CurrentBet;
            int totalRounds = scoreDataEvent.TotalRounds;
            int currentRound = scoreDataEvent.CurrentRound;

            if (scoreDataEvent.RoundRecords is { Count: > 0 })
            {
                List<INetworkRoundRecord> roundRecords = scoreDataEvent.RoundRecords;

                foreach (INetworkRoundRecord roundRecord in roundRecords)
                {
                    int potAmount = roundRecord.PotAmount;

                    List<INetworkPlayerRecord> playerRecords = roundRecord.PlayerRecords;

                    if (roundRecord.PlayerRecords is { Count: > 0 })
                    {
                        foreach (INetworkPlayerRecord playerRecord in playerRecords)
                        {
                            int handValue = playerRecord.HandValue;
                            string formattedHand = playerRecord.FormattedHand;

                        }
                    }

                }
            }


            Pot.text = $"Pot: {pot}";
            CurrentBet.text = $"Bet: {currentBet}";
            RoundNumber.text = $"{currentRound}";
            RoundNumberOf.text = $"{totalRounds}";
        }




        private void OnRegisterLocalPlayerEvent(RegisterLocalPlayerEvent arg)
        {
            HumanPlayer = arg.LocalHumanPlayer;
            SetLocalPlayerHand();
            UpdateUIOrientedButtons();
        }

        #endregion
        #region Initialization

        private void Init()
        {
            FindUIComponents();

        }


        private void FindAndAssign<T>(PlayerDecision playerDecision, Dictionary<PlayerDecision, PlayerDecisionButton> bettingButtons) where T : PlayerDecisionButton
        {


            List<T> foundObjects = transform.FindAllChildrenOfType<T>();

            T found = null;

            foreach (T playerDecisionButton in foundObjects)
            {
                if (playerDecisionButton.PlayerDecision.DecisionId == playerDecision.DecisionId)
                {
                    found = playerDecisionButton;
                    break;
                }
            }

            if (found != null)
            {
                bettingButtons.TryAdd(playerDecision, found);
            }
            else
            {
                Debug.LogError($"PlayerDecisionButton with PlayerDecision '{playerDecision.Name}' not found.");

            }
        }

        private void FindAndAssignTopCardView<T>(PlayerDecision playerDecision, Dictionary<PlayerDecision, TopCardView> topCardViews) where T : TopCardView
        {


            List<T> foundObjects = transform.FindAllChildrenOfType<T>();
            T found = null;

            foreach (T topCardView in foundObjects)
            {
                if (topCardView.PlayerDecisionButton != null && topCardView.PlayerDecisionButton.PlayerDecision.DecisionId == playerDecision.DecisionId)
                {
                    found = topCardView;
                    break;
                }
            }

            if (found != null)
            {
                topCardViews.TryAdd(playerDecision, found);
            }
            else
            {
                Debug.LogError($"TopCardView with PlayerDecision '{playerDecision.Name}' not found.");

            }
        }


        private void FindUIComponents()
        {

            foreach (PlayerDecision playerDecision in PlayerDecision.GetUIOrientedDecisions())
            {
                FindAndAssign<PlayerDecisionButton>(playerDecision, UIOrientedDecisions);
            }

            foreach (PlayerDecision playerDecision in PlayerDecision.GetMainBettingDecisions())
            {
                FindAndAssign<PlayerDecisionButton>(playerDecision, BettingButtons);
            }


            if (BettingButtons.TryGetValue(PlayerDecision.SeeHand, out PlayerDecisionButton button))
            {
                ShowPlayerHand = button;
                LocalPlayerCardViews = button.GetComponentsInChildren<CardView>(true);
            }

            FloorCardView = GameObject.Find(nameof(FloorCardView)).GetComponent<CardView>();


            if (FloorCardView != null)
            {
                FloorCardDraggable = FloorCardView.GetComponent<Draggable>();
                FloorCardDraggable.LocalPlayerCardViews = LocalPlayerCardViews;
            }

            foreach (PlayerDecision playerDecision in PlayerDecision.GetExtraGamePlayDecisions())
            {
                FindAndAssignTopCardView<TopCardView>(playerDecision, TopCardViews);
            }


            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));

            Pot = transform.FindChildRecursively<TextMeshPro>(nameof(Pot));
            CurrentBet = transform.FindChildRecursively<TextMeshPro>(nameof(CurrentBet));

            RoundNumber = transform.FindChildRecursively<TextMeshPro>(nameof(RoundNumber));
            RoundNumberOf = transform.FindChildRecursively<TextMeshPro>(nameof(RoundNumberOf));


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
                ShowMessage("Please enter a valid raise amount.", "OK", 3f).Forget();
                raiseAmount = 0;
                return false;
            }
        }
        

        private void ResetAllCardViews()
        {
            SetLocalPlayerHand();
            UpdateUIOrientedButtons();
            LeftPanelController.ResetView();
            ShowPlayerHand.SetInteractable(true);
        }

        private void SetLocalPlayerHand(Card card = null)
        {
            if (LocalPlayerCardViews is { Length: > 0 })
            {
                foreach (CardView cardView in LocalPlayerCardViews)
                {
                    cardView.SetCard(card);
                    cardView.UpdateCardView();
                    if (!HumanPlayer.HasSeenHand.Value)
                    {
                        cardView.ShowBackside();
                    }
                }
            }
        }

        private async UniTask ShowMessage(string message, string buttonName, float delay = 5f)
        {
            await EventBus.Instance.PublishAsync(new UIMessageEvent(buttonName, message, delay));
            await UniTask.Yield();
        }




    }
}