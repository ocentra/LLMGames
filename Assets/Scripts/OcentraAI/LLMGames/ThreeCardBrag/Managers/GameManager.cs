using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.Utilities;
using OcentraAI.LLMGames.GameModes;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static OcentraAI.LLMGames.Utility;
using Random = UnityEngine.Random;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{

    public class GameManager : ManagerBase<GameManager>
    {
        #region Fields and Properties



        // Managers
        [Required] private PlayerManager PlayerManager => PlayerManager.Instance;
        [Required] private ScoreManager ScoreManager => ScoreManager.Instance;
        [Required] private DeckManager DeckManager => DeckManager.Instance;
        [Required] private TurnManager TurnManager => TurnManager.Instance;


        public CancellationTokenSource GlobalCancellationTokenSource { get; set; }

        [OdinSerialize,ShowInInspector,Required]
        public GameMode GameMode { get; set; }

        #endregion

        #region Unity Lifecycle Methods

        protected override void Awake()
        {
            base.Awake();

        }



        protected override void Start()
        {
            if (GameMode == null)
            {
                return;
            }

            if (IsCancellationRequested()) return;

            AuthenticationManager.Instance.AuthenticationCompleted += async () =>
            {
                await InitializeGameAsync();
            };
        }

        #endregion

        #region Initialization



        private async Task InitializeGameAsync()
        {
            if (IsCancellationRequested()) return;


            await Task.WhenAll(
                InitializeDeck(),
                InitializePlayers(),

                InitializeUIPlayers()

            );

            await StartNewGameAsync();
        }

        private Task InitializeDeck()
        {
            EventBus.Subscribe<SetFloorCard>(DeckManager.OnSetFloorCard);

            return Task.CompletedTask;
        }

        private Task InitializePlayers()
        {

            PlayerManager.AddPlayer(AuthenticationManager.Instance.PlayerData, PlayerType.Human);

            PlayerData playerData = new PlayerData { PlayerID = Guid.NewGuid().ToString(), PlayerName = nameof(ComputerPlayer) };
            PlayerManager.AddPlayer(playerData, PlayerType.Computer);



            return Task.CompletedTask;
        }





        private Task InitializeUIPlayers()
        {
            TaskCompletionSource<bool> initializedUIPlayersSource = new TaskCompletionSource<bool>();

            EventBus.Publish(new InitializeUIPlayers(initializedUIPlayersSource, this));

            return initializedUIPlayersSource.Task;
        }

        #endregion

        #region Event Subscriptions

        protected override void OnEnable()
        {
            GlobalCancellationTokenSource = new CancellationTokenSource();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

            SubscribeToEvents();
        }

        protected override void OnDisable()
        {
            GlobalCancellationTokenSource?.Cancel();
            GlobalCancellationTokenSource?.Dispose();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<PlayerActionStartNewGame>(OnPlayerActionStartNewGame);
            EventBus.Subscribe<PlayerActionNewRound>(OnPlayerActionNewRound);
            EventBus.Subscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Subscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Subscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);
            EventBus.Subscribe<PurchaseCoins>(OnPurchaseCoins);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<PlayerActionStartNewGame>(OnPlayerActionStartNewGame);
            EventBus.Unsubscribe<PlayerActionNewRound>(OnPlayerActionNewRound);
            EventBus.Unsubscribe<PlayerActionEvent>(OnPlayerAction);
            EventBus.Unsubscribe<PlayerActionRaiseBet>(OnPlayerActionRaiseBet);
            EventBus.Unsubscribe<PlayerActionPickAndSwap>(OnPlayerActionPickAndSwap);
            EventBus.Unsubscribe<PurchaseCoins>(OnPurchaseCoins);
            EventBus.Unsubscribe<SetFloorCard>(DeckManager.OnSetFloorCard);
        }

        #endregion

        #region Event Handlers and Utilities

        private async void OnPlayerActionStartNewGame(PlayerActionStartNewGame obj)
        {
            if (IsCancellationRequested()) return;

            await StartNewGameAsync();
        }

        private async void OnPlayerActionNewRound(PlayerActionNewRound e)
        {
            if (IsCancellationRequested()) return;

            await StartNewRoundAsync();

        }

        private async void OnPlayerAction(PlayerActionEvent e)
        {
            if (IsCancellationRequested()) return;

            if (TurnManager.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                await ProcessPlayerAction(e.Action);
            }
        }

        private async void OnPlayerActionRaiseBet(PlayerActionRaiseBet e)
        {
            if (IsCancellationRequested()) return;

            if (TurnManager.CurrentPlayer.GetType() == e.CurrentPlayerType)
            {
                if (string.IsNullOrEmpty(e.Amount))
                {
                    ShowMessage($"Please Set RaiseAmount! Needs to be higher than CurrentBet {ScoreManager.CurrentBet}");
                    return;
                }

                if (int.TryParse(e.Amount, out int raiseAmount) && raiseAmount > ScoreManager.CurrentBet)
                {
                    int newBet = raiseAmount;
                    if (raiseAmount <= 0) // this will happen from computer atm
                    {
                        float randomMultiplier = Random.Range(0.25f, 3f);
                        // because raise have to be double + if just double it's normal bet!
                        newBet = (int)(ScoreManager.CurrentBet * 2 + ScoreManager.CurrentBet * randomMultiplier);
                    }

                    var (success, errorMessage) = ScoreManager.ProcessRaise(newBet);

                    if (success)
                    {
                        TurnManager.SetLastBettor();

                        await SwitchTurn();
                    }
                    else
                    {
                        ShowMessage($"{errorMessage} You need to fold!");
                        await Fold();
                    }
                }
                else
                {
                    ShowMessage($"RaiseAmount {raiseAmount} Needs to be higher than CurrentBet {ScoreManager.CurrentBet}");
                }
            }
        }

        private void OnPlayerActionPickAndSwap(PlayerActionPickAndSwap e)
        {
            if (IsCancellationRequested()) return;

            if (e.CurrentPlayerType == TurnManager.CurrentPlayer.GetType())
            {
                TurnManager.CurrentPlayer.PickAndSwap(e.FloorCard, e.SwapCard);
            }
        }

        private void OnPurchaseCoins(PurchaseCoins obj)
        {
            // This method would interface with the external service to handle coin purchases
            // For now, we'll just add the coins directly
            PlayerManager.HumanPlayer.AdjustCoins(obj.Amount);
        }

        private async void ShowMessage(string message, bool delay = true, float delayTime = 5f)
        {
            EventBus.Publish(new UIMessage(message, delayTime));
            if (delay)
            {
                await DelayWithCancellation(GlobalCancellationTokenSource, (int)delayTime * 1000);
            }
        }

        private void Log(string message)
        {
            GameLogger.Log($"{nameof(GameManager)} {message}");
        }

        private void LogError(string message)
        {
            GameLogger.LogError($"{nameof(GameManager)}  {message}");
        }

        private bool IsCancellationRequested()
        {
            return GlobalCancellationTokenSource?.IsCancellationRequested ?? false;
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                GlobalCancellationTokenSource.Cancel();
            }
        }
#endif

        #endregion

        #region Game Flow Control

        public async Task StartNewGameAsync()
        {
            PlayerManager.ResetForNewGame();
            DeckManager.ResetForNewGame();
            ScoreManager.ResetForNewGame();
            TurnManager.ResetForNewGame();
            EventBus.Publish(new NewGameEventArgs(this, $"Starting new game"));

            await StartNewRoundAsync();
        }



        private async Task StartNewRoundAsync()
        {
            try
            {
                DeckManager.ResetForNewRound();
                PlayerManager.ResetForNewRound();
                ScoreManager.ResetForNewRound();
                TurnManager.ResetForNewRound();

                Log("Gamemanager Calling TurnManager.ResetForNewRound");

                EventBus.Publish(new NewRoundEventArgs(this));
                // Ensure the first turn is always started correctly
                await StartFirstTurn();


            }
            catch (Exception ex)
            {
                LogError($"Error in StartNewRoundAsync: {ex.Message}");
                // Handle the error appropriately
            }
        }

        private async Task StartFirstTurn()
        {
            try
            {
                TurnManager.StartTurn();
                await PlayerTurnAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error in StartFirstTurn: {ex.Message}");
                // Handle the error appropriately
            }
        }

        private async Task PlayerTurnAsync()
        {
            try
            {
                EventBus.Publish(new UpdateGameState(this));

                if (TurnManager.CurrentPlayer is HumanPlayer)
                {
                    await WaitForPlayerActionAsync();
                }
                else if (TurnManager.CurrentPlayer is ComputerPlayer computerPlayer)
                {
                    await Task.Delay(100);
                    await computerPlayer.MakeDecision(ScoreManager.CurrentBet);
                    await WaitForPlayerActionAsync();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in PlayerTurnAsync: {ex.Message}");
                // Handle the error appropriately
            }
        }

        private async Task WaitForPlayerActionAsync()
        {
            if (IsCancellationRequested()) return;

            try
            {
                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationTokenSource.Token);

                Task completedTask = await Task.WhenAny(TurnManager.ActionCompletionSource.Task, TurnManager.TimerCompletionSource.Task);

                if (completedTask == TurnManager.ActionCompletionSource.Task)
                {
                    await TurnManager.StopTurn();
                }
                else if (completedTask == TurnManager.TimerCompletionSource.Task)
                {
                    Log("Time's up! Placing automatic bet.");
                    await ProcessPlayerAction(PlayerAction.Bet);
                }

                cancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                LogError($"Error in WaitForPlayerActionAsync: {ex.Message}");
                // Handle the error appropriately
            }
        }


        private async Task SwitchTurn()
        {
            await TurnManager.SwitchTurn();
            await PlayerTurnAsync();
        }

        #endregion

        #region Player Actions

        private async Task ProcessPlayerAction(PlayerAction action)
        {
            string message = $"<color={GetColor(Color.white)}>Player : </color> <color={GetColor(Color.blue)}>{TurnManager.CurrentPlayer.PlayerName}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>PlayerAction : </color> <color={GetColor(Color.green)}>{action.ToString()}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>Current bet : </color> <color={GetColor(Color.yellow)}>{ScoreManager.CurrentBet}</color>" +
                             $"{Environment.NewLine}<color={GetColor(Color.white)}>Player coins : </color> <color={GetColor(Color.yellow)}>{TurnManager.CurrentPlayer.Coins}</color>";

            ShowMessage(message, false);

            switch (action)
            {
                case PlayerAction.SeeHand:
                case PlayerAction.DrawFromDeck:
                    HandleViewingActions(action);
                    break;
                case PlayerAction.PlayBlind:
                case PlayerAction.Bet:
                    await HandleBettingActions(action);
                    break;
                case PlayerAction.Fold:
                    await Fold();
                    break;
                case PlayerAction.Show:
                    await Show();
                    break;
            }
        }

        private void HandleViewingActions(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.SeeHand:
                    TurnManager.CurrentPlayer.SeeHand();
                    break;
                case PlayerAction.DrawFromDeck:
                    TurnManager.CurrentPlayer.DrawFromDeck();
                    break;
            }

            EventBus.Publish(new UpdateGameState(this));
            TurnManager.ActionCompletionSource.TrySetResult(true);
        }

        private async Task HandleBettingActions(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.PlayBlind:
                    await PlayBlind();
                    break;
                case PlayerAction.Bet:
                    await Bet();
                    break;
            }

            if (TurnManager.IsRoundComplete())
            {
                await SwitchTurn();
            }

        }

        private async Task PlayBlind()
        {
            var (success, errorMessage) = ScoreManager.ProcessBlindBet();

            if (success)
            {
                TurnManager.SetLastBettor();
            }
            else
            {
                ShowMessage($"{errorMessage} You need to fold!");
                await Fold();
            }
        }

        private async Task Bet()
        {
            var (success, errorMessage) = ScoreManager.ProcessRegularBet();

            if (success)
            {
                TurnManager.SetLastBettor();
            }
            else
            {
                ShowMessage($"{errorMessage} You need to fold!");
                await Fold();
            }
        }

        private async Task Fold()
        {
            ScoreManager.ProcessFold();
            TurnManager.CallShow();

            await EndRound(PlayerManager.GetActivePlayers(), false);
        }

        private async Task Show()
        {
            var (success, errorMessage) = ScoreManager.ProcessShowBet();

            if (success)
            {
                TurnManager.CallShow();
                await DetermineWinner();
            }
            else
            {
                ShowMessage($"{errorMessage} You need to fold!");
                await Fold();
            }
        }

        #endregion

        #region Game End and Continuation

        private async Task DetermineWinner()
        {
            List<Player> activePlayers = PlayerManager.GetActivePlayers();
            if (activePlayers.Count == 0)
            {
                ShowMessage("No active players found when determining winner");
                return;
            }

            Dictionary<Player, int> playerHandValues = new Dictionary<Player, int>();
            foreach (var player in activePlayers)
            {
                playerHandValues[player] = player.CalculateHandValue();
            }

            int highestHandValue = playerHandValues.Values.Max();
            List<Player> potentialWinners = playerHandValues
                .Where(p => p.Value == highestHandValue)
                .Select(p => p.Key)
                .ToList();

            if (potentialWinners.Count == 1)
            {
                await EndRound(potentialWinners, true);
                return;
            }

            Dictionary<Player, int> potentialWinnersCardValues = new Dictionary<Player, int>();
            foreach (var player in potentialWinners)
            {
                potentialWinnersCardValues[player] = player.GetHighestCardValue();
            }

            int highestCardValue = potentialWinnersCardValues.Values.Max();
            List<Player> winners = potentialWinnersCardValues
                .Where(p => p.Value == highestCardValue)
                .Select(p => p.Key)
                .ToList();

            await EndRound(winners, true);
        }


        private async Task EndRound(List<Player> winners, bool showHand)
        {
            await TurnManager.StopTurn();

            if (winners.Count == 0)
            {
                Debug.LogError("EndRound called with no winners");
                return;
            }

            if (winners.Count > 1)
            {
                var winner = BreakTie(winners);
                if (winner != null)
                {
                    await HandleSingleWinner(winner, showHand);
                }
                else
                {
                    HandleTie(winners, showHand);

                }
            }
            else
            {
                await HandleSingleWinner(winners[0], showHand);
            }


        }

        public Player BreakTie(List<Player> tiedPlayers)
        {
            // First, compare the highest cards
            var highestCards = tiedPlayers.Select(p => p.Hand.Max(c => c.GetRankValue())).ToList();
            int maxHighCard = highestCards.Max();
            var playersWithMaxHighCard = tiedPlayers.Where(p => p.Hand.Max(c => c.GetRankValue()) == maxHighCard).ToList();

            if (playersWithMaxHighCard.Count == 1)
            {
                return playersWithMaxHighCard[0];
            }

            // If still tied, compare the second highest cards
            var secondHighestCards = playersWithMaxHighCard.Select(p => p.Hand.OrderByDescending(c => c.GetRankValue()).Skip(1).First().GetRankValue()).ToList();
            int maxSecondHighCard = secondHighestCards.Max();
            var playersWithMaxSecondHighCard = playersWithMaxHighCard.Where(p => p.Hand.OrderByDescending(c => c.GetRankValue()).Skip(1).First().GetRankValue() == maxSecondHighCard).ToList();

            if (playersWithMaxSecondHighCard.Count == 1)
            {
                return playersWithMaxSecondHighCard[0];
            }

            // If still tied, compare the lowest cards
            var lowestCards = playersWithMaxSecondHighCard.Select(p => p.Hand.Min(c => c.GetRankValue())).ToList();
            int maxLowestCard = lowestCards.Max();
            var winnersWithMaxLowestCard = playersWithMaxSecondHighCard.Where(p => p.Hand.Min(c => c.GetRankValue()) == maxLowestCard).ToList();

            // If still tied after comparing all cards, it's a true tie
            if (winnersWithMaxLowestCard.Count > 1)
            {
                return null;
            }

            return winnersWithMaxLowestCard[0];
        }


        private void HandleTie(List<Player> winners, bool showHand)
        {
            if (ScoreManager.AwardTiedPot(winners))
            {
                string winnerNames = string.Join(", ", winners.Select(w => w.PlayerName));
                EventBus.Publish(new UpdateRoundDisplay(ScoreManager));
                EventBus.Publish(new UpdateGameState(this));
                OfferContinuation(showHand);
            }
            else
            {
                Debug.LogError("Failed to award tied pot");
            }
        }

        private async Task HandleSingleWinner(Player winner, bool showHand)
        {
            if (ScoreManager.AwardPotToWinner(winner))
            {
                EventBus.Publish(new UpdateRoundDisplay(ScoreManager));
                EventBus.Publish(new UpdateGameState(this));

                if (PlayerManager.GetActivePlayers().Any(p => p.Coins <= 0))
                {
                    await EndGame();
                }
                else
                {
                    await CheckForContinuation(winner, showHand);
                }
            }
            else
            {
                Debug.LogError("Failed to award pot to winner");
            }
        }

        private async Task CheckForContinuation(Player winner, bool showHand)
        {



            if (TurnManager.IsFixedRoundsOver())
            {
                var leaderboard = ScoreManager.GetLeaderboard();
                if (leaderboard.Count > 1 && leaderboard[0].TotalWinnings > leaderboard[1].TotalWinnings)
                {
                    await EndGame();
                }
                else
                {
                    OfferContinuation(showHand);
                }
            }
            else
            {
                OfferContinuation(showHand);
            }
        }

        private void OfferContinuation(bool showHand)
        {
            TurnManager.CallShow();
            PlayerManager.ShowHand(showHand, true);
            EventBus.Publish(new OfferContinuation(this, 10));

        }

        private Task EndGame()
        {
            TurnManager.CallShow();
            PlayerManager.ShowHand(true);
            EventBus.Publish(new OfferNewGame(this, 60));
            return Task.CompletedTask;
        }

        #endregion
    }
}
