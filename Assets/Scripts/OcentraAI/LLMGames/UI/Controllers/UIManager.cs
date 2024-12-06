using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.UI;
using OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers;
using OcentraAI.LLMGames.UI.Controllers;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class UIManager : SerializedMonoBehaviour, IUIManager
    {
        #region UI Elements

        [ShowInInspector] protected IPlayerData Player { get; set; }
        bool IsPlayerTurn => Player is { IsPlayerTurn: { Value: true } };
        private Button3D ShowPlayerHand { get; set; }

        [Required, ShowInInspector] private Transform MessageHolder { get; set; }
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

        [HideInInspector] public Dictionary<PlayerDecision, CardView> TopCardViews { get; set; } = new Dictionary<PlayerDecision, CardView>();

        [HideInInspector] public Dictionary<PlayerDecision, PlayerDecisionButton> UIOrientedDecisions { get; set; } = new Dictionary<PlayerDecision, PlayerDecisionButton>();

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
            EventBus.Instance.Subscribe<UpdateWildCardsEvent<Card>>(OnUpdateWildCards);
            EventBus.Instance.Subscribe<UpdateFloorCardEvent<Card>>(OnUpdateFloorCard);
            EventBus.Instance.Subscribe<UpdatePlayerHandDisplayEvent>(OnUpdatePlayerHandDisplay);
            EventBus.Instance.Subscribe<UpdateScoreDataEvent<INetworkRoundRecord>>(OnUpdateScoreDataEvent);
            EventBus.Instance.Subscribe<UpdateWildCardsHighlightEvent>(OnUpdateWildCardsHighlight);


        }



        public void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<UIMessageEvent>(OnMessage);
            EventBus.Instance.Unsubscribe<RegisterLocalPlayerEvent>(OnRegisterLocalPlayerEvent);
            EventBus.Instance.Unsubscribe<TimerStartEvent>(OnTimerStartEvent);
            EventBus.Instance.Unsubscribe<TimerStopEvent>(OnTimerStopEvent);
            EventBus.Instance.Unsubscribe<UpdateWildCardsEvent<Card>>(OnUpdateWildCards);
            EventBus.Instance.Unsubscribe<UpdateFloorCardEvent<Card>>(OnUpdateFloorCard);
            EventBus.Instance.Unsubscribe<UpdatePlayerHandDisplayEvent>(OnUpdatePlayerHandDisplay);
            EventBus.Instance.Unsubscribe<UpdateScoreDataEvent<INetworkRoundRecord>>(OnUpdateScoreDataEvent);
            EventBus.Instance.Unsubscribe<UpdateWildCardsHighlightEvent>(OnUpdateWildCardsHighlight);



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
            SetMainBettingButtons();

        }



        private void OnTimerStopEvent(TimerStopEvent obj)
        {
            UpdateFloorCardDraggable();
            UpdateUIOrientedButtons();
            UpdateTopCardView();
            SetMainBettingButtons();
        }

        private void SetMainBettingButtons()
        {
            

            foreach (PlayerDecisionButton playerDecisionButton in BettingButtons.Values)
            {
                if (playerDecisionButton.PlayerDecision == PlayerDecision.PlayBlind)
                {
                    playerDecisionButton.gameObject.SetActive(!Player.HasSeenHand.Value);
                    playerDecisionButton.SetInteractable(!Player.HasSeenHand.Value && IsPlayerTurn);
                }
                else if (playerDecisionButton.PlayerDecision == PlayerDecision.SeeHand)
                {
                    if (Player.HasSeenHand.Value)
                    {
                        playerDecisionButton.SetInteractable(false, false);
                    }
                    else
                    {
                        playerDecisionButton.SetInteractable(IsPlayerTurn);
                    }
                }
                else
                {
                    playerDecisionButton.gameObject.SetActive(Player.HasSeenHand.Value);
                    playerDecisionButton.SetInteractable(Player.HasSeenHand.Value && IsPlayerTurn);
                }
            }
        }

        private void UpdateTopCardView()
        {
            foreach (CardView cardView in TopCardViews.Values)
            {
                cardView.SetInteractable(IsPlayerTurn);
            }
        }

        private void UpdateUIOrientedButtons()
        {
            foreach (PlayerDecisionButton button in UIOrientedDecisions.Values)
            {
                if (button.PlayerDecision == PlayerDecision.PurchaseCoins)
                {
                    button.gameObject.SetActive(Player.Coins.Value <= 0);
                }

                button.SetInteractable(IsPlayerTurn);
            }
        }


        private void OnUpdateWildCards(UpdateWildCardsEvent<Card> e)
        {
            foreach ((PlayerDecision playerDecision, CardView cardView) in TopCardViews)
            {
                if (e.WildCards.TryGetValue(playerDecision, out Card card))
                {
                    cardView.SetCard(card);
                    cardView.UpdateCardView();
                    cardView.gameObject.SetActive(true);
                }
                else
                {
                    cardView.SetCard(null);
                    cardView.UpdateCardView();
                    cardView.gameObject.SetActive(false);
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
                FloorCardDraggable.IsPlayerTurn = IsPlayerTurn;

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
            if (e.Player == Player)
            {
                if (e.Player.HasSeenHand.Value)
                {
                    for (int i = 0; i < LocalPlayerCardViews.Length; i++)
                    {
                        string handCard = e.Player.GetCard(i);
                        Card card = CardUtility.GetCardFromSymbol(handCard);
                        LocalPlayerCardViews[i].SetCard(card);
                        LocalPlayerCardViews[i].UpdateCardView();
                    }
                }
            }

            SetMainBettingButtons();
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


            Pot.text = $"Total Pot : {pot}";
            CurrentBet.text = $"CurrentBet : {currentBet}";
            RoundNumber.text = $"{currentRound}";
            RoundNumberOf.text = $"{totalRounds}";
        }




        private void OnRegisterLocalPlayerEvent(RegisterLocalPlayerEvent arg)
        {
            Player = arg.LocalPlayer;
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
            }

            SetLocalPlayerHand();
            UpdateUIOrientedButtons();
            UpdateTopCardView();
            SetMainBettingButtons();
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

        private void FindAndAssignTopCardView<T>(PlayerDecision playerDecision, Dictionary<PlayerDecision, CardView> topCardViews) where T : TopCardView
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
            MessageHolder = transform.FindChildRecursively<Transform>(nameof(MessageHolder));

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


            if (FloorCardView !=null)
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

        }

        private void ResetAllCardViews()
        {
            SetLocalPlayerHand();
            UpdateUIOrientedButtons();
            UpdateTopCardView();
            SetMainBettingButtons();
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
                }
            }
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