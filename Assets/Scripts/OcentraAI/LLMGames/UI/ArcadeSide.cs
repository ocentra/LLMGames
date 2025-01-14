using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
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
        [ShowInInspector] protected GameObject InfoTab { get; set; }
        [ShowInInspector] protected GameObject FriendsTab { get; set; }
        [ShowInInspector] protected ScrollRect InfoScrollRect { get; set; }
        [ShowInInspector] private RectTransform InfoScrollRectTransform { get; set; }
        [SerializeField, ShowInInspector] protected RectVector InfoScrollRectExpanded { get; set; }
        [SerializeField, ShowInInspector] protected RectVector InfoScrollCollapsed { get; set; }

        [SerializeField, ShowInInspector, ReadOnly] protected Vector3 FriendsTabHiddenPosition { get; set; }
        [SerializeField, ShowInInspector, ReadOnly] protected Vector3 FriendsTabVisiblePosition { get; set; }
        [SerializeField, ShowInInspector, OnValueChanged(nameof(OnValueChanged))] protected bool FriendsTabVisible { get; set; }

        [SerializeField, TextArea(5, 15), RichText, PropertyOrder(-1)]
        protected string Info;


        protected override void OnValidate()
        {
            Init();
            base.OnValidate();
        }

        protected override void Awake()
        {
            Init();
            base.Start();
        }



        public override void SubscribeToEvents()
        {
            EventRegistrar.Subscribe<ArcadeInfoEvent>(OnArcadeInfoEvent);
            EventRegistrar.Subscribe<ShowFriendsEvent>(OnShowFriendsEvent);
            EventRegistrar.Subscribe<ShowArcadeSideEvent>(OnShowArcadeSideEvent);
            base.SubscribeToEvents();
        }

        private async UniTask OnShowArcadeSideEvent(ShowArcadeSideEvent e)
        {
            Visible = e.Show;
            await ShowHide();
            await UniTask.Yield();
        }

        private async UniTask OnShowFriendsEvent(ShowFriendsEvent e)
        {
            FriendsTabVisible = e.Show;
            await ShowHideFriendsTab();
            await UniTask.Yield();
        }
        private async UniTask OnArcadeInfoEvent(ArcadeInfoEvent e)
        {
            if (InfoText != null)
            {
                InfoText.text = e.Info;
            }

            await UniTask.Yield();
        }

        
        private async UniTask ShowHide()
        {
            
            Vector3 initialPosition = transform.localPosition;
            Vector3 targetPosition = Visible ? ShowPosition : OriginalPosition;


            RemainingTime = AnimateDuration;
            float startTime = Time.realtimeSinceStartup;

            while (RemainingTime > 0)
            {
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                RemainingTime = (Mathf.Max(0, AnimateDuration - elapsedTime));

                float t = Mathf.Clamp01(elapsedTime / AnimateDuration);

                t = Mathf.SmoothStep(0, 1, t);

                transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);

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

        private async UniTask ShowHideFriendsTab()
        {


          
            Vector3 initialPosition = FriendsTab.transform.localPosition;
            Vector3 targetPosition = FriendsTabVisible ? FriendsTabVisiblePosition : FriendsTabHiddenPosition;

            Vector3 initialInfoPosition = InfoScrollRectTransform.localPosition;
            Vector3 targetInfoPosition = FriendsTabVisible
                ? new Vector3(InfoScrollCollapsed.X, InfoScrollCollapsed.Y, InfoScrollCollapsed.Z)
                : new Vector3(InfoScrollRectExpanded.X, InfoScrollRectExpanded.Y, InfoScrollRectExpanded.Z);

            Vector2 initialInfoSize = InfoScrollRectTransform.sizeDelta;
            Vector2 targetInfoSize = FriendsTabVisible
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

                FriendsTab.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);

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

                await UniTask.Delay(1);
            }

            FriendsTab.transform.localPosition = targetPosition;

            if (InfoScrollRectTransform != null)
            {
                InfoScrollRectTransform.localPosition = targetInfoPosition;
                InfoScrollRectTransform.sizeDelta = targetInfoSize;
            }

           
        }




        public void Init()
        {
            InfoText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(InfoText));
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
            ShowHideFriendsTab().Forget();
            ShowHide().Forget();

        }
    }
}
