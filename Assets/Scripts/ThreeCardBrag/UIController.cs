using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeCardBrag
{
    public class UIController : MonoBehaviour
    {



        [Required, ShowInInspector]
        private Button ShowPlayerHand { get;  set; }
        [Required, ShowInInspector]
        private Button PlayBlind { get;  set; }
        [Required, ShowInInspector]
        private Button RaiseBet { get; set; }

        [Required, ShowInInspector]
        private Button Fold { get;  set; }

        [Required, ShowInInspector]
        private Button DrawFromDeck { get; set; }

        [Required, ShowInInspector]
        private Button PickFromFloor { get; set; }

        [Required, ShowInInspector]
        private Button ShowCall { get; set; }

        [Required, ShowInInspector]
        private List<Button> Discard { get; set; } = new List<Button>();

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
        private PlayerTimer HumanPlayerTimer { get; set; }

        [Required, ShowInInspector]
        private PlayerTimer ComputerPlayerTimer { get; set; }

        [Required, ShowInInspector]
        private CardView[] PlayerCardViews { get; set; }

        [Required, ShowInInspector]
        private CardView FloorCardView { get; set; }

        [Required, ShowInInspector]
        private CardView[] ComputerCardViews { get; set; }

        public bool ActionTaken { get; set; }

        public int SwapCardIndex = -1;


        void OnValidate()
        {
            Init();
        }

        private void Init()
        {
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


            PlayerCardViews = ShowPlayerHand.GetComponentsInChildren<CardView>();
            ComputerCardViews = ComputerHand.GetComponentsInChildren<CardView>();

            FloorCardView = transform.FindChildRecursively<CardView>(nameof(FloorCardView));

            Message = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Message));
            HumanPlayersCoins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HumanPlayersCoins));
            ComputerPlayerCoins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayerCoins));
            Pot = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Pot));
            CurrentBet = transform.FindChildRecursively<TextMeshProUGUI>(nameof(CurrentBet));

            ComputerPlayerWins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(ComputerPlayerWins));
            HumanPlayersWins = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HumanPlayersWins));
            
            HumanPlayerTimer = transform.parent.FindChildRecursively<PlayerTimer>(nameof(HumanPlayerTimer));
            ComputerPlayerTimer = transform.parent.FindChildRecursively<PlayerTimer>(nameof(ComputerPlayerTimer));
            StopTurnCountdown();

            RaiseAmount = transform.FindChildRecursively<TMP_InputField>(nameof(RaiseAmount));
            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(false);

            }

            if (ContinueRound != null)
            {
                ContinueRound.gameObject.SetActive(false);

            }

            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(false);

            }



            foreach (CardView cardView in PlayerCardViews)
            {
                Button component = cardView.GetComponent<Button>();
                component.enabled = false;
                if (!Discard.Contains(component))
                {
                    Discard.Add(component);

                }

            }


        }

        private void Start()
        {
            Init();

            if (ShowPlayerHand != null)
            {
                ShowPlayerHand.onClick.AddListener(OnShowPlayerHand);
            }



            if (PlayBlind != null) PlayBlind.onClick.AddListener(() => TakeAction(PlayerAction.PlayBlind));

            if (RaiseBet != null)
            {
                RaiseBet.onClick.AddListener(OnRaiseBet);
            }


            if (Fold != null) Fold.onClick.AddListener(() => TakeAction(PlayerAction.Fold));

            if (DrawFromDeck != null) DrawFromDeck.onClick.AddListener(() => TakeAction(PlayerAction.DrawFromDeck));

            if (PickFromFloor != null)
            {

                PickFromFloor.onClick.AddListener(OnPickFromFloor);
            }

            if (ShowCall != null) ShowCall.onClick.AddListener(() => TakeAction(PlayerAction.Show));

            for (int i = 0; i < Discard.Count; i++)
            {
                Discard[i].onClick.AddListener(()=>OnDiscardCardClicked(i));
            }

            if (ContinueRound != null) ContinueRound.onClick.AddListener(() => GameController.Instance.ContinueGame(true));
            if (NewGame != null) NewGame.onClick.AddListener(() => GameController.Instance.StartNewGame());

            if (PurchaseCoins != null)
            {
                PurchaseCoins.onClick.AddListener(() => GameController.Instance.PurchaseCoins(GameController.Instance.HumanPlayer, 1000));
            }

        }

        private void OnDiscardCardClicked(int i)
        {
            SwapCardIndex = i;
        }


        private void OnPickFromFloor()
        {
            Discard.ForEach(b => b.enabled = true);
            ShowMessage($"Pick A card to discard else Draw new card ");
            // todo wait for SwapIndex be greater than -1
            TakeAction(PlayerAction.PickAndSwap);
        }

        private void OnRaiseBet()
        {
            if (string.IsNullOrEmpty(RaiseAmount.text))
            {
                ShowMessage($" Please Set RaiseAmount ! Needs to be higher than CurrentBet {GameController.Instance.CurrentBet}");

                return;
            }

            if (int.TryParse(RaiseAmount.text, out int raiseAmount) &&
                raiseAmount > GameController.Instance.CurrentBet)
            {
                GameController.Instance.SetCurrentBet(raiseAmount); 
                TakeAction(PlayerAction.Raise);
            }
            else
            {
                ShowMessage($" RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {GameController.Instance.CurrentBet}");
            }
        }

        private void OnShowPlayerHand()
        {
            ShowPlayerHand.enabled = false;
           
            PlayBlind.gameObject.SetActive(false);
            TakeAction(PlayerAction.SeeHand);
        }

        private void TakeAction(PlayerAction action, int amount = -1)
        {
            GameController.Instance.HandlePlayerAction(action);
            ActionTaken = true;
        }

        public void UpdateGameState()
        {
            UpdateCoinsDisplay();
            UpdatePotDisplay();
            UpdateCurrentBetDisplay();
            UpdateHumanPlayerHandDisplay();
            UpdateFloorCard();
            UpdateComputerHandDisplay();
            SwapCardIndex = -1;
        }

        public void EnablePlayerActions(bool hasSeen)
        {
            if (PlayBlind != null) PlayBlind.gameObject.SetActive(!hasSeen);
            if (RaiseBet != null) RaiseBet.interactable = hasSeen;
            if (Fold != null) Fold.interactable = hasSeen;
            if (DrawFromDeck != null) DrawFromDeck.interactable = hasSeen;
            if (PickFromFloor != null) PickFromFloor.interactable = hasSeen && GameController.Instance.DeckManager.FloorCard != null;
            if (ShowCall != null) ShowCall.interactable = true;

            foreach (Button button in Discard)
            {
                button.interactable = hasSeen;
            }
        }

        public void UpdateCoinsDisplay()
        {
            if (HumanPlayersCoins != null) HumanPlayersCoins.text = $"{GameController.Instance.HumanPlayer.Coins}";
            if (ComputerPlayerCoins != null) ComputerPlayerCoins.text = $"{GameController.Instance.ComputerPlayer.Coins}";
        }

        public void UpdatePotDisplay()
        {
            if (Pot != null) Pot.text = $"{GameController.Instance.Pot}";
        }

        public void UpdateCurrentBetDisplay()
        {
            if (CurrentBet != null) CurrentBet.text = $"Current Bet: {GameController.Instance.CurrentBet} ";
        }

        public void UpdateRoundDisplay()
        {
            ComputerPlayerWins.text = $"{GameController.Instance.ScoreKeeper.ComputerTotalWins}";
            HumanPlayersWins.text = $"{GameController.Instance.ScoreKeeper.HumanTotalWins}";

        }

        public void UpdateFloorCard()
        {
            if (FloorCardView != null)
            {
                if (GameController.Instance.DeckManager.FloorCard != null)
                {
                    FloorCardView.SetCard(GameController.Instance.DeckManager.FloorCard);
                }
                
                FloorCardView.SetActive(GameController.Instance.DeckManager.FloorCard != null);
            }


        }



        public void ShowMessage(string message)
        {
            if (MessageHolder != null)
            {
                MessageHolder.gameObject.SetActive(true);
                if (Message != null)
                {
                    Message.text = message;
                }
                StartCoroutine(HideMessageAfterDelay(5f)); 
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
        public void OfferContinuation()
        {
            ShowMessage($" Do you Want to ContinueRound playing One More Round?");
            if (ContinueRound != null) ContinueRound.gameObject.SetActive(true);
        }

        public void OfferNewGame()
        {
            ShowMessage($"do you want to Play New Round ?");

            if (NewGame != null)
            {
                NewGame.gameObject.SetActive(true);
            }
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

        public void UpdateHumanPlayerHandDisplay()
        {
            
            if (GameController.Instance.HumanPlayer.HasSeenHand)
            {
                for (int i = 0; i < GameController.Instance.HumanPlayer.Hand.Count; i++)
                {
                    PlayerCardViews[i].SetCard(GameController.Instance.HumanPlayer.Hand[i]);
                    PlayerCardViews[i].UpdateCardView();
                }
            }
 
        }

        public void UpdateComputerHandDisplay(bool isRoundEnd = false)
        {
            if (isRoundEnd)
            {
                foreach (CardView cardView in ComputerCardViews)
                {
                    cardView.UpdateCardView();
                }
            }
            else
            {
                foreach (CardView cardView in ComputerCardViews)
                {
                    cardView.ShowBackside();
                }
            }

        }
    }
}