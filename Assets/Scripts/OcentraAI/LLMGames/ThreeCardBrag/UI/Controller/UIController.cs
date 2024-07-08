using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using OcentraAI.LLMGames.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames
{
    public class UIController : MonoBehaviour
    {
        [ShowInInspector]
        private DeckManager DeckManager { get; set; }

        [Required, ShowInInspector]
        private Button ShowPlayerHand { get; set; }
        [Required, ShowInInspector]
        private Button PlayBlind { get; set; }
        [Required, ShowInInspector]
        private Button RaiseBet { get; set; }

        [Required, ShowInInspector]
        private Button Fold { get; set; }

        [Required, ShowInInspector]
        private Button DrawFromDeck { get; set; }

        [Required, ShowInInspector]
        private Button PickFromFloor { get; set; }

        [Required, ShowInInspector]
        private Button ShowCall { get; set; }

        //[Required, ShowInInspector]
        //private List<Button> Discard { get; set; } = new List<Button>();

        [Required, ShowInInspector]
        private Button ContinueRound { get; set; }

        [Required, ShowInInspector]
        private Button NewGame { get; set; }
        [Required, ShowInInspector]
        private Button PurchaseCoins { get; set; }

        [Required, ShowInInspector]
        private Transform ComputerHand { get; set; }

        [Required, ShowInInspector]
        private Transform MessageHolder { get; set; }

        [Required, ShowInInspector]
        private TMP_InputField RaiseAmount { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI Message { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI HumanPlayersCoins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI ComputerPlayerCoins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI HumanPlayersWins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI ComputerPlayerWins { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI Pot { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI CurrentBet { get; set; }

        [Required, ShowInInspector]
        private TextMeshProUGUI ComputerPlayingBlind { get; set; }

        [Required, ShowInInspector]
        private PlayerTimer HumanPlayerTimer { get; set; }

        [Required, ShowInInspector]
        private PlayerTimer ComputerPlayerTimer { get; set; }

        [Required, ShowInInspector]
        private CardView[] HumanPlayerCardViews { get; set; }

        [Required, ShowInInspector]
        private CardView FloorCardView { get; set; }

        [Required, ShowInInspector]
        private CardView[] ComputerPlayerCardViews { get; set; }

        public bool ActionTaken { get; set; }

        [Required, ShowInInspector]
        public LeftPanelController LeftPanelController { get; set; }

        void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            if (GameManager.Instance != null)
            {
                DeckManager = GameManager.Instance.DeckManager;
            }

            ComputerHand = transform.FindChildRecursively<Transform>(nameof(ComputerHand));
            MessageHolder = transform.FindChildRecursively<Transform>(nameof(MessageHolder));

            ShowPlayerHand = transform.FindChildRecursively<Button>(nameof(ShowPlayerHand));
            PlayBlind = transform.FindChildRecursively<Button>(nameof(PlayBlind));
            RaiseBet = transform.FindChildRecursively<Button>(nameof(RaiseBet));
            Fold = transform.FindChildRecursively<Button>(nameof(Fold));
            DrawFromDeck = transform.FindChildRecursively<Button>(nameof(DrawFromDeck));
            PickFromFloor = transform.FindChildRecursively<Button>(nameof(PickFromFloor));
            ShowCall = transform.FindChildRecursively<Button>(nameof(ShowCall));
            ContinueRound = transform.FindChildRecursively<Button>(nameof(ContinueRound));
            NewGame = transform.FindChildRecursively<Button>(nameof(NewGame));
            PurchaseCoins = transform.FindChildRecursively<Button>(nameof(PurchaseCoins));

            HumanPlayerCardViews = ShowPlayerHand.GetComponentsInChildren<CardView>();
            ComputerPlayerCardViews = ComputerHand.GetComponentsInChildren<CardView>();

            FloorCardView = transform.FindChildRecursively<CardView>(nameof(FloorCardView));

            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));
            HumanPlayersCoins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HumanPlayersCoins));
            ComputerPlayerCoins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayerCoins));
            Pot = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Pot));
            CurrentBet = transform.FindChildRecursively<TextMeshProUGUI>(nameof(CurrentBet));
            ComputerPlayingBlind = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayingBlind));

            ComputerPlayerWins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayerWins));
            HumanPlayersWins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HumanPlayersWins));

            HumanPlayerTimer = transform.parent.FindChildRecursively<PlayerTimer>(nameof(HumanPlayerTimer));
            ComputerPlayerTimer = transform.parent.FindChildRecursively<PlayerTimer>(nameof(ComputerPlayerTimer));
            StopTurnCountdown();

            RaiseAmount = transform.FindChildRecursively<TMP_InputField>(nameof(RaiseAmount));
            if (NewGame != null) NewGame.gameObject.SetActive(false);
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(false);
            if (MessageHolder != null) MessageHolder.gameObject.SetActive(false);

            //foreach (CardView cardView in HumanPlayerCardViews)
            //{
            //    Button component = cardView.GetComponent<Button>();
            //    component.enabled = false;
            //    if (!Discard.Contains(component))
            //    {
            //        Discard.Add(component);
            //    }
            //}

            LeftPanelController = FindObjectOfType<LeftPanelController>();
        }

        private void Start()
        {
            Init();

            if (ShowPlayerHand != null) ShowPlayerHand.onClick.AddListener(OnShowPlayerHand);
            if (PlayBlind != null) PlayBlind.onClick.AddListener(OnPlayBlind);
            if (RaiseBet != null) RaiseBet.onClick.AddListener(OnRaiseBet);
            if (Fold != null) Fold.onClick.AddListener(OnFold);
            if (DrawFromDeck != null) DrawFromDeck.onClick.AddListener(OnDrawFromDeck);
            //if (PickFromFloor != null) PickFromFloor.onClick.AddListener(OnPickFromFloor);
            if (ShowCall != null) ShowCall.onClick.AddListener(OnShowCall);

            //foreach (Button button in Discard)
            //{
            //    CardView cardView = button.GetComponent<CardView>();
            //    button.onClick.AddListener(() => OnDiscardCardSet(cardView));
            //}

            if (ContinueRound != null) ContinueRound.onClick.AddListener(() => GameManager.Instance.ContinueGame(true));
            if (NewGame != null) NewGame.onClick.AddListener(() => GameManager.Instance.StartNewGame());
            if (PurchaseCoins != null)
            {
                PurchaseCoins.onClick.AddListener(() => GameManager.Instance.PurchaseCoins(GameManager.Instance.HumanPlayer, 1000));
            }
        }

        private void OnShowCall()
        {
            TakeAction(PlayerAction.Show);
        }

        private void OnDrawFromDeck()
        {

            TakeAction(PlayerAction.DrawFromDeck);
        }

        private void OnFold()
        {
            TakeAction(PlayerAction.Fold);
        }

        private void OnPlayBlind()
        {
            TakeAction(PlayerAction.PlayBlind);
        }

        public void OnDiscardCardSet(CardView cardView)
        {
            if (DeckManager != null)
            {
                Card card = cardView.Card;

                DeckManager.SetSwapCard(card);
            }
        }

        public void OnPickFromFloor()
        {
            ShowMessage($"Drop the Card to Hand Card to discard, else Draw new card ", 5f);
            StartCoroutine(WaitForSwapCardIndex());
        }
        private void OnShowPlayerHand()
        {
            ShowPlayerHand.enabled = false;
            TakeAction(PlayerAction.SeeHand);
        }


        private void OnRaiseBet()
        {
            if (string.IsNullOrEmpty(RaiseAmount.text))
            {
                ShowMessage($" Please Set RaiseAmount ! Needs to be higher than CurrentBet {GameManager.Instance.CurrentBet}", 5f);
                return;
            }

            if (int.TryParse(RaiseAmount.text, out int raiseAmount) && raiseAmount > GameManager.Instance.CurrentBet)
            {
                GameManager.Instance.SetCurrentBet(raiseAmount);
                TakeAction(PlayerAction.Raise);
            }
            else
            {
                ShowMessage($" RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {GameManager.Instance.CurrentBet}", 5f);
            }
        }
        private IEnumerator WaitForSwapCardIndex()
        {
            if (DeckManager != null)
            {
                yield return new WaitUntil(() => DeckManager.SwapCard != null);
                TakeAction(PlayerAction.PickAndSwap);
            }
            else
            {
                Debug.LogError($"DeckManager is null!");
            }
        }
        public void SetComputerSeenHand(bool hasSeenHand)
        {
            string message = GameManager.Instance.BlindMultiplier > 1 ? $" Playing Blind {GameManager.Instance.BlindMultiplier}" : $" Playing Blind ";
            ComputerPlayingBlind.text = hasSeenHand ? "" : message;
        }



        private async void TakeAction(PlayerAction action)
        {
            await GameManager.Instance.HandlePlayerAction(action);


            ActionTaken = true;
        }

        public void UpdateGameState(bool isNewRound = false)
        {
            UpdateCoinsDisplay();
            UpdatePotDisplay();
            UpdateCurrentBetDisplay();
            UpdateHumanPlayerHandDisplay();
            UpdateFloorCard();
            if (isNewRound)
            {
                ResetAllCardViews();
            }
        }

        private void ResetAllCardViews()
        {
            foreach (CardView cardView in HumanPlayerCardViews)
            {
                cardView.ResetCardView();
            }

            foreach (CardView cardView in ComputerPlayerCardViews)
            {
                cardView.ResetCardView();
            }

            LeftPanelController.ResetView();
        }

        public void EnablePlayerActions()
        {
            bool humanPlayerHasSeenHand = GameManager.Instance.HumanPlayer.HasSeenHand;
            bool isCurrentPlayerHuman = GameManager.Instance.CurrentTurn.CurrentPlayer == GameManager.Instance.HumanPlayer;

            if (PlayBlind != null) PlayBlind.gameObject.SetActive(!humanPlayerHasSeenHand);
            if (RaiseBet != null) RaiseBet.interactable = isCurrentPlayerHuman;
            if (Fold != null) Fold.interactable = isCurrentPlayerHuman;
            if (DrawFromDeck != null)
            {
                DrawFromDeck.interactable = humanPlayerHasSeenHand && isCurrentPlayerHuman;
            }

            //if (PickFromFloor != null) PickFromFloor.interactable = humanPlayerHasSeenHand && DeckManager.FloorCard != null;
            if (ShowCall != null) ShowCall.interactable = isCurrentPlayerHuman;


        }

        public void UpdateCoinsDisplay()
        {
            if (HumanPlayersCoins != null) HumanPlayersCoins.text = $"{GameManager.Instance.HumanPlayer.Coins}";
            if (ComputerPlayerCoins != null) ComputerPlayerCoins.text = $"{GameManager.Instance.ComputerPlayer.Coins}";
        }

        public void UpdatePotDisplay()
        {
            if (Pot != null) Pot.text = $"{GameManager.Instance.Pot}";
        }

        public void UpdateCurrentBetDisplay()
        {
            if (CurrentBet != null) CurrentBet.text = $"Current Bet: {GameManager.Instance.CurrentBet} ";
        }

        public void UpdateRoundDisplay()
        {
            ComputerPlayerWins.text = $"{GameManager.Instance.ScoreKeeper.ComputerTotalWins}";
            HumanPlayersWins.text = $"{GameManager.Instance.ScoreKeeper.HumanTotalWins}";
        }

        public void UpdateFloorCard()
        {
            if (FloorCardView != null)
            {
                if (DeckManager != null)
                {
                    FloorCardView.SetCard(DeckManager.FloorCard);

                    FloorCardView.UpdateCardView();
                    bool value = FloorCardView.Card != null;
                    FloorCardView.SetActive(value);
                }
                else
                {

                    FloorCardView.SetActive(false);
                }

            }
        }

        public void UpdateFloorCards(Card card)
        {
            LeftPanelController.AddCard(card);
        }

        public void ShowMessage(string message, float delay = 5f)
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(true);
                if (Message != null)
                {
                    Message.text = message;
                }

                // Check if the object is still valid before starting the coroutine
                if (this != null)
                {
                    StartCoroutine(HideMessageAfterDelay(delay));
                }
            }
        }

        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);
            }
        }

        public void OfferContinuation(float delay)
        {
            ShowMessage($" Do you Want to ContinueRound playing One More Round?", delay);
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(true);
        }

        public void OfferNewGame()
        {
            ShowMessage($"Do you want to Play New Round ?", 15f);

            if (NewGame != null) NewGame.gameObject.SetActive(true);
        }

        public void StartTurnCountdown(Player player, float duration)
        {
            if (player is HumanPlayer)
            {
                HumanPlayerTimer.StartTimer(duration);
                HumanPlayerTimer.Show(true);
                ComputerPlayerTimer.Show(false);
            }
            else
            {
                ComputerPlayerTimer.StartTimer(duration);
                HumanPlayerTimer.Show(false);
                ComputerPlayerTimer.Show(true);
            }
        }

        public void StopTurnCountdown()
        {
            HumanPlayerTimer.Show(false);
            ComputerPlayerTimer.Show(false);
        }

        public void UpdateHumanPlayerHandDisplay(bool isRoundEnd = false)
        {
            if (GameManager.Instance.HumanPlayer.HasSeenHand || isRoundEnd)
            {
                for (int i = 0; i < GameManager.Instance.HumanPlayer.Hand.Count; i++)
                {
                    HumanPlayerCardViews[i].SetCard(GameManager.Instance.HumanPlayer.Hand[i]);
                    HumanPlayerCardViews[i].UpdateCardView();
                }

            }


        }

        public void ActivateDiscardCard(bool activate)
        {
            //Discard.ForEach(button =>
            //{
            //    button.enabled = true;
            //    button.interactable = activate;
            //});

        }

        public void UpdateComputerHandDisplay(bool isRoundEnd = false)
        {
            for (int i = 0; i < GameManager.Instance.ComputerPlayer.Hand.Count; i++)
            {
                ComputerPlayerCardViews[i].SetCard(GameManager.Instance.ComputerPlayer.Hand[i]);
            }

            if (isRoundEnd)
            {
                foreach (CardView cardView in ComputerPlayerCardViews)
                {
                    cardView.UpdateCardView();
                }
            }
            else
            {
                foreach (CardView cardView in ComputerPlayerCardViews)
                {
                    cardView.ShowBackside();
                }
            }
        }
    }
}
