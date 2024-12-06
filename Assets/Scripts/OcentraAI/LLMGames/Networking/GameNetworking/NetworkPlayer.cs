using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Networking.Manager;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

namespace OcentraAI.LLMGames.GamesNetworking
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerBase
    {
        protected GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;

        [ShowInInspector, ReadOnly] public NetworkVariable<int> PlayerIndex { get; private set; } = new NetworkVariable<int>();
        [ShowInInspector, ReadOnly] public NetworkVariable<bool> HasSeenHand { get; set; } = new NetworkVariable<bool>();
        [ShowInInspector, ReadOnly] public NetworkVariable<bool> HasBetOnBlind { get; set; } = new NetworkVariable<bool>();
        [ShowInInspector, ReadOnly] public NetworkVariable<bool> HasFolded { get; set; } = new NetworkVariable<bool>();
        [ShowInInspector][ReadOnly] public NetworkVariable<int> Coins { get; private set; } = new NetworkVariable<int>();

        public GameObject GameObject { get; set; }

        [ShowInInspector, ReadOnly]
        public NetworkVariable<FixedString64Bytes> AuthenticatedPlayerId { get; private set; } = new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        [ShowInInspector] public IPlayerManager PlayerManager { get; set; }
        [ShowInInspector]
        public NetworkVariable<ulong> PlayerId { get; set; } = new NetworkVariable<ulong>(ulong.MaxValue);

        [ShowInInspector, ReadOnly] public NetworkVariable<FixedString64Bytes> PlayerName { get; private set; } = new NetworkVariable<FixedString64Bytes>();

        [ShowInInspector] public NetworkVariable<bool> IsPlayerRegistered { get; set; } = new NetworkVariable<bool>();
        [ShowInInspector] public NetworkVariable<bool> ReadyForNewGame { get; set; } = new NetworkVariable<bool>();
        [ShowInInspector] public NetworkVariable<bool> IsPlayerTurn { get; set; } = new NetworkVariable<bool>();

        [ShowInInspector] private GameMode GameMode { get; set; }

        [ShowInInspector, ReadOnly]
        public List<BaseBonusRule> AppliedRules { get; private set; } = new List<BaseBonusRule>();
        [ShowInInspector, ReadOnly] public List<BonusDetail> BonusDetails { get; private set; }

        [ShowInInspector, ReadOnly] public int HandRankSum { get; private set; }
        [ShowInInspector, ReadOnly] public int HandValue { get; private set; }
        [ShowInInspector, ReadOnly] public Hand Hand { get; private set; }

        protected Card FloorCard { get; set; }

        [ShowInInspector, ReadOnly,DictionaryDrawerSettings]
        public Dictionary<PlayerDecision, Card> WildCards { get; private set; }
        

        public PlayerDecision PlayerDecision;

        public override async void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                PlayerId.Value = OwnerClientId;
            }
            else
            {
                await UniTask.WaitUntil(() => PlayerId.Value != ulong.MaxValue);
            }

            int maxFrames = 500;
            while (PlayerManager == null && maxFrames > 0)
            {
                GameLoggerScriptable.Log($"Stuck in setting AuthenticatedPlayerId.Value For {PlayerId.Value}. Frames left: {maxFrames}", this);

                foreach (NetworkObject networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
                {
                    IPlayerManager playerManager = networkObject.GetComponent<IPlayerManager>();
                    if (playerManager != null)
                    {
                        PlayerManager = playerManager;
                        break;
                    }
                }

                await UniTask.DelayFrame(10);
                maxFrames--;
            }

            if (PlayerManager == null)
            {
                GameLoggerScriptable.LogError("Failed to find PlayerManager within timeout.", this);
            }
            else
            {
                ScriptableObject scriptableObject = PlayerManager.GetGameMode();

                if (scriptableObject is GameMode gameMode)
                {
                    GameMode = gameMode;
                }
            }



            SubscribeToEvents();
        }


        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            GameLoggerScriptable.Log($"Player : {PlayerName.Value.Value} Index: {PlayerIndex.Value} despawned", this);
            UnsubscribeFromEvents();
        }

        public virtual void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<PlayerDecisionEvent>(OnPlayerDecision);
            EventBus.Instance.Subscribe<UpdateWildCardsEvent<Card>>(OnUpdateWildCards);
        }

        public virtual void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<PlayerDecisionEvent>(OnPlayerDecision);
            EventBus.Instance.Unsubscribe<UpdateWildCardsEvent<Card>>(OnUpdateWildCards);
        }


        void Awake()
        {
            GameObject = gameObject;
        }

        void OnValidate()
        {
            GameObject = gameObject;

        }


        public virtual void SetCoins(int coins)
        {
            if (IsServer)
            {
                Coins.Value += coins;

                EventBus.Instance.Publish(new UpdateNetworkPlayerUIEvent(coins));
            }
        }

        public int GetCoins()
        {
            return Coins.Value;
        }

        public virtual void SetPlayerIndex(int index)
        {

            if (IsServer)
            {
                PlayerIndex.Value = index;
            }
        }
        public virtual void SetPlayerRegistered(bool value = true)
        {
            if (IsServer)
            {
                IsPlayerRegistered.Value = value;
            }
        }

        public virtual void SetReadyForGame(bool value = true)
        {
            if (IsServer)
            {
                ReadyForNewGame.Value = value;
            }
        }

        public virtual void SetIsPlayerTurn(bool value = true)
        {
            if (IsServer)
            {
                IsPlayerTurn.Value = value;
            }
        }

        public virtual void ResetForNewRound(NetworkDeckManager networkDeckManager, Hand customHand = null)
        {
            if (IsServer)
            {
                AppliedRules = new List<BaseBonusRule>();
                HandRankSum = 0;

                // this is temp solution for quick testing on dev mode 
                if (customHand != null)
                {
                    Hand = customHand;
                    networkDeckManager.RemoveCardsFromDeck(customHand.GetCards().ToList());
                    HasSeenHand.Value = true;
                }
                else
                {
                    List<Card> cards = new List<Card>();
                    for (int i = 0; i < GameMode.NumberOfCards; i++)
                    {
                        cards.Add(networkDeckManager.DrawCard());
                    }

                    Hand = new Hand(cards);
                }

                Hand.OrderByDescending();
                CheckForWildCardsInHand();
                HasBetOnBlind.Value = false;
                HasFolded.Value = false;
                HasSeenHand.Value = false;
            }

        }

        private void CheckForWildCardsInHand()
        {
            if (IsServer)
            {

                if (WildCards != null)
                {
                    foreach ((PlayerDecision playerDecision, Card card) in WildCards)
                    {
                        if (Hand != null && Hand.Any(card.Id))
                        {
                            EventBus.Instance.Publish(new UpdateWildCardsHighlightEvent(playerDecision.DecisionId));
                        }
                    }
                }

               
            }


        }


        public bool CanAffordBet(int betAmount)
        {
            return GetCoins() >= betAmount;
        }

        public void AdjustCoins(int amount)
        {
            SetCoins(amount);
        }

        public virtual void PickAndSwap(Card draggedCard, Card handCard)
        {
            if (draggedCard != null && handCard != null)
            {
                for (int index = 0; index < Hand.Count(); index++)
                {
                    Card cardInHand = Hand.GetCard(index);
                    if (cardInHand.Suit == handCard.Suit && cardInHand.Rank == handCard.Rank)
                    {
                        Hand.ReplaceCard(index, draggedCard);
                        break;
                    }
                }

                CheckForWildCardsInHand();
                EventBus.Instance.Publish(new UpdatePlayerHandDisplayEvent(this));
                EventBus.Instance.Publish(new SetFloorCardEvent<Card>(handCard));
            }
        }



        public int CalculateHandValue()
        {
            if (IsServer)
            {
                AppliedRules = new List<BaseBonusRule>();
                BonusDetails = new List<BonusDetail>();
                HandRankSum = Hand.Sum();
                HandValue = HandRankSum;
                List<BaseBonusRule> bonusRules = GameMode.BonusRules;
                foreach (BaseBonusRule rule in bonusRules)
                {
                    if (rule.Evaluate(Hand, out BonusDetail bonusDetails))
                    {
                        HandValue += bonusDetails.TotalBonus;

                        if (!AppliedRules.Contains(rule))
                        {
                            AppliedRules.Add(rule);
                        }

                        if (!BonusDetails.Contains(bonusDetails))
                        {
                            BonusDetails.Add(bonusDetails);
                        }
                    }
                }
            }

            return HandValue;
        }

        private void OnUpdateWildCards(UpdateWildCardsEvent<Card> updateWildCardsEvent)
        {
            WildCards = updateWildCardsEvent.WildCards;
            CheckForWildCardsInHand();
        }


        public virtual async UniTask OnPlayerDecision(PlayerDecisionEvent decisionEvent)
        {
            if (!IsLocalPlayer) return;

            if (!IsPlayerTurn.Value)
            {
                Debug.Log($" {PlayerName.Value.Value} Is not PlayerTurn!!! ");
                return;
            }

            if (!IsServer)
            {
                ProcessDecisionServerRpc(decisionEvent, PlayerId.Value);
            }
            else
            {
                await EventBus.Instance.PublishAsync(new ProcessDecisionEvent(decisionEvent, PlayerId.Value));
            }

            await UniTask.Yield();
        }


        [ServerRpc(RequireOwnership = false)]
        protected virtual void ProcessDecisionServerRpc(PlayerDecisionEvent decisionEvent, ulong playerId)
        {
            EventBus.Instance.Publish(new ProcessDecisionEvent(decisionEvent, playerId));
        }

        public string GetCard(int cardIndex)
        {
            Card card = Hand.GetCard(cardIndex);
            if (card == null) return "";
            return CardUtility.GetRankSymbol(card.Suit, card.Rank, coloured: false);
        }



    }
}