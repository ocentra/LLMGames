using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
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


        public int CalculateHandValue()
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

            return HandValue;
        }

        void Awake()
        {
            GameObject = gameObject;
        }

        void OnValidate()
        {
            GameObject = gameObject;

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





        public virtual void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<PlayerDecisionEvent>(OnPlayerDecision);
        }

        public virtual void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<PlayerDecisionEvent>(OnPlayerDecision);
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





    }
}