using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.Scriptable.ScriptableSingletons;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.Scores;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static OcentraAI.LLMGames.Utility;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [RequireComponent(typeof(LLMManager))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [ShowInInspector]
        public HumanPlayer HumanPlayer { get; private set; }

        [ShowInInspector]
        public ComputerPlayer ComputerPlayer { get; private set; }

        [ShowInInspector, ReadOnly]
        public ScoreKeeper ScoreKeeper { get; private set; }

        [ShowInInspector]
        public float TurnDuration { get; private set; } = 60f;

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
        public TurnInfo CurrentTurn { get; private set; }

        public AIHelper AIHelper { get; private set; }

        private CancellationTokenSource globalCancellationTokenSource;


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

        private void OnEnable()
        {
            globalCancellationTokenSource = new CancellationTokenSource();
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            EventBus.Subscribe<PlayerActionStartNewGame>(OnPlayerActionStartNewGame);
            EventBus.Subscribe<PlayerActionContinueGame>(OnPlayerActionContinueGame);

            EventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Subscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Subscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);


            EventBus.Subscribe<PurchaseCoins>(OnPurchaseCoins);

        }

        private void OnDisable()
        {
            globalCancellationTokenSource?.Cancel();
            globalCancellationTokenSource?.Dispose();
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            EventBus.Unsubscribe<PlayerActionStartNewGame>(OnPlayerActionStartNewGame);
            EventBus.Unsubscribe<PlayerActionContinueGame>(OnPlayerActionContinueGame);

            EventBus.Unsubscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Unsubscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Unsubscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);


            EventBus.Unsubscribe<PurchaseCoins>(OnPurchaseCoins);

            EventBus.Unsubscribe<SetFloorCard>(DeckManager.OnSetFloorCard);

        }

        private async void Start()
        {
            await InitializeGameAsync();
        }

        private async void OnPlayerActionStartNewGame(PlayerActionStartNewGame obj)
        {
            await StartNewGameAsync();
        }
        private async void OnPlayerActionContinueGame(PlayerActionContinueGame e)
        {

            if (e.ShouldContinue)
            {
                await StartNewRoundAsync();
            }
            else
            {
                await StartNewGameAsync();
            }
        }

        private async void OnPlayerAction(PlayerActionEvent e)
        {
            if (CurrentTurn.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                await ProcessPlayerAction(e.Action);
            }
        }

        private async void OnPlayerActionRaiseBet(PlayerActionRaiseBet e)
        {

            if (CurrentTurn.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                if (string.IsNullOrEmpty(e.Amount))
                {
                    ShowMessage($" Please Set RaiseAmount! Needs to be higher than CurrentBet {CurrentBet}", 5f);
                    return;
                }

                if (int.TryParse(e.Amount, out int raiseAmount) && raiseAmount > CurrentBet)
                {
                    int newBet = raiseAmount;
                    if (raiseAmount <= 0) // this will happen from computer atm
                    {
                        float randomMultiplier = Random.Range(0.25f, 3f);
                        // because raise have to be double + if just doubble its normal bet!
                        newBet = (int)(CurrentBet * 2 + CurrentBet * randomMultiplier);
                    }



                    if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
                    {
                        SetCurrentBet(newBet);
                        CurrentTurn.CurrentPlayer.Raise(CurrentBet);
                        Pot += CurrentBet;
                        CurrentTurn.ActionCompletionSource.TrySetResult(true);

                    }
                    else
                    {
                        EventBus.Publish(new UIMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f));
                        await Fold();
                    }
                }
                else
                {
                    ShowMessage($" RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {CurrentBet}", 5f);
                }


            }

        }

        private void OnPlayerActionPickAndSwap(PlayerActionPickAndSwap e)
        {
            if (e.CurrentPlayerType == CurrentTurn.CurrentPlayer.GetType())
            {
                CurrentTurn.CurrentPlayer.PickAndSwap(e.FloorCard, e.SwapCard);
                EventBus.Publish(new UpdateGameState(this));

            }
        }
        private void OnPurchaseCoins(PurchaseCoins obj)
        {
            // This method would interface with the external service to handle coin purchases
            // For now, we'll just add the coins directly

            HumanPlayer.AdjustCoins(obj.Amount);
            EventBus.Publish(new UpdateGameState(this));
        }



        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                globalCancellationTokenSource.Cancel();
            }
        }



        private async Task InitializeGameAsync()
        {
            Init();
            await Task.WhenAll(
                InitializePlayers(),
                InitializeDeck(),
                InitializeUIPlayers()
            );
            await StartNewGameAsync();
        }



        private Task InitializeUIPlayers()
        {
            var initializedUIPlayersSource = new TaskCompletionSource<bool>();

            EventBus.Publish(new InitializeUIPlayers(initializedUIPlayersSource, this));

            return initializedUIPlayersSource.Task;

        }


        private void Init()
        {
            ScoreKeeper = new ScoreKeeper();
            AIHelper = new AIHelper(GameInfo.Instance, this);
        }

        private Task InitializePlayers()
        {
            HumanPlayer = new HumanPlayer();
            HumanPlayer.SetName(nameof(HumanPlayer));

            ComputerPlayer = new ComputerPlayer();
            ComputerPlayer.SetName(nameof(ComputerPlayer));

            return Task.CompletedTask;
        }

        private Task InitializeDeck()
        {
            DeckManager = new DeckManager();
            EventBus.Subscribe<SetFloorCard>(DeckManager.OnSetFloorCard);

            return Task.CompletedTask;
        }

        public async Task StartNewGameAsync()
        {

            CurrentRound = 0;
            HumanPlayer.AdjustCoins(InitialCoins);
            ComputerPlayer.AdjustCoins(InitialCoins);
            DeckManager.ResetForNewGame();
            EventBus.Publish(new NewGameEventArgs(InitialCoins));

            await StartNewRoundAsync();

        }

        private async Task StartNewRoundAsync()
        {

            CurrentRound++;
            DeckManager.Reset();
            HumanPlayer.ResetForNewRound();
            ComputerPlayer.ResetForNewRound();
            Pot = 0;
            CurrentBet = BaseBet;
            BlindMultiplier = 1;

            for (int i = 0; i < 3; i++)
            {
                HumanPlayer.Hand.Add(DeckManager.DrawCard());
                ComputerPlayer.Hand.Add(DeckManager.DrawCard());
            }

            DeckManager.SetRandomTrumpCard();

            EventBus.Publish(new UpdateGameState(this, true));

            await PlayerTurnAsync(HumanPlayer);
        }

        private async Task PlayerTurnAsync(Player currentPlayer)
        {
            CurrentTurn = new TurnInfo(currentPlayer, TurnDuration);
            CurrentTurn.StartTurn();

            if (CurrentTurn.CurrentPlayer is HumanPlayer)
            {
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
            else
            {
                await ComputerPlayer.MakeDecision(CurrentBet);
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
        }




        private async Task WaitForPlayerActionAndSwitchTurnAsync()
        {
            using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(globalCancellationTokenSource.Token);


            Task completedTask = await Task.WhenAny(CurrentTurn.ActionCompletionSource.Task, CurrentTurn.TimerCompletionSource.Task);


            if (completedTask == CurrentTurn.ActionCompletionSource.Task)
            {
                CurrentTurn.StopTurn();
            }
            else if (completedTask == CurrentTurn.TimerCompletionSource.Task)
            {
                ShowMessage("Time's up! Placing automatic bet.");
                await ProcessPlayerAction(PlayerAction.Bet);
            }

            cancellationTokenSource.Cancel();
        }




        private async Task ProcessPlayerAction(PlayerAction action)
        {



            string message = $"<color={GetColor(Color.white)}>Player : </color> <color={GetColor(Color.blue)}>{CurrentTurn.CurrentPlayer.PlayerName}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>PlayerAction : </color> <color={GetColor(Color.green)}>{action.ToString()}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>Current bet : </color> <color={GetColor(Color.yellow)}>{CurrentBet}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>Player coins : </color> <color={GetColor(Color.yellow)}>{CurrentTurn.CurrentPlayer.Coins}</color>";


            ShowMessage(message);


            switch (action)
            {
                case PlayerAction.SeeHand:
                    CurrentTurn.CurrentPlayer.SeeHand();
                    EventBus.Publish(new UpdateGameState(this));
                    await CurrentTurn.ActionCompletionSource.Task;
                    break;
                case PlayerAction.PlayBlind:
                    EventBus.Publish(new UpdateGameState(this));

                    await PlayBlind();
                    break;
                case PlayerAction.Bet:
                    EventBus.Publish(new UpdateGameState(this));

                    await Bet();
                    break;
                case PlayerAction.Fold:
                    EventBus.Publish(new UpdateGameState(this));

                    await Fold();
                    break;
                case PlayerAction.DrawFromDeck:
                    EventBus.Publish(new UpdateGameState(this));

                    CurrentTurn.CurrentPlayer.DrawFromDeck();
                    await CurrentTurn.ActionCompletionSource.Task;
                    break;

                case PlayerAction.Show:
                    EventBus.Publish(new UpdateGameState(this));

                    await Show();
                    break;
                default:
                    break;
            }


            CurrentTurn.StopTurn();

            SwitchTurn();

        }



        private async Task PlayBlind()
        {
            CurrentBet *= BlindMultiplier;
            if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
            {
                CurrentTurn.CurrentPlayer.BetOnBlind(CurrentBet);
                Pot += CurrentBet;
                BlindMultiplier *= 2;
                CurrentTurn.ActionCompletionSource.TrySetResult(true);

            }
            else
            {
                ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
                await Fold();
            }
        }

        private async Task Bet()
        {

            int betAmount = CurrentTurn.CurrentPlayer.HasSeenHand ? CurrentBet * 2 : CurrentBet;
            if (CurrentTurn.CurrentPlayer.Coins >= betAmount)
            {
                CurrentTurn.CurrentPlayer.Bet(CurrentBet);
                Pot += betAmount;
                CurrentTurn.ActionCompletionSource.TrySetResult(true);

            }
            else
            {
                ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
                await Fold();
            }
        }



        private async Task Fold()
        {
            CurrentTurn.CurrentPlayer.Fold();
            await EndRound(GetOtherPlayer(CurrentTurn.CurrentPlayer), false);
        }

        private async Task Show()
        {
            HumanPlayer.ShowHand(true);
            ComputerPlayer.ShowHand(true);

            await DetermineWinner();
        }

        private async void SwitchTurn()
        {
            if (globalCancellationTokenSource?.IsCancellationRequested == true) return;
            Player nextPlayer = CurrentTurn.CurrentPlayer is HumanPlayer ? ComputerPlayer : HumanPlayer;

            await PlayerTurnAsync(nextPlayer);
        }

        private Player GetOtherPlayer(Player currentPlayer)
        {
            return currentPlayer == HumanPlayer ? ComputerPlayer : HumanPlayer;
        }

        private async Task DetermineWinner()
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

            await EndRound(winner, true);
        }

        private async Task EndRound(Player winner, bool showHand)
        {
            CurrentTurn.StopTurn();
            if (winner == null)
            {
                HumanPlayer.ShowHand(true);
                ComputerPlayer.ShowHand(true);
                ShowMessage("It's a tie!", 5f);
            }
            else
            {
                winner.AdjustCoins(Pot);
                ScoreKeeper.AddToTotalRoundScores(winner, Pot);
                ShowMessage($"{winner.PlayerName} wins the round with Pot {Pot} coins!", 6f);
                EventBus.Publish(new UpdateRoundDisplay(ScoreKeeper));
                EventBus.Publish(new UpdateGameState(this));
            }

            if (HumanPlayer.Coins <= 0 || ComputerPlayer.Coins <= 0)
            {
                await EndGame(true);
            }
            else
            {
                await CheckForContinuation(showHand);
            }


        }

        private void ShowMessage(string message, float f = 5f)
        {
            EventBus.Publish(new UIMessage(message, f));
        }

        private async Task CheckForContinuation(bool showHand)
        {
            if (CurrentRound >= MaxRounds)
            {
                Player trailingPlayer = ScoreKeeper.HumanTotalWins < ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
                Player leadingPlayer = GetOtherPlayer(trailingPlayer);

                if (trailingPlayer.Coins > leadingPlayer.Coins)
                {
                    CurrentTurn.StopTurn();
                    EventBus.Publish(new OfferContinuation(10));
                    HumanPlayer.ShowHand(showHand);
                    ComputerPlayer.ShowHand(showHand);
                }
                else
                {
                    await EndGame(showHand);
                }
            }
            else
            {
                CurrentTurn.StopTurn();
                EventBus.Publish(new OfferContinuation(10));
                HumanPlayer.ShowHand(showHand);
                ComputerPlayer.ShowHand(showHand);
            }

        }



        private async Task EndGame(bool showHand)
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

            ShowMessage($"Game Over! {winner.PlayerName} wins the game!", 6f);
            HumanPlayer.ShowHand(showHand);
            ComputerPlayer.ShowHand(showHand);

            await Task.Delay(6000, globalCancellationTokenSource.Token);

            EventBus.Publish(new OfferNewGame(15));


        }



        public void SetCurrentBet(int bet)
        {
            CurrentBet = bet;
        }
    }


}
