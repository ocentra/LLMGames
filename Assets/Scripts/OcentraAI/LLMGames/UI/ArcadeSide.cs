using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class ArcadeSide : MonoBehaviourBase<ArcadeSide>
    {
        [SerializeField, ShowInInspector, OnValueChanged(nameof(OnValueChanged))] protected bool Visible { get; set; }
        [SerializeField, ShowInInspector, ReadOnly] protected Vector3 OriginalPosition { get; set; }
        [SerializeField, ShowInInspector, ReadOnly] protected Vector3 ShowPosition { get; set; }

        [ShowInInspector, OnValueChanged(nameof(OnValueChanged))] protected float AnimateDuration = 0.5f;
        [ShowInInspector] private float RemainingTime { get; set; }


        [ShowInInspector] protected TextMeshProUGUI InfoText { get; set; }
        [ShowInInspector] protected RectTransform InfoTextPanel { get; set; }

        [ShowInInspector] protected GameObject InfoTab { get; set; }
        [ShowInInspector] protected GameObject FriendsTab { get; set; }
        [ShowInInspector] protected ScrollRect InfoScrollRect { get; set; }
        [ShowInInspector] private RectTransform InfoScrollRectTransform { get; set; }
        [SerializeField, ShowInInspector] protected RectVector InfoScrollRectExpanded { get; set; }
        [SerializeField, ShowInInspector] protected RectVector InfoScrollCollapsed { get; set; }

        [SerializeField, ShowInInspector, ReadOnly] protected Vector3 FriendsTabHiddenPosition { get; set; }
        [SerializeField, ShowInInspector, ReadOnly] protected Vector3 FriendsTabVisiblePosition { get; set; }
        [SerializeField, ShowInInspector, OnValueChanged(nameof(OnValueChanged))] protected bool FriendsTabVisible { get; set; }
        [SerializeField, ShowInInspector, OnValueChanged(nameof(OnValueChanged))] protected bool LobbyInfoUIVisible { get; set; }
        [SerializeField, ShowInInspector] protected List<Mask3DHandler> Mask3DHandlers { get; set; }

        [SerializeField, TextArea(5, 15), RichText, PropertyOrder(-1)]
        protected string Info;

        [SerializeField, PropertyOrder(-1)]
        protected LobbyInfoUI LobbyInfoUI;


        protected override void OnValidate()
        {
            Init();
            base.OnValidate();
        }

        protected override void Awake()
        {
            Init();
            base.Awake();
        }

        protected override void Start()
        {
            if (LobbyInfoUI != null)
            {
                LobbyInfoUI.gameObject.SetActive(false);
            }
            base.Start();
        }


        public override void SubscribeToEvents()
        {
            EventRegistrar.Subscribe<ArcadeInfoEvent>(OnArcadeInfoEvent);
            EventRegistrar.Subscribe<LobbyInfoEvent>(OnLobbyInfoEvent);
            EventRegistrar.Subscribe<LobbyPlayerUpdateEvent>(OnLobbyPlayerUpdateEvent);
            EventRegistrar.Subscribe<ShowSubTabEvent>(OnShowFriendsEvent);
            EventRegistrar.Subscribe<ShowArcadeSideEvent>(OnShowArcadeSideEvent);
            base.SubscribeToEvents();
        }


        private async UniTask OnShowArcadeSideEvent(ShowArcadeSideEvent e)
        {
            Visible = e.Show;
            await ShowHide(e.Show);
            await UniTask.Yield();
        }

        private async UniTask OnShowFriendsEvent(ShowSubTabEvent e)
        {

            await ShowHideFriendsTab(e.Show);
            await UniTask.Yield();
        }

        private async UniTask OnLobbyPlayerUpdateEvent(LobbyPlayerUpdateEvent lobbyPlayerUpdateEvent)
        {
           
            LobbyInfoUIShow(true);

            if (LobbyInfoUI != null)
            {
                if (lobbyPlayerUpdateEvent.Button3DSimple is LobbyHolderUI lobbyHolderUI)
                {
                    Debug.Log($"Player Update: {lobbyPlayerUpdateEvent.Type}");

                    if (lobbyPlayerUpdateEvent.Type == LobbyPlayerUpdateEvent.UpdateType.Add)
                    {
                        Debug.Log("A player was added!");
                    }
                    else if (lobbyPlayerUpdateEvent.Type == LobbyPlayerUpdateEvent.UpdateType.Remove)
                    {
                        Debug.Log("A player was removed!");
                    }
                    
                }

            }

            await UniTask.Yield();
        }

        private async UniTask UpdateMask3DHandlers()
        {
            if (Mask3DHandlers is { Count: > 0 })
            {
                foreach (Mask3DHandler mask3DHandler in Mask3DHandlers)
                {
                    await mask3DHandler.UpdateVisibilityAsync();
                }
            }
        }

        private async UniTask OnLobbyInfoEvent(LobbyInfoEvent lobbyInfoEvent)
        {
            LobbyInfoUIShow(true);

            if (LobbyInfoUI != null)
            {
                if (lobbyInfoEvent.Button3DSimple is LobbyHolderUI lobbyHolderUI)
                {
                    LobbyInfoUI.SetGameMode(lobbyHolderUI);
                    LobbyInfoUI.SetKeyValues(lobbyHolderUI.LobbyInfoEntries);
                   
                }
            }

            await UniTask.Yield();
        }

        private async UniTask OnArcadeInfoEvent(ArcadeInfoEvent infoEvent)
        {
           
            if (InfoText != null)
            {
                InfoText.text = infoEvent.Info;
            }
            
            LobbyInfoUIShow(false);

            await UniTask.Yield();
        }


        private async UniTask ShowHide(bool show)
        {
            Vector3 currentPosition = transform.localPosition;
            Vector3 targetPosition = show ? ShowPosition : OriginalPosition;

            if (currentPosition == targetPosition) return;

            float remainingTime = AnimateDuration;
            float startTime = Time.realtimeSinceStartup;

            while (remainingTime > 0)
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                remainingTime = Mathf.Max(0, AnimateDuration - elapsedTime);

                float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsedTime / AnimateDuration));

                transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, t);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEditor.SceneView.RepaintAll();
                }
#endif

                await UniTask.Delay(1);
            }

            transform.localPosition = targetPosition;
        }


        private async UniTask ShowHideFriendsTab(bool show)
        {
            Vector3 initialFriendsTabPosition = FriendsTab.transform.localPosition;
            Vector3 targetFriendsTabPosition = show ? FriendsTabVisiblePosition : FriendsTabHiddenPosition;


            if (initialFriendsTabPosition == targetFriendsTabPosition)
            {
                return;
            }



            Vector3 initialInfoPosition = InfoScrollRectTransform.localPosition;
            Vector3 targetInfoPosition = show
                ? new Vector3(InfoScrollCollapsed.X, InfoScrollCollapsed.Y, InfoScrollCollapsed.Z)
                : new Vector3(InfoScrollRectExpanded.X, InfoScrollRectExpanded.Y, InfoScrollRectExpanded.Z);

            Vector2 initialInfoSize = InfoScrollRectTransform.sizeDelta;
            Vector2 targetInfoSize = show
                ? new Vector2(InfoScrollCollapsed.Width, InfoScrollCollapsed.Height)
                : new Vector2(InfoScrollRectExpanded.Width, InfoScrollRectExpanded.Height);


            RemainingTime = AnimateDuration;
            float startTime = Time.realtimeSinceStartup;

            while (RemainingTime > 0)
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                RemainingTime = (Mathf.Max(0, AnimateDuration - elapsedTime));

                float t = Mathf.Clamp01(elapsedTime / AnimateDuration);

                t = Mathf.SmoothStep(0, 1, t);

                FriendsTab.transform.localPosition = Vector3.Lerp(initialFriendsTabPosition, targetFriendsTabPosition, t);

                if (InfoScrollRectTransform != null)
                {
                    InfoScrollRectTransform.localPosition = Vector3.Lerp(initialInfoPosition, targetInfoPosition, t);
                    InfoScrollRectTransform.sizeDelta = Vector2.Lerp(initialInfoSize, targetInfoSize, t);
                }

#if UNITY_EDITOR

                if (!Application.isPlaying)
                {
                    UnityEditor.SceneView.RepaintAll();
                }
#endif
                await UpdateMask3DHandlers();
                await UniTask.Delay(1);
            }

            FriendsTab.transform.localPosition = targetFriendsTabPosition;

            if (InfoScrollRectTransform != null)
            {
                InfoScrollRectTransform.localPosition = targetInfoPosition;
                InfoScrollRectTransform.sizeDelta = targetInfoSize;
            }
            
            FriendsTabVisible = FriendsTab.transform.localPosition == FriendsTabVisiblePosition;
           
            await EventBus.Instance.PublishAsync(new InfoSubTabStateChangedEvent(FriendsTabVisible));

        }




        public void Init()
        {
            InfoText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(InfoText));
            InfoTextPanel = transform.FindChildRecursively<RectTransform>(nameof(InfoTextPanel));
            FriendsTab = transform.RecursiveFindChildGameObject(nameof(FriendsTab));
            InfoTab = transform.RecursiveFindChildGameObject(nameof(InfoTab));
            if (InfoTab != null)
            {
                InfoScrollRect = InfoTab.transform.FindChildRecursively<ScrollRect>(nameof(InfoScrollRect));
                if (InfoScrollRect != null)
                {
                    InfoScrollRectTransform = InfoScrollRect.GetComponent<RectTransform>();
                }
            }

            Mask3DHandlers = transform.FindAllChildrenOfType<Mask3DHandler>();

            LobbyInfoUI = transform.FindChildRecursively<LobbyInfoUI>(nameof(LobbyInfoUI));

        }

        [Button]
        void KeyPanelState()
        {
            if (Visible)
            {
                ShowPosition = transform.localPosition;
            }
            else
            {
                OriginalPosition = transform.localPosition;
            }
        }

        [Button]
        void KeyFriendsState()
        {
            if (FriendsTabVisible)
            {
                FriendsTabVisiblePosition = FriendsTab.transform.localPosition;
            }
            else
            {
                FriendsTabHiddenPosition = FriendsTab.transform.localPosition;
            }
        }

        [Button]
        void KeyRectVector()
        {
            if (FriendsTabVisible)
            {
                if (InfoScrollRectTransform != null)
                {
                    InfoScrollCollapsed = new RectVector(
                        InfoScrollRectTransform.localPosition.x,
                        InfoScrollRectTransform.localPosition.y,
                        InfoScrollRectTransform.localPosition.z,
                        InfoScrollRectTransform.sizeDelta.x,
                        InfoScrollRectTransform.sizeDelta.y
                    );
                }
            }
            else
            {
                if (InfoScrollRectTransform != null)
                {
                    InfoScrollRectExpanded = new RectVector(
                        InfoScrollRectTransform.localPosition.x,
                        InfoScrollRectTransform.localPosition.y,
                        InfoScrollRectTransform.localPosition.z,
                        InfoScrollRectTransform.sizeDelta.x,
                        InfoScrollRectTransform.sizeDelta.y
                    );
                }
            }
        }

        private void OnValueChanged()
        {
            if (!Application.isPlaying)
            {
                ShowHideFriendsTab(FriendsTabVisible).Forget();
                ShowHide(Visible).Forget();
               
                LobbyInfoUIShow(LobbyInfoUIVisible);

            }
        }

        private void LobbyInfoUIShow(bool lobbyInfoUIVisible)
        {
            if (LobbyInfoUI != null)
            {
                LobbyInfoUI.gameObject.SetActive(lobbyInfoUIVisible);
            }

            if (InfoTextPanel != null)
            {
                InfoTextPanel.gameObject.SetActive(!lobbyInfoUIVisible);
            }

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }
    }
}
