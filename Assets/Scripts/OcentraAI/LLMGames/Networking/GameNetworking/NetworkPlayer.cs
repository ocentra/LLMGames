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
        [ShowInInspector, ReadOnly] public NetworkVariable<int> Coins { get; private set; } = new NetworkVariable<int>();
        [ShowInInspector, ReadOnly] public NetworkVariable<int> LastDecision { get; set; } = new NetworkVariable<int>();
        [ShowInInspector, ReadOnly] public NetworkVariable<bool> HasTakenBettingDecision { get; set; } = new NetworkVariable<bool>();
        [ShowInInspector, ReadOnly] public NetworkVariable<bool> IsBankrupt { get; set; } = new NetworkVariable<bool>();

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

        [ShowInInspector, ReadOnly, DictionaryDrawerSettings]
        public Dictionary<PlayerDecision, Card> WildCards { get; private set; }


        public PlayerDecision PlayerDecision;

        [ShowInInspector, Required] public IEventRegistrar EventRegistrar { get; set; } = new EventRegistrar();

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
            EventRegistrar.Subscribe<PlayerDecisionEvent>(OnPlayerDecision);
            EventRegistrar.Subscribe<UpdateWildCardsEvent<Card>>(OnUpdateWildCards);
        }

        public virtual void UnsubscribeFromEvents()
        {
            EventRegistrar.UnsubscribeAll();
        }
        


        public virtual void SetCoins(int coins)
        {
            if (IsServer)
            {
                Coins.Value += coins;
                UpdateNetworkPlayerClientRpc();
            }
        }

        [ClientRpc]
        private void UpdateNetworkPlayerClientRpc()
        {
            EventBus.Instance.Publish(new UpdateNetworkPlayerUIEvent());
        }

        public int GetCoins()
        {
            return Coins.Value;
        }

        public void SetHasTakenBettingDecision(bool value)
        {
            if (IsServer)
            {
                HasTakenBettingDecision.Value = value;
            }
        }


        public void SetBankrupt(bool value = true)
        {
            if (IsServer)
            {
                IsBankrupt.Value = value;

                UpdateNetworkPlayerClientRpc();
            }
        }

        public void AutoBet()
        {
            PlayerDecisionEvent bettingEvent = new PlayerDecisionBettingEvent(PlayerDecision.Bet);
            EventBus.Instance.Publish(bettingEvent);
        }


        public void SetLastDecision(int value)
        {
            if (IsServer)
            {
                LastDecision.Value = value;
            }
        }
        public virtual void SetPlayerIndex(int index)
        {

            if (IsServer)
            {
                PlayerIndex.Value = index;
            }
        }
        public virtual void RegisterPlayer(IPlayerManager playerManager, string displayName)
        {
            if (IsServer)
            {
                IsPlayerRegistered.Value = true;
            }

            PlayerManager = playerManager;
            gameObject.name = displayName;
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
                        Card drawnCard = networkDeckManager.DrawCard();
                        if (drawnCard != null)  
                        {
                            cards.Add(drawnCard);
                        }
                    }

                  
                    if (cards.Count != GameMode.NumberOfCards)
                    {
                        GameLoggerScriptable.LogError($"Failed to draw correct number of cards. Expected: {GameMode.NumberOfCards}, Got: {cards.Count}", this);
                        return;
                    }

                    Hand = new Hand(cards);
                }

                if (Hand != null && Hand.GetCards()?.Any() == true && Hand.GetCards().All(c => c != null))
                {
                    Hand.OrderByDescending();
                    CheckForWildCardsInHand();
                }
                else
                {
                    GameLoggerScriptable.LogError($"Invalid hand state before ordering. Hand is {(Hand == null ? "null" : "contains null cards")}", this);
                }

                HasBetOnBlind.Value = false;
                HasFolded.Value = false;
                HasSeenHand.Value = false;
                IsPlayerTurn.Value = false;
                HasTakenBettingDecision.Value = false;

            }
        }

        private void CheckForWildCardsInHand()
        {
            if (!IsServer) return;

            if (WildCards == null || Hand == null) return;

            foreach ((PlayerDecision playerDecision, Card wildCard) in WildCards)
            {
                if (Hand.Any(handCard => handCard.Id == wildCard.Id))
                {
                    UpdateWildCardsHighlightClientRpc(playerDecision.DecisionId);
                }
            }
        }

        [ClientRpc]
        private void UpdateWildCardsHighlightClientRpc(int decisionId)
        {
            EventBus.Instance.Publish(new UpdateWildCardsHighlightEvent(decisionId));
        }



        public bool CanAffordBet(int betAmount)
        {
            bool canAffordBet = GetCoins() >= betAmount;
            if (!canAffordBet)
            {
                SetBankrupt(true);
            }
            return canAffordBet;
        }

        public void AdjustCoins(int amount)
        {
            SetCoins(amount);
        }

        public virtual async UniTask<bool> PickAndSwap(Card draggedCard, Card handCard)
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
                await EventBus.Instance.PublishAsync(new UpdatePlayerHandDisplayEvent(this));
                SetFloorCardClientRpc(CardUtility.GetRankSymbol(handCard.Suit, handCard.Rank, false));
                return true;
            }

            return false;
        }

        [ClientRpc]
        private void SetFloorCardClientRpc(string handCard)
        {
            Card card = CardUtility.GetCardFromSymbol(handCard);
            EventBus.Instance.Publish(new SetFloorCardEvent<Card>(card));
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

            if (!IsPlayerTurn.Value)
            {
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