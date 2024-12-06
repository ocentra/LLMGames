using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using System.Threading;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace OcentraAI.LLMGames.Players.UI
{
    [ExecuteAlways]
    public class NetworkPlayerUI : MonoBehaviour, IPlayerUI, IEventHandler
    {
        [ShowInInspector] protected IPlayerBase PlayerBase { get; set; }
        [ShowInInspector] protected IPlayerData PlayerData { get; set; }
        [ShowInInspector] protected bool IsLocalPlayer { get; set; }
        [ShowInInspector] private MeshRenderer greenRingRenderer;

        private bool isTimerRunning;
        [SerializeField] private Material originalGreenMaterial;
        [SerializeField] private Material originalRedMaterial;

        [SerializeField, ReadOnly, ShowInInspector]
        private int playerIndex;
        public int PlayerIndex => playerIndex;

        [ShowInInspector] private MeshRenderer redRingRenderer;
        [ShowInInspector] private bool IsPlayerTurn { get; set; }

        [Required, ShowInInspector] private TextMeshPro TurnCountdownText { get; set; }
        [Required, ShowInInspector] private TextMeshPro CoinsText { get; set; }
        [Required, ShowInInspector] public CurvedText PlayerName { get; set; }
        [Required, ShowInInspector] private GameObject RingGreen { get; set; }
        [Required, ShowInInspector] private GameObject RingRed { get; set; }

        [ShowInInspector] private Material RingGreenMaterial { get; set; }
        [ShowInInspector] private Material RingRedMaterial { get; set; }

        [ShowInInspector] private float Duration { get; set; }
        [ShowInInspector] private float RemainingTime { get; set; }
        [ShowInInspector] private float FillAmount { get; set; }

        private void OnValidate()
        {
            Init();
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            RingGreen = transform.RecursiveFindChildGameObject(nameof(RingGreen));
            if (RingGreen != null)
            {
                greenRingRenderer = RingGreen.GetComponent<MeshRenderer>();

                if (greenRingRenderer != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(greenRingRenderer.gameObject);


                        if (prefab != null)
                        {
                            Renderer prefabRenderer = prefab.GetComponent<Renderer>();
                            originalGreenMaterial = prefabRenderer != null
                                ? prefabRenderer.sharedMaterial
                                : greenRingRenderer.sharedMaterial;
                        }
                        else
                        {
                            originalGreenMaterial = greenRingRenderer.sharedMaterial;
                        }

                        EditorUtility.SetDirty(this);
                        EditorUtility.SetDirty(greenRingRenderer.gameObject);
                    }

#endif


                    string materialName = $"{nameof(RingGreenMaterial)}_{gameObject.name}";

                    if (originalGreenMaterial != null)
                    {
                        if (RingGreenMaterial == null || RingGreenMaterial.name != materialName)
                        {
                            RingGreenMaterial = new Material(originalGreenMaterial) { name = materialName };
                        }


                        greenRingRenderer.material = RingGreenMaterial;
                    }
                }
            }

            RingRed = transform.RecursiveFindChildGameObject(nameof(RingRed));
            if (RingRed != null)
            {
                redRingRenderer = RingRed.GetComponent<MeshRenderer>();
                if (redRingRenderer != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        GameObject prefab =
                            PrefabUtility.GetCorrespondingObjectFromOriginalSource(redRingRenderer.gameObject);


                        if (prefab != null)
                        {
                            Renderer prefabRenderer = prefab.GetComponent<Renderer>();
                            originalRedMaterial = prefabRenderer != null
                                ? prefabRenderer.sharedMaterial
                                : redRingRenderer.sharedMaterial;
                        }
                        else
                        {
                            originalRedMaterial = redRingRenderer.sharedMaterial;
                        }

                        EditorUtility.SetDirty(this);
                        EditorUtility.SetDirty(redRingRenderer.gameObject);
                    }

#endif

                    string materialName = $"{nameof(RingRedMaterial)}_{gameObject.name}";
                    if (originalRedMaterial != null)
                    {
                        if (RingRedMaterial == null || RingRedMaterial.name != materialName)
                        {
                            RingRedMaterial = new Material(originalRedMaterial) { name = materialName };
                        }

                        redRingRenderer.material = RingRedMaterial;
                    }
                }
            }

            PlayerName = transform.FindChildRecursively<CurvedText>();
            CoinsText = transform.FindChildRecursively<TextMeshPro>(nameof(CoinsText));
            TurnCountdownText = transform.FindChildRecursively<TextMeshPro>(nameof(TurnCountdownText));

            ShowTimer(false);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            Reset();
        }
        public void SubscribeToEvents()
        {
            EventBus.Instance.SubscribeAsync<TimerStartEvent>(OnTimerStartEvent);
            EventBus.Instance.Subscribe<TimerStopEvent>(OnTimerStopEvent);
            EventBus.Instance.SubscribeAsync<RegisterUIPlayerEvent>(OnRegisterUIPlayerEvent);
            EventBus.Instance.Subscribe<UpdateNetworkPlayerUIEvent>(OnUpdateNetworkPlayerUIEvent);
        }

        public void UnsubscribeFromEvents()
        {
            EventBus.Instance.UnsubscribeAsync<TimerStartEvent>(OnTimerStartEvent);
            EventBus.Instance.Unsubscribe<TimerStopEvent>(OnTimerStopEvent);
            EventBus.Instance.UnsubscribeAsync<RegisterUIPlayerEvent>(OnRegisterUIPlayerEvent);
            EventBus.Instance.Unsubscribe<UpdateNetworkPlayerUIEvent>(OnUpdateNetworkPlayerUIEvent);
        }




        [Button]
        private void Reset()
        {
            if (greenRingRenderer != null && originalGreenMaterial != null)
            {
                greenRingRenderer.material = originalGreenMaterial;
            }

            if (redRingRenderer != null && originalRedMaterial != null)
            {
                redRingRenderer.material = originalRedMaterial;
            }

            RingRedMaterial = null;
            RingGreenMaterial = null;
        }



        public void SetPlayerIndex(int index)
        {
            playerIndex = index;
        }

        public void UpdatePlayerCoins()
        {
            //if (llmPlayer != null && CoinsText != null)
            //{
            //    CoinsText.text = $"{llmPlayer.Coins}";
            //}
        }

        private async UniTask OnTimerStartEvent(TimerStartEvent timerStartEvent)
        {
            IsPlayerTurn = timerStartEvent.PlayerIndex == playerIndex;

            if (IsPlayerTurn)
            {
                ShowTimer(true);
                

                Duration = timerStartEvent.Duration;
                RemainingTime = timerStartEvent.Duration;

                CancellationToken cancellationToken = timerStartEvent.CancellationTokenSource.Token;
                float startTime = Time.realtimeSinceStartup;

                while (RemainingTime > 0 && !cancellationToken.IsCancellationRequested)
                {

                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    RemainingTime = (Mathf.Max(0, Duration - elapsedTime));

                    if (TurnCountdownText != null)
                    {
                        TurnCountdownText.text = Mathf.CeilToInt(RemainingTime).ToString();
                        GameLoggerScriptable.Instance.Log($"[UI] PlayerTimer text updated: {TurnCountdownText.text}", this);
                    }

                    if (RingRedMaterial != null)
                    {
                        FillAmount = 1 - (RemainingTime / Duration);

                        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                        redRingRenderer.GetPropertyBlock(propBlock);
                        propBlock.SetFloat("_FillAmount", FillAmount);
                        redRingRenderer.SetPropertyBlock(propBlock);

                        GameLoggerScriptable.Instance.Log($"[UI] PlayerTimer circle fill updated: {FillAmount:F2}", this);
                    }

                    await UniTask.Delay(1, cancellationToken: cancellationToken);
                }

                if (RemainingTime <= 0 && !timerStartEvent.TimCompletionSource.Task.GetAwaiter().IsCompleted)
                {
                    EventBus.Instance.Publish(new TimerCompletedEvent());
                    timerStartEvent.TimCompletionSource.TrySetResult(true);
                }

            }
            else
            {
                ShowTimer(false);
            }

        }

        private void OnTimerStopEvent(TimerStopEvent timerStopEvent)
        {
            ShowTimer(false);
        }




        public void ShowTimer(bool show)
        {
            if (TurnCountdownText != null)
            {
                TurnCountdownText.enabled = show;
            }

            if (RingRed != null)
            {
                RingRed.SetActive(show);
            }

            if (RingGreenMaterial != null)
            {
                if (show)
                {
                    RingGreenMaterial.EnableKeyword("_EMISSION");
                    RingRedMaterial.EnableKeyword("_EMISSION");
                }
                else
                {
                    RingGreenMaterial.DisableKeyword("_EMISSION");
                    RingRedMaterial.EnableKeyword("_EMISSION");
                }
            }

            if (RingRedMaterial != null)
            {
                RingRedMaterial.SetFloat("_FillAmount", 0);
            }


        }

        private void OnUpdateNetworkPlayerUIEvent(UpdateNetworkPlayerUIEvent updateNetworkPlayerUI)
        {
            int coins = updateNetworkPlayerUI.Coins;
            CoinsText.text = $"{coins}";

        }

        private async UniTask OnRegisterUIPlayerEvent(RegisterUIPlayerEvent arg)
        {
            foreach (IPlayerBase playerBase in arg.Players)
            {

                if (playerBase.PlayerIndex.Value == playerIndex)
                {
                    PlayerBase = playerBase;

                    string[] nameParts = playerBase.PlayerName.Value.Value.Split('_', 2);
                    string formattedName = nameParts.Length > 1 ? nameParts[1] : playerBase.PlayerName.Value.Value;

                    PlayerName.SetPlayerName(formattedName);

                    if (playerBase is IPlayerData playerData)
                    {
                        PlayerData = playerData;
                        IsLocalPlayer = playerData.IsLocalPlayer;
                        if (IsLocalPlayer)
                        {
                            await EventBus.Instance.PublishAsync(new RegisterLocalPlayerEvent(playerData));
                        }
                    }
                }

                playerBase.SetReadyForGame();
            }

            await UniTask.Yield();
        }


        private void OnDestroy()
        {
            Reset();
            if (RingGreenMaterial != null)
            {
                DestroyImmediate(RingGreenMaterial);
            }

            if (RingRedMaterial != null)
            {
                DestroyImmediate(RingRedMaterial);
            }
        }


    }
}