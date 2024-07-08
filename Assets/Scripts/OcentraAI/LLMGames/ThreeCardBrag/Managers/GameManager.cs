using System;
using System.Collections;
using System.Threading.Tasks;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.ScriptableSingletons;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    [RequireComponent(typeof(LLMManager))]

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [ShowInInspector]
        public HumanPlayer HumanPlayer { get; private set; } = new HumanPlayer();

        [ShowInInspector]
        public ComputerPlayer ComputerPlayer { get; private set; } = new ComputerPlayer();

        [ShowInInspector]
        public UIController UIController { get; private set; }

        [ShowInInspector, ReadOnly]
        public ScoreKeeper ScoreKeeper { get; private set; } = new ScoreKeeper();

        public bool IsHumanTurn = true;
        public float TurnDuration = 15f;

        [ShowInInspector, ReadOnly]
        public int Pot { get; private set; } = 0;

        [ShowInInspector]
        public int BaseBet { get; private set; } = 10;

        [ShowInInspector, ReadOnly]
        public int CurrentBet { get; private set; }

        [ShowInInspector, ReadOnly]
        public int BlindMultiplier { get; private set; } = 1;

        [ShowInInspector]
        public int InitialCoins { get; private set; } = 1000;

        [ShowInInspector, ReadOnly]
        private int CurrentRound { get; set; } = 0;

        [ShowInInspector, ReadOnly]
        public DeckManager DeckManager { get; private set; }


        [ShowInInspector]
        public int MaxRounds { get; private set; } = 10;

        [ShowInInspector]
        public TurnInfo CurrentTurn;
        
        public AIHelper AIHelper { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            UIController = FindObjectOfType<UIController>();
            HumanPlayer.SetName(nameof(HumanPlayer));
            ComputerPlayer.SetName(nameof(ComputerPlayer));
            DeckManager = new DeckManager();
            AIHelper = new AIHelper(GameInfo.Instance, this);

        }

        private void Start()
        {
            Init();

            HumanPlayer.OnActionTaken += async (action) => await HandlePlayerAction(action);
            ComputerPlayer.OnActionTaken += async (action) => await HandlePlayerAction(action);
            HumanPlayer.OnCoinsChanged += UIController.UpdateCoinsDisplay;
            HumanPlayer.OnCoinsChanged += UIController.UpdateCoinsDisplay;
            ComputerPlayer.OnCoinsChanged += UIController.UpdateCoinsDisplay;

            StartNewGame();
        }

        public void StartNewGame()
        {
            CurrentRound = 0;
            HumanPlayer.AdjustCoins(InitialCoins);
            ComputerPlayer.AdjustCoins(InitialCoins);
            StartNewRound();
        }

        public void SetCurrentBet(int bet)
        {
            CurrentBet = bet;
        }
        private void StartNewRound()
        {
            CurrentRound++;
            DeckManager.Reset();
            HumanPlayer.ResetForNewRound();
            ComputerPlayer.ResetForNewRound();
            Pot = 0;
            CurrentBet = BaseBet;
            BlindMultiplier = 1;
            IsHumanTurn = true;

            // Deal initial hands
            for (int i = 0; i < 3; i++)
            {
                HumanPlayer.Hand.Add(DeckManager.DrawCard());
                ComputerPlayer.Hand.Add(DeckManager.DrawCard());
            }

            UIController.UpdateGameState();
            StartCoroutine(PlayerTurn());
        }

        private IEnumerator PlayerTurn()
        {
            Player currentPlayer = IsHumanTurn ? HumanPlayer : ComputerPlayer;
            UIController.StartTurnCountdown(currentPlayer, TurnDuration);
            UIController.EnablePlayerActions();
            if (IsHumanTurn)
            {
                
                CurrentTurn = new TurnInfo(currentPlayer)
                {
                    ElapsedTime = 0f
                };

                while (CurrentTurn.ElapsedTime < TurnDuration && !UIController.ActionTaken)
                {
                    CurrentTurn.ElapsedTime += Time.deltaTime;
                    yield return null;
                }
                CurrentTurn.TurnCompletionSource.TrySetResult(true);

                if (!UIController.ActionTaken)
                {
                    // Player did not take action in time, pass turn to the other player
                    UIController.ShowMessage($"{currentPlayer.PlayerName} took too long, turn is passed to the opponent!", 5f);
                }
            }
            else
            {

                ComputerPlayer.MakeDecision(CurrentBet);
                CurrentTurn = new TurnInfo(currentPlayer)
                {
                    ElapsedTime = 0f
                };

                while (CurrentTurn.ElapsedTime < TurnDuration && !UIController.ActionTaken)
                {
                    CurrentTurn.ElapsedTime += Time.deltaTime;
                    yield return null;
                }
                CurrentTurn.TurnCompletionSource.TrySetResult(true);


                if (!UIController.ActionTaken)
                {
                    // Player did not take action in time, pass turn to the other player
                    UIController.ShowMessage($"{currentPlayer.PlayerName} took too long, turn is passed to the opponent!", 5f);
                }

            }
            UIController.ActionTaken = false;
        }


        public async Task HandlePlayerAction(PlayerAction action)
        {
           await ProcessPlayerAction(action);

            if (action != PlayerAction.Fold && action != PlayerAction.Show)
            {
                SwitchTurn();
            }

        }

        private async Task ProcessPlayerAction(PlayerAction action)
        {
            Player currentPlayer = IsHumanTurn ? HumanPlayer : ComputerPlayer;

            switch (action)
            {
                case PlayerAction.SeeHand:
                    currentPlayer.SeeHand();
                    UIController.EnablePlayerActions();
                    UIController.UpdateHumanPlayerHandDisplay();
                    UIController.ActivateDiscardCard(true);
                    await WaitForTurnCompletion(currentPlayer,CurrentTurn);
                    UIController.ActivateDiscardCard(false);
                    break;
                case PlayerAction.PlayBlind:
                    PlayBlind(currentPlayer);
                    break;
                case PlayerAction.Bet:
                case PlayerAction.Call:
                    Call(currentPlayer);
                    break;
                case PlayerAction.Raise:
                    Raise(currentPlayer);
                    break;
                case PlayerAction.Fold:
                    Fold(currentPlayer);
                    break;
                case PlayerAction.DrawFromDeck:
                    currentPlayer.DrawFromDeck();
                    await WaitForTurnCompletion(currentPlayer, CurrentTurn);

                    break;
                case PlayerAction.PickAndSwap:
                    currentPlayer.PickAndSwap();
                    break;
                case PlayerAction.Show:
                    Show();
                    break;


            }

            UIController.UpdateGameState();
        }

        public async Task WaitForTurnCompletion(Player currentPlayer, TurnInfo turnInfo)
        {
            if (turnInfo.CurrentPlayer == currentPlayer)
            {
                await turnInfo.TurnCompletionSource.Task;

            }
        }
        

        private void PlayBlind(Player player)
        {
            CurrentBet *= BlindMultiplier;
            if (player.Coins >= CurrentBet)
            {
                player.BetOnBlind();
                Pot += CurrentBet;
                BlindMultiplier *= 2;
            }
            else
            {
                UIController.ShowMessage($" not enough {player.Coins} coins  CurrentBet is {CurrentBet} {Environment.NewLine}You Need To Fold!", 5f);
            }
        }

        private void Call(Player player)
        {
            int betAmount = player.HasSeenHand ? CurrentBet * 2 : CurrentBet;
            if (player.Coins >= betAmount)
            {
                player.Bet();
                Pot += betAmount;
            }
            else
            {
                UIController.ShowMessage($" not enough {player.Coins} coins  CurrentBet is {CurrentBet} {Environment.NewLine}You Need To Fold!", 5f);
            }
        }

        private void Raise(Player player)
        {
            if (player.Coins >= CurrentBet)
            {
                player.Raise();
                Pot += CurrentBet;
            }
            else
            {
                UIController.ShowMessage($" not enough {player.Coins} coins  CurrentBet is {CurrentBet} {Environment.NewLine}You Need To Fold!", 5f);
            }
        }

        private void Fold(Player player)
        {
            player.Fold();
            EndRound(GetOtherPlayer(player));
        }


        private void Show()
        {
            HumanPlayer.ShowHand(true);
            ComputerPlayer.ShowHand(true);
            DetermineWinner();
        }

        private void SwitchTurn()
        {
            IsHumanTurn = !IsHumanTurn;
            StartCoroutine(PlayerTurn());
        }

        private Player GetOtherPlayer(Player currentPlayer)
        {
            return currentPlayer == HumanPlayer ? ComputerPlayer : HumanPlayer;
        }

        private void DetermineWinner()
        {
            int humanValue = HumanPlayer.CalculateHandValue();
            int computerValue = ComputerPlayer.CalculateHandValue();

            Player winner;
            if (humanValue > computerValue)
            {
                winner = HumanPlayer;
            }
            else if (computerValue > humanValue)
            {
                winner = ComputerPlayer;
            }
            else
            {
                int humanHighCard = HumanPlayer.GetHighestCardValue();
                int computerHighCard = ComputerPlayer.GetHighestCardValue();

                if (humanHighCard > computerHighCard)
                {
                    winner = HumanPlayer;
                }
                else if (computerHighCard > humanHighCard)
                {
                    winner = ComputerPlayer;
                }
                else
                {
                    // It's a tie
                    winner = null;
                }
            }

            EndRound(winner);
        }

        private async void EndRound(Player winner)
        {
            UIController.StopTurnCountdown();
            if (winner == null)
            {

                UIController.ShowMessage("It's a tie! Play Another Round !", 5f);
            }
            else
            {
                winner.AdjustCoins(Pot);
                ScoreKeeper.AddToTotalRoundScores(winner, Pot);
                UIController.ShowMessage($"{winner.PlayerName} wins the round and {Pot} coins!", 6f);
                await Utility.Delay(6f);
                UIController.UpdateRoundDisplay();
            }

            if (HumanPlayer.Coins <= 0 || ComputerPlayer.Coins <= 0)
            {
                EndGame();
            }
            else
            {
                CheckForContinuation();
            }

            Pot = 0;
            UIController.UpdateGameState();


        }

        private async void CheckForContinuation()
        {
            if (CurrentRound >= MaxRounds)
            {
                Player trailingPlayer = ScoreKeeper.HumanTotalWins < ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
                Player leadingPlayer = GetOtherPlayer(trailingPlayer);

                if (trailingPlayer.Coins > leadingPlayer.Coins)
                {
                    UIController.OfferContinuation(10);
                    await Utility.Delay(10f);

                }
                else
                {
                    EndGame();
                }
            }
            else
            {
                StartNewRound();
            }
        }


        public void ContinueGame(bool playerWantsToContinue)
        {
            if (playerWantsToContinue)
            {
                StartNewRound();
            }
            else
            {
                EndGame();
            }
        }

        private async void EndGame()
        {
            Player winner;
            if (HumanPlayer.Coins <= 0)
            {

                winner = ComputerPlayer;
            }
            else if (ComputerPlayer.Coins <= 0)
            {
                winner = HumanPlayer;

            }
            else if (ScoreKeeper.HumanTotalWins != ScoreKeeper.ComputerTotalWins)
            {

                winner = ScoreKeeper.HumanTotalWins > ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
            }
            else
            {
                winner = HumanPlayer.Coins > ComputerPlayer.Coins ? HumanPlayer : ComputerPlayer;

            }

            UIController.ShowMessage($"Game Over! {winner.PlayerName} wins the game!", 6f);

            await Utility.Delay(6f);
            UIController.OfferNewGame();
        }





        public void PurchaseCoins(Player player, int amount)
        {
            // This method would interface with the external service to handle coin purchases
            // For now, we'll just add the coins directly
            player.AdjustCoins(amount);
            UIController.UpdateGameState();
        }
    }
}
