using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Collections;

#if true
using UnityEditor;
#endif

using UnityEngine.SceneManagement;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [RequireComponent(typeof(PlayerManager))]
    public class GameStartManager : NetworkBehaviour
    {
        [ShowInInspector] PlayerManager PlayerManager { get; set; }
        [SerializeField] private NetworkVariable<FixedString64Bytes> LobbyId { get; set; } = new NetworkVariable<FixedString64Bytes>("");

        [ValueDropdown(nameof(GetScenesFromBuild), DropdownTitle = "Select Scene"), InfoBox("Make sure scene is in Build Settings!")]
        public string SceneToLoad;
        
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

        void Awake()
        {
            if (PlayerManager == null) PlayerManager = GetComponent<PlayerManager>();

            SubscribeToEvents();
            DontDestroyOnLoad(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromEvents();

        }

        protected virtual void OnEnable()
        {
            SubscribeToEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        protected void SubscribeToEvents()
        {
            //Debug.Log($" OnStartGame SubscribeToEvents");
            EventBus.Instance.SubscribeAsync<StartLobbyAsHostEvent>(OnStartGame);

        }



        protected void UnsubscribeFromEvents()
        {
            //Debug.Log($" OnStartGame UnsubscribeFromEvents");
            EventBus.Instance.UnsubscribeAsync<StartLobbyAsHostEvent>(OnStartGame);

        }

        public async UniTask OnStartGame(StartLobbyAsHostEvent e)
        {
            if (PlayerManager == null) PlayerManager = GetComponent<PlayerManager>();
            StartGameServerRpc(e.LobbyId);
            await UniTask.Yield();
        }




        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc(string lobbyId)
        {
            if (IsServer)
            {
                LobbyId.Value = lobbyId;
                NetworkManager.Singleton.SceneManager.LoadScene(SceneToLoad, LoadSceneMode.Single);
            }

        }
        
        public void Init(List<Player> playerList)
        {
            PlayerManager.Init(playerList);
           
        }
    }
}