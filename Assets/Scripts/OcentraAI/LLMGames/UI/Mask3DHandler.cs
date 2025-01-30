using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/3D Mask Handler", 14)]
[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Mask))]
[ExecuteAlways]
public class Mask3DHandler : MonoBehaviourBase<Mask3DHandler>
{
    [ShowInInspector, ListDrawerSettings(HideAddButton = true)]
    [ReadOnly]
    public Dictionary<GameObject, List<Renderer>> TargetObjects;
    [ShowInInspector, SerializeField] protected RectTransform Content;
    [ShowInInspector, SerializeField] protected Mask Mask;
    [ShowInInspector, SerializeField] protected ScrollRect PlayerScrollView;
    [ShowInInspector, SerializeField] protected Camera MainCamera;
    [ShowInInspector, SerializeField] protected RectTransform MaskRect;



    protected override void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Initialize();
            UpdateVisibilityAsync().Forget();
        }

        base.OnValidate();
    }

    protected override void Awake()
    {
        Initialize();
        base.Awake();
    }
    
    private void Initialize()
    {
        if (Mask == null)
        {
            Mask = GetComponent<Mask>();
        }


        if (Mask == null) return ;

        MaskRect = Mask.rectTransform;

        MainCamera = Camera.main;

        if (Content == null)
        {
            Content = transform.FindChildRecursively<RectTransform>(nameof(Content));
        }

        if (Content == null) return ;


        TargetObjects = null;

        int childCount = Content.childCount;
        TargetObjects = new Dictionary<GameObject, List<Renderer>>(childCount);

        for (int i = 0; i < childCount; i++)
        {
            GameObject go = Content.GetChild(i).gameObject;
            TargetObjects.Add(go, new List<Renderer>(go.GetComponentsInChildren<Renderer>()));
        }


        if (PlayerScrollView == null)
        {
            PlayerScrollView = transform.parent.GetComponent<ScrollRect>();
        }

        if (PlayerScrollView != null)
        {
            PlayerScrollView.onValueChanged.RemoveAllListeners();
            PlayerScrollView.onValueChanged.AddListener(v => UpdateVisibility());
        }

        
    }

    private void UpdateVisibility()
    {
        UpdateVisibilityAsync().Forget();
    }
    public async UniTask UpdateVisibilityAsync()
    {
        if (MaskRect == null || TargetObjects == null || !gameObject.activeInHierarchy) return;

        MaskRect = Mask.rectTransform;

        foreach (KeyValuePair<GameObject, List<Renderer>> targetObject in TargetObjects)
        {
            if (targetObject.Key != null)
            {
                Vector3 screenPoint = MainCamera.WorldToScreenPoint(targetObject.Key.transform.position);
                bool isVisible = RectTransformUtility.RectangleContainsScreenPoint(MaskRect, screenPoint, MainCamera);
                foreach (Renderer r in targetObject.Value)
                {
                    r.enabled = isVisible;
                }

            }
        }

        await UniTask.Yield();
    }


}
