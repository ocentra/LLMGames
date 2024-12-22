using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GamesNetworking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Collections;
using System;

#if true
using UnityEditor;
#endif

using UnityEngine.SceneManagement;

namespace OcentraAI.LLMGames.Networking.Manager
{

    public class NetworkGameManager : NetworkManagerBase
    {

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



        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitComponents();
            gameObject.name = $"{nameof(NetworkGameManager)}";
        }



        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            EventRegistrar.Subscribe<StartLobbyAsHostEvent>(OnStartGame);
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
                    GameLoggerScriptable.LogError("SceneToLoad is not set. Ensure a scene is assigned in the inspector.", this, ToEditor, ToFile, UseStackTrace);
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


        private void OnApplicationQuit()
        {
            EventBus.Instance.Clear();
        }


    }
}