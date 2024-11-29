using Cysharp.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using static OcentraAI.LLMGames.Networking.Manager.HandleMethods;


namespace OcentraAI.LLMGames.Networking.Manager
{
    public class NetworkBettingProcessManager : NetworkBehaviour, IEventHandler
    {
        public  GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;
        [ShowInInspector] public NetworkGameManager NetworkGameManager { get; set; }
        [ShowInInspector] public NetworkPlayerManager NetworkPlayerManager { get; set; }
        [ShowInInspector] public NetworkTurnManager NetworkTurnManager { get; set; }

        [SerializeField, HideInInspector] private bool logToFile = false;
        [SerializeField, HideInInspector] private bool logStackTrace = true;
        [SerializeField, HideInInspector] private bool toEditor = true;


        [ShowInInspector]
        public bool ToFile
        {
            get => logToFile;
            set => logToFile = value;
        }

        [ShowInInspector]
        public bool UseStackTrace
        {
            get => logStackTrace;
            set => logStackTrace = value;
        }

        [ShowInInspector]
        public bool ToEditor
        {
            get => toEditor;
            set => toEditor = value;
        }

        

        [SerializeField] private GameMode gameMode;

        [ShowInInspector, Required]
        public GameMode GameMode
        {
            get => gameMode;
            set => gameMode = value;
        }

        void OnValidate()
        {
            InitComponents();
        }
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitComponents();
            SubscribeToEvents();
            gameObject.name = $"{nameof(NetworkGameManager)}";
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            UnsubscribeFromEvents();
        }

        private void InitComponents()
        {
            if (NetworkPlayerManager == null)
            {
                NetworkPlayerManager = GetComponent<NetworkPlayerManager>();
            }

            if (NetworkTurnManager == null)
            {
                NetworkTurnManager = GetComponent<NetworkTurnManager>();
            }

            if (NetworkGameManager == null)
            {
                NetworkGameManager = GetComponent<NetworkGameManager>();
            }

            if (NetworkGameManager != null && gameMode == null)
            {
                gameMode = NetworkGameManager.GameMode;
            }



        }



        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();
        }


        public void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<ProcessDecisionEvent>(OnProcessDecisionEvent);
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<ProcessDecisionEvent>(OnProcessDecisionEvent);
        }

        protected async UniTask OnProcessDecisionEvent(ProcessDecisionEvent processDecisionEvent)
        {
            if (!IsServer) return;

            if (NetworkPlayerManager.TryGetPlayer(processDecisionEvent.PlayerId, out IPlayerBase playerBase))
            {
                var playerDecisionEvent = processDecisionEvent.DecisionEvent;
                var decision = PlayerDecision.FromId(playerDecisionEvent.Decision.DecisionId);

                GameLoggerScriptable.Log($" {playerBase.PlayerName.Value.Value} PlayerDecision {decision.Name} Processed ",this,ToEditor,ToFile,UseStackTrace);

                switch (decision.Name)
                {
                    case nameof(PlayerDecision.RaiseBet):
                        HandleRaiseBet(playerDecisionEvent as PlayerDecisionRaiseBetEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.SeeHand):
                        HandleSeeHand(playerDecisionEvent as PlayerDecisionBettingEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.PlayBlind):
                        HandlePlayBlind(playerDecisionEvent as PlayerDecisionBettingEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.Bet):
                        HandleBet(playerDecisionEvent as PlayerDecisionBettingEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.Fold):
                        HandleFold(playerDecisionEvent as PlayerDecisionBettingEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.DrawFromDeck):
                        HandleDrawFromDeck(playerDecisionEvent as PlayerDecisionBettingEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.ShowCall):
                        HandleShowCall(playerDecisionEvent as PlayerDecisionBettingEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.WildCard0):
                        HandleWildCard0(playerDecisionEvent as PlayerDecisionWildcardEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.WildCard1):
                        HandleWildCard1(playerDecisionEvent as PlayerDecisionWildcardEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.WildCard2):
                        HandleWildCard2(playerDecisionEvent as PlayerDecisionWildcardEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.WildCard3):
                        HandleWildCard3(playerDecisionEvent as PlayerDecisionWildcardEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.Trump):
                        HandleTrump(playerDecisionEvent as PlayerDecisionWildcardEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.ShowAllFloorCards):
                        HandleShowAllFloorCards(playerDecisionEvent as PlayerDecisionUIEvent, playerBase, this);
                        break;

                    case nameof(PlayerDecision.PurchaseCoins):
                        HandlePurchaseCoins(playerDecisionEvent as PlayerDecisionUIEvent, playerBase, this);
                        break;

                    default:
                        Debug.LogWarning($"Unhandled decision: {decision.Name}");
                        break;
                }
            }

            await UniTask.Yield();
        }





        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc(string lobbyId)
        {


        }



    }




    public static class HandleMethods
    {
       static GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        public static void HandleRaiseBet(PlayerDecisionRaiseBetEvent raiseBetEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (raiseBetEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for RaiseBet.", context, context.ToEditor, context.ToFile, context.UseStackTrace);
                return;
            }

            float raiseAmount = raiseBetEvent.Amount;
            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} raised the bet by {raiseAmount}", context);
        }

        public static void HandleSeeHand(PlayerDecisionBettingEvent bettingEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (bettingEvent == null)
            {
                context.GameLoggerScriptable.LogWarning("Invalid event type for SeeHand.", context);
                return;
            }

            playerBase.HasSeenHand.Value = true;
            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} chose to see their hand.", context);
        }

        public static void HandlePlayBlind(PlayerDecisionBettingEvent bettingEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for PlayBlind.", context);
                return;
            }

            playerBase.HasSeenHand.Value = false;
            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} chose to play blind.", context);
        }

        public static void HandleBet(PlayerDecisionBettingEvent bettingEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Bet.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} placed a bet.", context);
        }

        public static void HandleFold(PlayerDecisionBettingEvent bettingEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Fold.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} folded.", context);
        }

        public static void HandleDrawFromDeck(PlayerDecisionBettingEvent bettingEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for DrawFromDeck.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} drew a card from the deck.", context);
        }

        public static void HandleShowCall(PlayerDecisionBettingEvent bettingEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (bettingEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for ShowCall.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} called.", context);
        }

        public static void HandleWildCard0(PlayerDecisionWildcardEvent wildcardEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard0.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard0.", context);
        }

        public static void HandleWildCard1(PlayerDecisionWildcardEvent wildcardEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard1.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard1.", context);
        }

        public static void HandleWildCard2(PlayerDecisionWildcardEvent wildcardEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard2.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard2.", context);
        }

        public static void HandleWildCard3(PlayerDecisionWildcardEvent wildcardEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for WildCard3.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played WildCard3.", context);
        }

        public static void HandleTrump(PlayerDecisionWildcardEvent wildcardEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (wildcardEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for Trump.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} played Trump.", context);
        }

        public static void HandleShowAllFloorCards(PlayerDecisionUIEvent uiEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (uiEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for ShowAllFloorCards.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} showed all floor cards.", context);
        }

        public static void HandlePurchaseCoins(PlayerDecisionUIEvent uiEvent, IPlayerBase playerBase, NetworkBettingProcessManager context)
        {
            if (uiEvent == null)
            {
                GameLoggerScriptable.LogWarning("Invalid event type for PurchaseCoins.", context);
                return;
            }

            GameLoggerScriptable.Log($"Player {playerBase.PlayerName.Value.Value} purchased coins.", context);
        }
    }


}