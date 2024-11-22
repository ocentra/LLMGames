using Animancer;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.UI.Controllers
{
    [RequireComponent(typeof(AnimancerComponent))]
    [RequireComponent(typeof(Animator))]
    [ExecuteInEditMode]
    public class AnimationController : MonoBehaviour
    {
        [Required] public AnimancerComponent AnimancerComponent;

        [ShowInInspector] private AnimancerState animancerState;

        private Coroutine animationCoroutine;

        [Required] public Animator animator;

        [Required] public AnimationClip DashClip;

        [SerializeField] private readonly bool startwithzero = true;

        [SerializeField]
        [ShowInInspector]
        [Range(0f, 1f)]
        [OnValueChanged("OnPercentageChanged")]
        [Tooltip("Control how much of the animation to play (0 = start, 1 = full).")]
        private float stopPercentage = 1f;

        [SerializeField] [Tooltip("Duration of the smooth transition when using SetPercentageAnimated")]
        private readonly float transitionDuration = 0.5f;


        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();
            if (startwithzero)
            {
                SetPercentage(0);
            }
        }

        private void Init()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (AnimancerComponent == null)
            {
                AnimancerComponent = GetComponent<AnimancerComponent>();
            }

            if (AnimancerComponent != null)
            {
                AnimancerComponent.Animator = animator;
            }

            if (DashClip == null)
            {
                FindFirstAnimationClip();
            }
        }

        private void FindFirstAnimationClip()
        {
#if UNITY_EDITOR
            GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            if (prefabRoot == null)
            {
                Debug.LogWarning("No prefab or model instance found on this object or its first child.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(prefabRoot);

            if (!string.IsNullOrEmpty(assetPath))
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        DashClip = clip;
                        return;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Unable to find asset path for the GameObject or its child.");
            }
#endif
        }


        private void OnPercentageChanged()
        {
            if (DashClip != null)
            {
                if (AnimancerComponent.Animator == null || !AnimancerComponent.Animator.gameObject.scene.IsValid())
                {
                    return;
                }

                animancerState = AnimancerComponent.Play(DashClip, 0f);
                animancerState.NormalizedTime = 0f;
                animancerState.Speed = 0f;
            }
            else
            {
                Debug.LogError("DashClip is not assigned.");
            }

            if (animancerState != null)
            {
                animancerState.NormalizedTime = Mathf.Clamp01(stopPercentage);
                animancerState.Speed = 0f;
                AnimancerComponent.Evaluate();
            }
        }

        public void SetPercentage(float percentage)
        {
            stopPercentage = percentage;
            OnPercentageChanged();
        }

        [Button]
        public void SetPercentageAnimated(float targetPercentage)
        {
            targetPercentage = Mathf.Clamp01(targetPercentage);

            if (DashClip != null)
            {
                AnimancerState state = AnimancerComponent.Play(DashClip);
                float startPercentage = stopPercentage;

                if (Mathf.Approximately(startPercentage, targetPercentage))
                {
                    return;
                }

                float animationSpeed = Mathf.Sign(targetPercentage - startPercentage) * (1f / transitionDuration);
                state.Speed = animationSpeed;
                state.NormalizedTime = startPercentage;

                AnimancerComponent.Play(state).Events.OnEnd = () =>
                {
                    state.Speed = 0f;
                    state.NormalizedTime = targetPercentage;
                    stopPercentage = targetPercentage;
                };
            }
            else
            {
                Debug.LogError("DashClip is not assigned.");
            }
        }
    }
}