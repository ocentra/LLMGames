using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    [RequireComponent(typeof(NetworkPlayerManager), typeof(NetworkTurnManager), typeof(NetworkBettingManager))]
    [RequireComponent(typeof(NetworkDeckManager), typeof(NetworkScoreManager))]
    public class NetworkManagerBase : NetworkBehaviour, IEventHandler
    {
        protected GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;
        [ShowInInspector] protected NetworkPlayerManager NetworkPlayerManager { get; set; }
        [ShowInInspector] protected NetworkTurnManager NetworkTurnManager { get; set; }
        [ShowInInspector] protected NetworkBettingManager NetworkBettingManager { get; set; }
        [ShowInInspector] protected NetworkGameManager NetworkGameManager { get; set; }
        [ShowInInspector] protected NetworkDeckManager NetworkDeckManager { get; set; }
        [ShowInInspector] protected NetworkScoreManager NetworkScoreManager { get; set; }

        [SerializeField, HideInInspector] private bool logToFile = false;
        [SerializeField, HideInInspector] private bool logStackTrace = true;
        [SerializeField, HideInInspector] private bool toEditor = true;


        [ShowInInspector] protected  bool ToFile { get => logToFile; set => logToFile = value; }

        [ShowInInspector] public  bool UseStackTrace { get => logStackTrace; set => logStackTrace = value; }

        [ShowInInspector] public  bool ToEditor { get => toEditor; set => toEditor = value; }


        [SerializeField] private GameMode gameMode;

        [ShowInInspector, Required]
        public GameMode GameMode { get => gameMode; set => gameMode = value; }

        public virtual void OnValidate()
        {
            InitComponents();
        }

        public virtual void Awake()
        {
            DontDestroyOnLoad(this);
        }


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitComponents();
            SubscribeToEvents();

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            UnsubscribeFromEvents();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();

        }

        public virtual void SubscribeToEvents()
        {
        }

        public virtual void UnsubscribeFromEvents()
        {
        }

        public virtual void InitComponents()
        {
            if (NetworkPlayerManager == null)
            {
                NetworkPlayerManager = GetComponent<NetworkPlayerManager>();
            }

            if (NetworkTurnManager == null)
            {
                NetworkTurnManager = GetComponent<NetworkTurnManager>();
            }

            if (NetworkBettingManager == null)
            {
                NetworkBettingManager = GetComponent<NetworkBettingManager>();
            }

            if (NetworkGameManager == null)
            {
                NetworkGameManager = GetComponent<NetworkGameManager>();
            }

            if (NetworkDeckManager == null)
            {
                NetworkDeckManager = GetComponent<NetworkDeckManager>();
            }

            if (NetworkScoreManager == null)
            {
                NetworkScoreManager = GetComponent<NetworkScoreManager>();
            }

            if (GameMode == null)
            {
                gameMode = Resources.FindObjectsOfTypeAll<GameMode>().FirstOrDefault();

                if (gameMode == null)
                {
                    GameLoggerScriptable.LogError("No GameMode ScriptableObject found. Please assign or create a GameMode.", this, true);
                }
                else
                {
                    GameLoggerScriptable.Log($"GameMode '{gameMode.name}' assigned automatically.", this);
                }
            }

        }
    }
}