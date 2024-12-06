using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Players;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static OcentraAI.LLMGames.Utilities.Utility;
using Random = UnityEngine.Random;


namespace OcentraAI.LLMGames.Manager
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

        [OdinSerialize]
        [ShowInInspector]
        [Required]
        public GameMode GameMode { get; set; }

        #endregion

        #region Unity Lifecycle Methods

        protected override void Start()
        {
            if (IsCancellationRequested())
            {
                LogError("Game start was canceled before it could begin. @ GameManager Start", this);
                return;
            }

            if (GameMode == null)
            {
                LogError("Cannot start the game because GameMode is null. @ GameManager Start", this);
                return;
            }

            // Additional start logic goes here

            Log("Game started successfully. @ GameManager Start", null);
        }


        #endregion

        #region Initialization

        protected override async UniTask InitializeAsync()
        {
            try
            {
                ManagerBase<Component>[] allManagers = FindObjectsByType<ManagerBase<Component>>(FindObjectsSortMode.InstanceID);

                foreach (ManagerBase<Component> manager in allManagers)
                {
                    if (manager == this)
                    {
                        continue;
                    }

                    ManagerBase<Component>.GetInstance();
                }

                List<UniTask> initializationTasks = new List<UniTask>();

                foreach (ManagerBase<Component> manager in allManagers)
                {
                    initializationTasks.Add(manager.WaitForInitializationAsync());
                }

                await UniTask.WhenAll(initializationTasks);
                await base.InitializeAsync();

                Log("All managers initialized successfully.", this);
            }
            catch (Exception ex)
            {
                LogError($"Error during InitializeAsync: {ex.Message}\n{ex.StackTrace}", this);
            }
        }




        private UniTask OnStartMainGame(StartMainGameEvent arg)
        {
            return InitializeGameAsync(arg.AuthPlayerData).AsAsyncUnitUniTask();

        }


        private async UniTask InitializeGameAsync(AuthPlayerData authPlayerData)
        {
            if (IsCancellationRequested())
            {
                return;
            }

            await UniTask.SwitchToMainThread();

            if (!await ExecuteWithTryCatch(() => InitializePlayers(authPlayerData, GameMode),
                    $"{nameof(InitializePlayers)} Failed @ {nameof(InitializeGameAsync)} Method"))
            {
                return;
            }



            if (!await ExecuteWithTryCatch(() => InitializePlayersUI(PlayerManager.GetAllPlayers()),
                    $"{nameof(InitializePlayersUI)} Failed @ {nameof(InitializeGameAsync)} Method"))
            {
                return;
            }

            if (!await ExecuteWithTryCatch(StartNewGameAsync,
                    $"{nameof(StartNewGameAsync)} Failed @ {nameof(InitializeGameAsync)} Method"))
            {
            }
        }





        // todo look on this after multiplayer
        private async UniTask<bool> InitializePlayers(AuthPlayerData playerData, GameMode gameMode)
        {
            if (IsCancellationRequested())
            {
                LogError("Initialization was canceled.", this);
                return false;
            }

            if (playerData == null)
            {
                LogError("playerData is null! Cannot initialize players.", this);
                return false;
            }

            if (gameMode == null)
            {
                LogError("gameMode is null! Cannot initialize players.", this);
                return false;
            }

            try
            {
                AuthPlayerData authPlayerData = new AuthPlayerData
                {
                    PlayerID = Guid.NewGuid().ToString(),
                    PlayerName = nameof(ComputerLLMPlayer)
                };

                PlayerManager.AddPlayer(authPlayerData, PlayerType.Computer, 0, gameMode.InitialPlayerCoins);
                PlayerManager.AddPlayer(playerData, PlayerType.Human, 1, gameMode.InitialPlayerCoins);

                await UniTask.Yield();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in InitializePlayers: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        private async UniTask<bool> InitializePlayersUI(List<LLMPlayer> players)
        {
            if (IsCancellationRequested())
            {
                LogError("Initialization was canceled.", this);
                return false;
            }

            if (players == null)
            {
                LogError("PlayerManager returned null for GetAllPlayers.", this);
                return false;
            }

            try
            {
                UniTaskCompletionSource<bool> initializedUIPlayersSource = new UniTaskCompletionSource<bool>();

                bool eventPublished = await EventBus.Instance.PublishAsync(new InitializeUIPlayersEvent<LLMPlayer>(initializedUIPlayersSource, players));
                if (!eventPublished)
                {
                    LogError("Failed to publish InitializeUIPlayers event.", this);
                    return false;
                }

                return await initializedUIPlayersSource.Task;
            }
            catch (Exception ex)
            {
                LogError($"Error in InitializePlayersUI: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
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

        public override void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<StartMainGameEvent>(OnStartMainGame);
            EventBus.Instance.SubscribeAsync<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGame);
            EventBus.Instance.SubscribeAsync<PlayerActionNewRoundEvent>(OnPlayerActionNewRound);
            EventBus.Instance.SubscribeAsync<PlayerActionEvent<PlayerAction>>(OnPlayerAction);
            EventBus.Instance.SubscribeAsync<PlayerActionRaiseBetEvent>(OnPlayerActionRaiseBet);
            EventBus.Instance.Subscribe<PurchaseCoinsEvent>(OnPurchaseCoins);
        }

        public override void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<StartMainGameEvent>(OnStartMainGame);
            EventBus.Instance.UnsubscribeAsync<PlayerActionStartNewGameEvent>(OnPlayerActionStartNewGame);
            EventBus.Instance.UnsubscribeAsync<PlayerActionNewRoundEvent>(OnPlayerActionNewRound);
            EventBus.Instance.UnsubscribeAsync<PlayerActionEvent<PlayerAction>>(OnPlayerAction);
            EventBus.Instance.UnsubscribeAsync<PlayerActionRaiseBetEvent>(OnPlayerActionRaiseBet);
            EventBus.Instance.Unsubscribe<PurchaseCoinsEvent>(OnPurchaseCoins);
        }



        #endregion

        #region Event Handlers and Utilities

        private async UniTask OnPlayerActionStartNewGame(PlayerActionStartNewGameEvent obj)
        {
            if (IsCancellationRequested())
            {
                LogError("Player action to start a new game was canceled.", this);
                return;
            }

            try
            {
                await StartNewGameAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPlayerActionStartNewGame: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask OnPlayerActionNewRound(PlayerActionNewRoundEvent e)
        {
            if (IsCancellationRequested())
            {
                LogError("Player action to start a new round was canceled.", this);
                return;
            }

            try
            {
                await StartNewRoundAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPlayerActionNewRound: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask OnPlayerAction(PlayerActionEvent<PlayerAction> e)
        {
            if (IsCancellationRequested())
            {
                LogError("Player action was canceled.", this);
                return;
            }

            if (e == null)
            {
                LogError("PlayerActionEvent is null. Cannot process player action.", this);
                return;
            }

            try
            {
                if (TurnManager != null && TurnManager.CurrentLLMPlayer != null &&
                    TurnManager.CurrentLLMPlayer.GetType() == e.CurrentPlayerType)
                {
                    await ProcessPlayerAction(e.Action);
                }
                else
                {
                    LogError("TurnManager or CurrentLLMPlayer is null, or CurrentPlayerType does not match.", this);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPlayerAction: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask OnPlayerActionRaiseBet(PlayerActionRaiseBetEvent e)
        {
            if (IsCancellationRequested())
            {
                LogError("Raise bet action was canceled.", this);
                return;
            }

            if (e == null)
            {
                LogError("PlayerActionRaiseBetEvent event is null. Cannot process raise bet action.", this);
                return;
            }

            try
            {
                if (TurnManager?.CurrentLLMPlayer != null && TurnManager.CurrentLLMPlayer.GetType() == e.CurrentPlayerType)
                {
                    if (string.IsNullOrEmpty(e.Amount))
                    {
                        await ShowMessage($"Please Set RaiseAmount! It needs to be higher than CurrentBet {ScoreManager.CurrentBet}");
                        return;
                    }

                    if (int.TryParse(e.Amount, out int raiseAmount) && raiseAmount > ScoreManager.CurrentBet)
                    {
                        int newBet = raiseAmount;
                        if (raiseAmount <= 0)
                        {
                            float randomMultiplier = Random.Range(0.25f, 3f);
                            newBet = (int)((ScoreManager.CurrentBet * 2) + (ScoreManager.CurrentBet * randomMultiplier));
                        }

                        (bool success, string errorMessage) = ScoreManager.ProcessRaise(newBet, TurnManager, PlayerManager);
                        if (success)
                        {
                            TurnManager.SetLastBettor(ActionCompletionSource);
                            await SwitchTurn();
                        }
                        else
                        {
                            await ShowMessage($"{errorMessage} You need to fold!");
                            await Fold();
                        }
                    }
                    else
                    {
                        await ShowMessage($"RaiseAmount {raiseAmount} needs to be higher than CurrentBet {ScoreManager.CurrentBet}");
                    }
                }
                else
                {
                    LogError("CurrentLLMPlayer is null or does not match the CurrentPlayerType. Raise bet action aborted.", this);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPlayerActionRaiseBet: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


     


        private void OnPurchaseCoins(PurchaseCoinsEvent e)
        {
            if (IsCancellationRequested())
            {
                LogError("Coin purchase action was canceled.", this);
                return;
            }

            if (e == null)
            {
                LogError("PurchaseCoinsEvent event is null. Cannot process coin purchase.", this);
                return;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot process coin purchase.", this);
                return;
            }

            if (TurnManager.CurrentLLMPlayer == null)
            {
                LogError("CurrentLLMPlayer in TurnManager is null. Cannot adjust coins.", this);
                return;
            }

            try
            {
                // This method would interface with the external service to handle coin purchases
                // For now, we'll just add the coins directly
                TurnManager.CurrentLLMPlayer.AdjustCoins(e.Amount);
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPurchaseCoins: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask ShowMessage(string message, bool delay = true, float delayTime = 5f)
        {
            if (IsCancellationRequested())
            {
                return;
            }

            await UniTask.SwitchToMainThread();
            EventBus.Instance.Publish(new UIMessageEvent(message, delayTime));
            if (delay)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delayTime),
                        cancellationToken: GlobalCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Log("ShowMessage delay was cancelled.", this);
                }
            }
        }






        private bool IsCancellationRequested()
        {
            try
            {
                if (GlobalCancellationTokenSource?.IsCancellationRequested ?? false)
                {
                    LogError($"{nameof(InitializeGameAsync)} was cancelled.", this);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error checking cancellation in {nameof(IsCancellationRequested)}: {ex.Message}", this);
                return true; // Return true as a safe fallback if an error occurs
            }
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

        public async UniTask<bool> StartNewGameAsync()
        {
            await UniTask.SwitchToMainThread();

            // Null checks for critical dependencies
            if (PlayerManager == null)
            {
                LogError($"{nameof(PlayerManager)} is null. Cannot start a new game.", this);
                return false;
            }
            if (DeckManager == null)
            {
                LogError($"{nameof(DeckManager)} is null. Cannot start a new game.", this);
                return false;
            }
            if (ScoreManager == null)
            {
                LogError($"{nameof(ScoreManager)} is null. Cannot start a new game.", this);
                return false;
            }
            if (TurnManager == null)
            {
                LogError($"{nameof(TurnManager)} is null. Cannot start a new game.", this);
                return false;
            }


            try
            {
                if (!await ExecuteWithTryCatch(() => PlayerManager.ResetForNewGame(GameMode.InitialPlayerCoins),
                    $"{nameof(PlayerManager)} failed {nameof(PlayerManager.ResetForNewGame)} for a new game"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => DeckManager.ResetForNewGame(GameMode),
                    $"{nameof(DeckManager)} failed {nameof(DeckManager.ResetForNewGame)} for a new game"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => ScoreManager.ResetForNewGame(GameMode, PlayerManager),
                    $"{nameof(ScoreManager)} failed {nameof(ScoreManager.ResetForNewGame)} for a new game"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => TurnManager.ResetForNewGame(PlayerManager,GameMode),
                    $"{nameof(TurnManager)} failed {nameof(TurnManager.ResetForNewGame)} for a new game"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() =>
                        {
                            return EventBus.Instance.PublishAsync(new NewGameEvent<GameManager>(this, "Starting new game", GameMode.InitialPlayerCoins));
                        }, $"EventBus failed to publish {nameof(NewGameEvent<GameManager>)}"))
                {
                    return false;
                }

                return await StartNewRoundAsync();
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error in StartNewGameAsync: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        private async UniTask<bool> StartNewRoundAsync()
        {
            await UniTask.SwitchToMainThread();

            // Null checks for critical dependencies
            if (DeckManager == null)
            {
                LogError($"{nameof(DeckManager)} is null. Cannot start a new round.", this);
                return false;
            }
            if (PlayerManager == null)
            {
                LogError($"{nameof(PlayerManager)} is null. Cannot start a new round.", this);
                return false;
            }
            if (ScoreManager == null)
            {
                LogError($"{nameof(ScoreManager)} is null. Cannot start a new round.", this);
                return false;
            }
            if (TurnManager == null)
            {
                LogError($"{nameof(TurnManager)} is null. Cannot start a new round.", this);
                return false;
            }

            try
            {
                if (!await ExecuteWithTryCatch(() => DeckManager.ResetForNewRound(GameMode),
                        $"{nameof(DeckManager)} failed {nameof(DeckManager.ResetForNewRound)} for a new round"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => PlayerManager.ResetForNewRound(DeckManager),
                        $"{nameof(PlayerManager)} failed {nameof(PlayerManager.ResetForNewRound)} for a new round"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => ScoreManager.ResetForNewRound(GameMode),
                        $"{nameof(ScoreManager)} failed {nameof(ScoreManager.ResetForNewRound)} for a new round"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => TurnManager.ResetForNewRound(ScoreManager, PlayerManager),
                        $"{nameof(TurnManager)} failed {nameof(TurnManager.ResetForNewRound)} for a new round"))
                {
                    return false;
                }

                if (!await ExecuteWithTryCatch(() => EventBus.Instance.PublishAsync(new NewRoundEvent<GameManager>(this)),
                        $"EventBus failed to publish {nameof(NewRoundEvent<GameManager>)}"))
                {
                    return false;
                }

                return await StartFirstTurn();
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error in StartNewRoundAsync: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }


        public UniTaskCompletionSource<bool> TimerCompletionSource { get; set; }

        public CancellationTokenSource TurnCancellationTokenSource { get; set; }

        public UniTaskCompletionSource<bool> ActionCompletionSource { get; set; }

        private async UniTask<bool> StartFirstTurn()
        {
            if (IsCancellationRequested())
            {
                LogError("StartFirstTurn was canceled.", this);
                return false;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot start the first turn.", this);
                return false;
            }

            try
            {

                TurnCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationTokenSource.Token);

                TurnManager.StartTurn(TurnCancellationTokenSource , TimerCompletionSource = new UniTaskCompletionSource<bool>());
                await PlayerTurnAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in StartFirstTurn: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }




        private async UniTask<bool> ExecuteWithTryCatch(Func<UniTask<bool>> action, string errorMessage)
        {
            try
            {
                if (await action())
                {
                    return true;
                }

                LogError(errorMessage, this);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"{errorMessage}: {ex.Message}\n{ex.StackTrace}", this);
                return false;
            }
        }



        private async UniTask PlayerTurnAsync()
        {
            try
            {
                await UniTask.SwitchToMainThread();

                if (TurnManager == null)
                {
                    LogError("TurnManager is null. Cannot proceed with PlayerTurnAsync.", this);
                    return;
                }

                if (TurnManager.CurrentLLMPlayer == null)
                {
                    LogError("CurrentLLMPlayer is null. Cannot proceed with PlayerTurnAsync.", this);
                    return;
                }

                await EventBus.Instance.PublishAsync(new UpdateGameStateEvent());

              
                if (TurnManager.CurrentLLMPlayer is ComputerLLMPlayer computerPlayer)
                {
                   
                    await computerPlayer.MakeDecision(ScoreManager.CurrentBet, GlobalCancellationTokenSource);

                }


                await WaitForPlayerActionAsync();

            }
            catch (Exception ex)
            {
                LogError($"Error in PlayerTurnAsync: {ex.Message}\n{ex.StackTrace}", this);
                
            }
        }


        private async UniTask WaitForPlayerActionAsync()
        {
            if (IsCancellationRequested())
            {
                Log("WaitForPlayerActionAsync was canceled before starting.", this);
                return;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot proceed with WaitForPlayerActionAsync.", this);
                return;
            }

            try
            {
                await UniTask.SwitchToMainThread();

                using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationTokenSource.Token);

                if (ActionCompletionSource?.Task == null || TimerCompletionSource?.Task == null)
                {
                    LogError("ActionCompletionSource or TimerCompletionSource task is null. Cannot wait for player action.", this);
                    return;
                }

                UniTask<bool> actionTask = ActionCompletionSource.Task;
                UniTask<bool> timerTask = TimerCompletionSource.Task;

                (int winArgumentIndex, bool result1, bool result2) completedTask = await UniTask.WhenAny(actionTask, timerTask);

                if (completedTask.result1)
                {
                    await TurnManager.StopTurn(TurnCancellationTokenSource, ActionCompletionSource);
                }
                else if (completedTask.result2)
                {
                    Log("Time's up! Placing automatic bet.", this);
                    await ProcessPlayerAction(PlayerAction.Bet);
                }

                cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
                Log("WaitForPlayerActionAsync was canceled during execution.", this);
            }
            catch (Exception ex)
            {   
                LogError($"Error in WaitForPlayerActionAsync: {ex.Message}\n{ex.StackTrace}", this);
            }
        }



        private async UniTask SwitchTurn()
        {
            await UniTask.SwitchToMainThread();

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot switch turn.", this);
                return;
            }

            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot switch turn.", this);
                return;
            }

            try
            {
                await TurnManager.SwitchTurn(PlayerManager, GlobalCancellationTokenSource,TurnCancellationTokenSource,TimerCompletionSource);
                await PlayerTurnAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error in SwitchTurn: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        #endregion

        #region Player Actions

        private async UniTask ProcessPlayerAction(PlayerAction action)
        {
            await UniTask.SwitchToMainThread();

            string message =
                $"<color={GetColor(Color.white)}>Player : </color> <color={GetColor(Color.blue)}>{TurnManager.CurrentLLMPlayer.AuthPlayerData.PlayerName}</color>" +
                $"{Environment.NewLine}<color={GetColor(Color.white)}>PlayerAction : </color> <color={GetColor(Color.green)}>{action.ToString()}</color>" +
                $"{Environment.NewLine}<color={GetColor(Color.white)}>Current bet : </color> <color={GetColor(Color.yellow)}>{ScoreManager.CurrentBet}</color>" +
                $"{Environment.NewLine}<color={GetColor(Color.white)}>Player coins : </color> <color={GetColor(Color.yellow)}>{TurnManager.CurrentLLMPlayer.Coins}</color>";

            ShowMessage(message, false).Forget();

            switch (action)
            {
                case PlayerAction.SeeHand:
                case PlayerAction.DrawFromDeck:
                    await HandleViewingActions(action);
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

        private async UniTask HandleViewingActions(PlayerAction action)
        {
            await UniTask.SwitchToMainThread();

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot handle viewing actions.", this);
                return;
            }

            if (TurnManager.CurrentLLMPlayer == null)
            {
                LogError("CurrentLLMPlayer in TurnManager is null. Cannot handle viewing actions.", this);
                return;
            }

            try
            {
                switch (action)
                {
                    case PlayerAction.SeeHand:
                        TurnManager.CurrentLLMPlayer.SeeHand();
                        break;
                    case PlayerAction.DrawFromDeck:
                        TurnManager.CurrentLLMPlayer.DrawFromDeck();
                        break;
                    default:
                        LogError($"Unhandled PlayerAction: {action}", this);    
                        return;
                }

                EventBus.Instance.Publish( new UpdateGameStateEvent());
                ActionCompletionSource.TrySetResult(true);
            }
            catch (Exception ex)
            {
                LogError($"Error in HandleViewingActions: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask HandleBettingActions(PlayerAction action)
        {
            await UniTask.SwitchToMainThread();

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot handle betting actions.", this);
                return;
            }

            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot handle betting actions.", this);
                return;
            }

            try
            {
                switch (action)
                {
                    case PlayerAction.PlayBlind:
                        await PlayBlind();
                        break;
                    case PlayerAction.Bet:
                        await Bet();
                        break;
                    default:
                        LogError($"Unhandled PlayerAction: {action}", this);
                        return;
                }

                if (TurnManager.IsRoundComplete(PlayerManager))
                {
                    await SwitchTurn();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in HandleBettingActions: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask PlayBlind()
        {
            if (ScoreManager == null)
            {
                LogError("ScoreManager is null. Cannot process blind bet.", this);
                return;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot process blind bet.", this);
                return;
            }

            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot process blind bet.", this);
                return;
            }

            try
            {
                (bool success, string errorMessage) = ScoreManager.ProcessBlindBet(TurnManager, PlayerManager);

                if (success)
                {
                    TurnManager.SetLastBettor(ActionCompletionSource);
                }
                else
                {
                    ShowMessage($"{errorMessage} You need to fold!").Forget();
                    await Fold();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in PlayBlind: {ex.Message}\n{ex.StackTrace}", null);
            }
        }


        private async UniTask Bet()
        {
            if (ScoreManager == null)
            {
                LogError("ScoreManager is null. Cannot process regular bet.", this);
                return;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot process regular bet.", this);
                return;
            }

            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot process regular bet.", this);
                return;
            }

            try
            {
                (bool success, string errorMessage) = ScoreManager.ProcessRegularBet(TurnManager, PlayerManager);

                if (success)
                {
                    TurnManager.SetLastBettor(ActionCompletionSource);
                }
                else
                {
                    ShowMessage($"{errorMessage} You need to fold!").Forget();
                    await Fold();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in Bet: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask Fold()
        {
            if (ScoreManager == null)
            {
                LogError("ScoreManager is null. Cannot process fold.", this);
                return;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot process fold.", this);
                return;
            }

            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot process fold.", this);
                return;
            }

            try
            {
                await UniTask.SwitchToMainThread();

                await UniTask.RunOnThreadPool(() => ScoreManager.ProcessFold(PlayerManager, TurnManager));
                TurnManager.CallShow(ActionCompletionSource);

                await EndRound(PlayerManager.GetActivePlayers(), false);
            }
            catch (Exception ex)
            {
                LogError($"Error in Fold: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        private async UniTask Show()
        {
            if (ScoreManager == null)
            {
                LogError("ScoreManager is null. Cannot process show bet.", this);
                return;
            }

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot process show bet.", this);
                return;
            }

            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot process show bet.", this);
                return;
            }

            try
            {
                (bool success, string errorMessage) = ScoreManager.ProcessShowBet(TurnManager, PlayerManager);

                if (success)
                {
                    TurnManager.CallShow(ActionCompletionSource);
                    await DetermineWinner();
                }
                else
                {
                    ShowMessage($"{errorMessage} You need to fold!").Forget();
                    await Fold();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in Show: {ex.Message}\n{ex.StackTrace}", this);
            }
        }


        #endregion

        #region Game End and Continuation

        private async UniTask DetermineWinner()
        {
            if (PlayerManager == null)
            {
                LogError("PlayerManager is null. Cannot determine winner.", this);
                return;
            }

            List<LLMPlayer> activePlayers = PlayerManager.GetActivePlayers();
            if (activePlayers == null || activePlayers.Count == 0)
            {
                ShowMessage("No active players found when determining the winner").Forget();
                return;
            }

            // Calculate hand values for each player
            Dictionary<LLMPlayer, int> playerHandValues = new Dictionary<LLMPlayer, int>();
            foreach (LLMPlayer player in activePlayers)
            {
                playerHandValues[player] = player.CalculateHandValue();
            }

            // Find the highest hand value
            int highestHandValue = int.MinValue;
            foreach (var handValue in playerHandValues.Values)
            {
                if (handValue > highestHandValue)
                {
                    highestHandValue = handValue;
                }
            }

            // Identify potential winners with the highest hand value
            List<LLMPlayer> potentialWinners = new List<LLMPlayer>();
            foreach (var player in playerHandValues)
            {
                if (player.Value == highestHandValue)
                {
                    potentialWinners.Add(player.Key);
                }
            }

            if (potentialWinners.Count == 1)
            {
                await EndRound(potentialWinners, true);
                return;
            }

            Dictionary<LLMPlayer, int> potentialWinnersCardValues = new Dictionary<LLMPlayer, int>();
            foreach (LLMPlayer player in potentialWinners)
            {
                potentialWinnersCardValues[player] = player.GetHighestCardValue();
            }

            int highestCardValue = int.MinValue;
            foreach (var cardValue in potentialWinnersCardValues.Values)
            {
                if (cardValue > highestCardValue)
                {
                    highestCardValue = cardValue;
                }
            }

            List<LLMPlayer> winners = new List<LLMPlayer>();
            foreach (var player in potentialWinnersCardValues)
            {
                if (player.Value == highestCardValue)
                {
                    winners.Add(player.Key);
                }
            }

            await EndRound(winners, true);
        }




        private async UniTask EndRound(List<LLMPlayer> winners, bool showHand)
        {
            await UniTask.SwitchToMainThread();

            if (TurnManager == null)
            {
                LogError("TurnManager is null. Cannot end round.", this);
                return;
            }

            await TurnManager.StopTurn(TurnCancellationTokenSource, ActionCompletionSource);

            if (winners == null || winners.Count == 0)
            {
                LogError("EndRound called with no winners.", this);
                return;
            }

            if (winners.Count > 1)
            {
                LLMPlayer winner = await UniTask.RunOnThreadPool(() => BreakTie(winners));
                if (winner != null)
                {
                    await HandleSingleWinner(winner, showHand);
                }
                else
                {
                    await HandleTie(winners, showHand);
                }
            }
            else
            {
                await HandleSingleWinner(winners[0], showHand);
            }
        }


        public LLMPlayer BreakTie(List<LLMPlayer> tiedPlayers)
        {
            if (tiedPlayers == null || tiedPlayers.Count == 0)
            {
                LogError("BreakTie called with no tied players.", this);
                return null;
            }

            // Find the players with the highest card value
            int maxHighCard = int.MinValue;
            List<LLMPlayer> playersWithMaxHighCard = new List<LLMPlayer>();

            foreach (LLMPlayer player in tiedPlayers)
            {
                if (player.Hand == null || player.Hand.Count() == 0)
                    continue;

                int highestCard = player.Hand.Max();

                if (highestCard > maxHighCard)
                {
                    maxHighCard = highestCard;
                    playersWithMaxHighCard.Clear();
                    playersWithMaxHighCard.Add(player);
                }
                else if (highestCard == maxHighCard)
                {
                    playersWithMaxHighCard.Add(player);
                }
            }

            if (playersWithMaxHighCard.Count == 1)
            {
                return playersWithMaxHighCard[0];
            }

            // Find the players with the highest second card value
            int maxSecondHighCard = int.MinValue;
            List<LLMPlayer> playersWithMaxSecondHighCard = new List<LLMPlayer>();

            foreach (LLMPlayer player in playersWithMaxHighCard)
            {
                if (player.Hand == null || player.Hand.Count() < 2)
                    continue;

                int secondHighestCard = player.Hand.OrderByDescending(c => c.Rank.Value).Skip(1).FirstOrDefault().Rank.Value;

                if (secondHighestCard > maxSecondHighCard)
                {
                    maxSecondHighCard = secondHighestCard;
                    playersWithMaxSecondHighCard.Clear();
                    playersWithMaxSecondHighCard.Add(player);
                }
                else if (secondHighestCard == maxSecondHighCard)
                {
                    playersWithMaxSecondHighCard.Add(player);
                }
            }

            if (playersWithMaxSecondHighCard.Count == 1)
            {
                return playersWithMaxSecondHighCard[0];
            }

            // Find the players with the highest lowest card value if still tied
            int maxLowestCard = int.MinValue;
            List<LLMPlayer> winnersWithMaxLowestCard = new List<LLMPlayer>();

            foreach (LLMPlayer player in playersWithMaxSecondHighCard)
            {
                if (player.Hand == null || player.Hand.Count() == 0)
                    continue;

                int lowestCard = player.Hand.Min();

                if (lowestCard > maxLowestCard)
                {
                    maxLowestCard = lowestCard;
                    winnersWithMaxLowestCard.Clear();
                    winnersWithMaxLowestCard.Add(player);
                }
                else if (lowestCard == maxLowestCard)
                {
                    winnersWithMaxLowestCard.Add(player);
                }
            }

            return winnersWithMaxLowestCard.Count == 1 ? winnersWithMaxLowestCard[0] : null;
        }




        private async UniTask HandleTie(List<LLMPlayer> winners, bool showHand)
        {
            if (ScoreManager == null || TurnManager == null || PlayerManager == null)
            {
                LogError("Critical component is null in HandleTie.", this);
                return;
            }

            if (ScoreManager.AwardTiedPot(winners, TurnManager, PlayerManager))
            {
                EventBus.Instance.Publish(new UpdateRoundDisplayEvent<ScoreManager>(ScoreManager));
                EventBus.Instance.Publish(new UpdateGameStateEvent());
                OfferContinuation(showHand);
            }
            else
            {
                LogError("Failed to award tied pot.", this);
            }

            await UniTask.CompletedTask;
        }


        private async UniTask HandleSingleWinner(LLMPlayer winner, bool showHand)
        {
            if (ScoreManager == null || TurnManager == null || PlayerManager == null)
            {
                LogError("Critical component is null in HandleSingleWinner.", this);
                return;
            }

            try
            {
                if (ScoreManager.AwardPotToWinner(winner, TurnManager, PlayerManager))
                {
                    EventBus.Instance.Publish(new UpdateRoundDisplayEvent<ScoreManager>(ScoreManager));
                    EventBus.Instance.Publish(new UpdateGameStateEvent());

                    bool playerWithZeroCoinsFound = false;
                    List<LLMPlayer> activePlayers = PlayerManager.GetActivePlayers();
                    foreach (LLMPlayer player in activePlayers)
                    {
                        if (player.Coins <= 0)
                        {
                            playerWithZeroCoinsFound = true;
                            break;
                        }
                    }

                    if (playerWithZeroCoinsFound)
                    {
                        await EndGame();
                    }
                    else
                    {
                        await CheckForContinuation(showHand);
                    }
                }
                else
                {
                    LogError("Failed to award pot to winner.", this);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in HandleSingleWinner: {ex.Message}\n{ex.StackTrace}", this);
            }
        }



        private async UniTask CheckForContinuation(bool showHand)
        {
            if (TurnManager.IsFixedRoundsOver())
            {
                List<(string PlayerId, int Wins, int TotalWinnings)> leaderboard = ScoreManager.GetLeaderboard();

                // If there's only one player, or the top player has zero winnings, or the top player has more winnings than the second player
                if (leaderboard.Count <= 1 ||
                    (leaderboard.Count > 1 && leaderboard[0].TotalWinnings > leaderboard[1].TotalWinnings &&
                     leaderboard[0].TotalWinnings > 0))
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
            TurnManager.CallShow(ActionCompletionSource);
            PlayerManager.ShowHand(showHand, true);
            EventBus.Instance.Publish(new OfferContinuationEvent(10));
        }

        private UniTask EndGame()
        {
            TurnManager.CallShow(ActionCompletionSource);
            PlayerManager.ShowHand(true);
            EventBus.Instance.Publish(new OfferNewGameEvent(60));
            return UniTask.CompletedTask;
        }

        #endregion
    }
}