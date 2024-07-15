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

            EventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Subscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Subscribe<StartNewGame>(OnStartNewGame);
            EventBus.Subscribe<ContinueGame>(OnContinueGame);
            EventBus.Subscribe<PurchaseCoins>(OnPurchaseCoins);
            EventBus.Subscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);

        }




        private void OnDisable()
        {
            globalCancellationTokenSource.Cancel();
            globalCancellationTokenSource.Dispose();
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            EventBus.Unsubscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Unsubscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Unsubscribe<StartNewGame>(OnStartNewGame);
            EventBus.Unsubscribe<ContinueGame>(OnContinueGame);
            EventBus.Unsubscribe<PurchaseCoins>(OnPurchaseCoins);
            EventBus.Unsubscribe<SetFloorCard>(DeckManager.OnSetFloorCard);
            EventBus.Unsubscribe<AddToFloorCardList>(DeckManager.OnSetFloorCardList);
            EventBus.Unsubscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);

        }


        private void OnPurchaseCoins(PurchaseCoins obj)
        {
            // This method would interface with the external service to handle coin purchases
            // For now, we'll just add the coins directly

            HumanPlayer.AdjustCoins(obj.Amount);
            EventBus.Publish(new UpdateGameState(this));
        }

        private async void OnContinueGame(ContinueGame e)
        {
            await ContinueGame(e.ShouldContinue);
        }

        private async void OnStartNewGame(StartNewGame obj)
        {
            await StartNewGameAsync();
        }


        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                globalCancellationTokenSource.Cancel();
            }
        }

        private async void Start()
        {
            await InitializeGameAsync();
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

            EventBus.Publish(new InitializeUIPlayers(initializedUIPlayersSource,this));

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
            EventBus.Subscribe<AddToFloorCardList>(DeckManager.OnSetFloorCardList);

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

            EventBus.Publish(new UpdateGameState(this,true));
            await PlayerTurnAsync(HumanPlayer);
        }

        private async Task PlayerTurnAsync(Player currentPlayer)
        {
            CurrentTurn = new TurnInfo(currentPlayer);

            if (CurrentTurn.CurrentPlayer is HumanPlayer)
            {
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
            else
            {
                await ComputerTurnAsync();
                await WaitForPlayerActionAndSwitchTurnAsync();
            }
        }


        private async Task WaitForPlayerActionAndSwitchTurnAsync()
        {
            using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(globalCancellationTokenSource.Token))
            {
                var actionCompletionSource = new TaskCompletionSource<bool>();
                var timerCompletionSource = new TaskCompletionSource<bool>();

                EventBus.Publish(new PlayerStartCountDown(CurrentTurn.CurrentPlayer, actionCompletionSource: actionCompletionSource, timerCompletionSource: timerCompletionSource, TurnDuration));

                Task completedTask = await Task.WhenAny(actionCompletionSource.Task, timerCompletionSource.Task);

                if (completedTask == actionCompletionSource.Task)
                {
                    timerCompletionSource.TrySetCanceled();
                    EventBus.Publish(new TimerStopEventArgs(CurrentTurn.CurrentPlayer, true));
                    await ProcessPlayerAction(PlayerAction.Bet);
                    
                }
                else if (completedTask == timerCompletionSource.Task)
                {
                    EventBus.Publish(new TimerStopEventArgs(CurrentTurn.CurrentPlayer, false));
                    EventBus.Publish(new UIMessage("Time's up! Placing automatic bet.", 5f));
                    await ProcessPlayerAction(PlayerAction.Bet);
                }

                cts.Cancel();
            }
        }



        private async Task ComputerTurnAsync()
        {
            ComputerPlayer.MakeDecision(CurrentBet);
            await Task.Delay(TimeSpan.FromSeconds(Random.Range(1f, 3f)), globalCancellationTokenSource.Token);
        }

        private async void OnPlayerAction(PlayerActionEvent e)
        {
            if (CurrentTurn.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                Debug.Log($"HandlePlayerAction: {e.Action} by {CurrentTurn.CurrentPlayer.PlayerName}");
                await ProcessPlayerAction(e.Action);
                EventBus.Publish(new UpdateGameState(this));

                if (e.Action != PlayerAction.Fold && e.Action != PlayerAction.Show)
                {
                    Debug.Log($"Switching turn to: {CurrentTurn.CurrentPlayer.PlayerName}");
                }

            }

        }



        private async Task ProcessPlayerAction(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.SeeHand:
                    CurrentTurn.CurrentPlayer.SeeHand();
                    EventBus.Publish(new UpdateGameState(this));
                    await WaitForTurnCompletion();
                    break;
                case PlayerAction.PlayBlind:
                    PlayBlind();
                    break;
                case PlayerAction.Bet:
                    Bet();
                    break;
                case PlayerAction.Fold:
                    Fold();
                    break;
                case PlayerAction.DrawFromDeck:
                    CurrentTurn.CurrentPlayer.DrawFromDeck();
                    await WaitForTurnCompletion();
                    break;

                case PlayerAction.Show:
                    Show();
                    break;
            }

        }

        private void OnPlayerActionPickAndSwap(PlayerActionPickAndSwap e)
        {
            if (e.CurrentPlayerType == CurrentTurn.CurrentPlayer.GetType())
            {
                CurrentTurn.CurrentPlayer.PickAndSwap(e.PickCard, e.SwapCard);

            }
        }

        public async Task WaitForTurnCompletion()
        {
            await CurrentTurn.TurnCompletionSource.Task;
        }

        private void PlayBlind()
        {
            CurrentBet *= BlindMultiplier;
            if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
            {
                CurrentTurn.CurrentPlayer.BetOnBlind(CurrentBet);
                Pot += CurrentBet;
                BlindMultiplier *= 2;
            }
            else
            {
                ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
            }
        }

        private void Bet()
        {
            ShowMessage($"Betting: Player {CurrentTurn.CurrentPlayer.PlayerName}, Current bet: {CurrentBet}, Player coins: {CurrentTurn.CurrentPlayer.Coins}");

            int betAmount = CurrentTurn.CurrentPlayer.HasSeenHand ? CurrentBet * 2 : CurrentBet;
            if (CurrentTurn.CurrentPlayer.Coins >= betAmount)
            {
                CurrentTurn.CurrentPlayer.Bet(CurrentBet);
                Pot += betAmount;
                EventBus.Publish(new TimerStopEventArgs(CurrentTurn.CurrentPlayer, true));

                SwitchTurn();
            }
            else
            {
                ShowMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f);
                Fold();
            }
        }




        private void OnPlayerActionRaiseBet(PlayerActionRaiseBet e)
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
                    
                    SetCurrentBet(newBet);

                    if (CurrentTurn.CurrentPlayer.Coins >= CurrentBet)
                    {

                        CurrentTurn.CurrentPlayer.Raise(CurrentBet);
                        Pot += CurrentBet;
                    }
                    else
                    {
                        EventBus.Publish(new UIMessage($"Not enough coins ({CurrentTurn.CurrentPlayer.Coins}). Current bet is {CurrentBet}. You need to fold!", 5f));

                    }
                }
                else
                {
                    ShowMessage($" RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {CurrentBet}", 5f);
                }


            }

        }



        private void Fold()
        {
            CurrentTurn.CurrentPlayer.Fold();
            EndRound(GetOtherPlayer(CurrentTurn.CurrentPlayer));
        }

        private void Show()
        {
            HumanPlayer.ShowHand(true);
            ComputerPlayer.ShowHand(true);
            DetermineWinner();
        }

        private async void SwitchTurn()
        {
            Player nextPlayer = CurrentTurn.CurrentPlayer is HumanPlayer ? ComputerPlayer : HumanPlayer;
            await PlayerTurnAsync(nextPlayer);
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
            EventBus.Publish(new StopTurnCountdown());
            if (winner == null)
            {

                ShowMessage("It's a tie! Play another round!", 5f);
            }
            else
            {
                winner.AdjustCoins(Pot);
                ScoreKeeper.AddToTotalRoundScores(winner, Pot);
                ShowMessage($"{winner.PlayerName} wins the round and {Pot} coins!", 6f);
                await Task.Delay(6000, globalCancellationTokenSource.Token);
                EventBus.Publish(new UpdateRoundDisplay(ScoreKeeper));
                EventBus.Publish(new UpdateGameState(this));
            }

            if (HumanPlayer.Coins <= 0 || ComputerPlayer.Coins <= 0)
            {
                await EndGame();
            }
            else
            {
                CheckForContinuation();
            }

            Pot = 0;
            EventBus.Publish(new UpdateGameState(this));
        }

        private void ShowMessage(string message, float f = 5f)
        {
            EventBus.Publish(new UIMessage(message, f));
        }

        private async void CheckForContinuation()
        {
            if (CurrentRound >= MaxRounds)
            {
                Player trailingPlayer = ScoreKeeper.HumanTotalWins < ScoreKeeper.ComputerTotalWins ? HumanPlayer : ComputerPlayer;
                Player leadingPlayer = GetOtherPlayer(trailingPlayer);

                if (trailingPlayer.Coins > leadingPlayer.Coins)
                {
                    EventBus.Publish(new StopTurnCountdown());
                    EventBus.Publish(new OfferContinuation(10));
                    await Task.Delay(10000, globalCancellationTokenSource.Token);
                }
                else
                {
                    await EndGame();
                }
            }
            else
            {
                await StartNewRoundAsync();
            }
        }

        public async Task ContinueGame(bool playerWantsToContinue)
        {
            if (playerWantsToContinue)
            {
                await StartNewRoundAsync();
            }
            else
            {
                await EndGame();
            }
        }

        private async Task EndGame()
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


            await Task.Delay(6000, globalCancellationTokenSource.Token);

            EventBus.Publish(new OfferNewGame(15));


        }



        public void SetCurrentBet(int bet)
        {
            CurrentBet = bet;
        }
    }


}
