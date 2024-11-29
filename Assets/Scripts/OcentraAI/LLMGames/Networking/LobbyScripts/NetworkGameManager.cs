using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Collections;
using System.Linq;



#if true
using UnityEditor;
#endif

using UnityEngine.SceneManagement;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [RequireComponent(typeof(NetworkPlayerManager), typeof(NetworkTurnManager), typeof(NetworkBettingProcessManager))]
    public class NetworkGameManager : NetworkBehaviour,IEventHandler
    {
        [ShowInInspector] NetworkPlayerManager NetworkPlayerManager { get; set; }
        [ShowInInspector] NetworkTurnManager NetworkTurnManager { get; set; }
        [ShowInInspector] NetworkBettingProcessManager NetworkBettingProcessManager { get; set; }
        [SerializeField] private NetworkVariable<FixedString64Bytes> LobbyId { get; set; } = new NetworkVariable<FixedString64Bytes>("");

        [ValueDropdown(nameof(GetScenesFromBuild), DropdownTitle = "Select Scene"), InfoBox("Make sure scene is in Build Settings!")]
        public string SceneToLoad;

        [SerializeField] private GameMode gameMode;

        [ShowInInspector, Required]
        public GameMode GameMode
        {
            get => gameMode;
            set => gameMode = value;
        }

        private IEnumerable GetScenesFromBuild()
        {
            List<ValueDropdownItem> sceneList = new List<ValueDropdownItem>();

#if UNITY_EDITOR
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene settingsScene in scenes)
            {
                if (settingsScene.enabled)
                {
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(settingsScene.path);
                    string scenePath = settingsScene.path;
                    sceneList.Add(new ValueDropdownItem(sceneName, scenePath));
                }
            }
#endif
            return sceneList;
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
            InitComponents();

            gameObject.name = $"{nameof(NetworkGameManager)}";
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

            if (NetworkBettingProcessManager == null)
            {
                NetworkBettingProcessManager = GetComponent<NetworkBettingProcessManager>();
            }

            if (GameMode == null)
            {
                gameMode = Resources.FindObjectsOfTypeAll<GameMode>().FirstOrDefault();

                if (gameMode == null)
                {
                    Debug.LogError("No GameMode ScriptableObject found. Please assign or create a GameMode.");
                }
                else
                {
                    Debug.Log($"GameMode '{gameMode.name}' assigned automatically.");
                }
            }

        }



        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();

        }

        public void SubscribeToEvents()
        {
            //Debug.Log($" OnStartGame SubscribeToEvents");
            EventBus.Instance.SubscribeAsync<StartLobbyAsHostEvent>(OnStartGame);

        }
        
        public void UnsubscribeFromEvents()
        {
            //Debug.Log($" OnStartGame UnsubscribeFromEvents");
            EventBus.Instance.UnsubscribeAsync<StartLobbyAsHostEvent>(OnStartGame);

        }

        public async UniTask OnStartGame(StartLobbyAsHostEvent e)
        {
            StartGameServerRpc(e.LobbyId);
            await UniTask.Yield();
        }




        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc(string lobbyId)
        {
            if (IsServer)
            {
                LobbyId.Value = lobbyId;

                if (string.IsNullOrEmpty(SceneToLoad))
                {
                    Debug.LogError("SceneToLoad is not set. Ensure a scene is assigned in the inspector.");
                    return;
                }

                NetworkManager.Singleton.SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Single);
            }
        }


        public void Init(List<Player> playerList, List<string> computerPlayerList, float? turnDuration = null, int? maxRounds = null)
        {


            if (GameMode != null)
            {
                turnDuration ??= GameMode.TurnDuration;
                maxRounds ??= GameMode.MaxRounds;
            }

            if (NetworkPlayerManager != null)
            {
                NetworkPlayerManager.Init(playerList, computerPlayerList);
            }

            turnDuration ??= 60f;
            maxRounds ??= 10;

            if (NetworkTurnManager != null)
            {
                NetworkTurnManager.Initialize(turnDuration.Value, maxRounds.Value);
            }
        }

    }
}