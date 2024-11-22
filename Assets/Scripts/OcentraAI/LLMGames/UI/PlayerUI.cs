using OcentraAI.LLMGames.Commons;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.Players.UI
{
    [ExecuteAlways]
    public class PlayerUI : MonoBehaviour
    {
        [ShowInInspector] private MeshRenderer greenRingRenderer;

        private bool isTimerRunning;
        [SerializeField] private Material originalGreenMaterial;
        [SerializeField] private Material originalRedMaterial;

        [SerializeField]
        [ReadOnly]
        [ShowInInspector]
        public int PlayerIndex;

        [ShowInInspector] private MeshRenderer redRingRenderer;
        [Required][ShowInInspector] private TextMeshPro TurnCountdownText { get; set; }
        [Required][ShowInInspector] private TextMeshPro CoinsText { get; set; }
        [Required][ShowInInspector] public CurvedText PlayerName { get; set; }
        [Required][ShowInInspector] private GameObject RingGreen { get; set; }
        [Required][ShowInInspector] private GameObject RingRed { get; set; }

        [Required][ShowInInspector] private Material RingGreenMaterial { get; set; }
        [Required][ShowInInspector] private Material RingRedMaterial { get; set; }

        [ShowInInspector] private LLMPlayer LLMPlayer { get; set; }

        private float Duration { get; set; }
        [ShowInInspector] private float RemainingTime { get; set; }

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
                        GameObject prefab =
                            PrefabUtility.GetCorrespondingObjectFromOriginalSource(greenRingRenderer.gameObject);


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
            EventBus.Instance.Subscribe<TimerUpdateEvent>(OnTimerUpdate);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TimerUpdateEvent>(OnTimerUpdate);
            Reset();
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

        public void SetPlayer(LLMPlayer llmPlayer)
        {
            if (llmPlayer != null && PlayerName != null)
            {
                LLMPlayer = llmPlayer;
                if (llmPlayer.AuthPlayerData != null)
                {
                    PlayerName.SetPlayerName(llmPlayer.AuthPlayerData.PlayerName);
                }
            }
        }

        public void SetPlayerIndex(int index)
        {
            PlayerIndex = index;
        }

        public void UpdatePlayerCoins(LLMPlayer llmPlayer)
        {
            if (llmPlayer != null && CoinsText != null)
            {
                CoinsText.text = $"{llmPlayer.Coins}";
            }
        }

        public void StartTimer(TurnManager turnManager)
        {
            if (LLMPlayer != turnManager.CurrentLLMPlayer)
            {
                GameLoggerScriptable.Instance.Log($"[UI] PlayerTimer not started. Current player ({turnManager.CurrentLLMPlayer.AuthPlayerData.PlayerName}) doesn't match this timer's player ({LLMPlayer.AuthPlayerData.PlayerName})", this);
                return;
            }

            Duration = turnManager.TurnDuration;
            RemainingTime = turnManager.RemainingTime;
            isTimerRunning = true;
            ShowTimer(true);

        }

        public void StopTimer(LLMPlayer currentLLMPlayer)
        {
            if (!isTimerRunning || LLMPlayer != currentLLMPlayer)
            {
                GameLoggerScriptable.Instance.LogError($"[UI] PlayerTimer cannot be stopped. Current player ({currentLLMPlayer.AuthPlayerData.PlayerName}) doesn't match this timer's player ({LLMPlayer.AuthPlayerData.PlayerName})", this);
                return;
            }

            isTimerRunning = false;
            ShowTimer(false);
        }


        private void OnTimerUpdate(TimerUpdateEvent obj)
        {
            if (!isTimerRunning)
            {
                return;
            }

            RemainingTime = obj.RemainingSeconds;


            if (TurnCountdownText != null)
            {
                TurnCountdownText.text = Mathf.CeilToInt(RemainingTime).ToString();
                GameLoggerScriptable.Instance.Log($"[UI] PlayerTimer text updated: {TurnCountdownText.text}", this);
            }

            if (RingRedMaterial != null)
            {
                float fillAmount = 1 - (RemainingTime / Duration);
                RingRedMaterial.SetFloat("_FillAmount", fillAmount);
                GameLoggerScriptable.Instance.Log($"[UI] PlayerTimer circle fill updated: {fillAmount:F2}", this);
            }
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

            GameLoggerScriptable.Instance.Log($"PlayerTimer visibility set to {show}", this);
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