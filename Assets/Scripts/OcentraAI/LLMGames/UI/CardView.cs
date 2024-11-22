using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Scriptable;
using System;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CardView : MonoBehaviour
{
    [System.Serializable]
    public class CardMaterialState
    {
        public Material[] OriginalMaterials;
        public Material[] RuntimeMaterials;
        public Color OriginalColor;
        public MeshRenderer Renderer;

        public void Initialize(MeshRenderer renderer, string materialNameSuffix)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && (renderer == null || !renderer)) return;
#endif
            Renderer = renderer;
            if (Renderer == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject prefab = null;
                try
                {
                    prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(renderer.gameObject);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting prefab source: {ex.Message}");
                }

                if (prefab != null && prefab)
                {
                    var meshRenderer = prefab.GetComponent<MeshRenderer>();
                    if (meshRenderer != null && meshRenderer)
                    {
                        OriginalMaterials = meshRenderer.sharedMaterials;
                    }
                }
                else
                {
                    OriginalMaterials = renderer.sharedMaterials;
                }
            }
            else
#endif
            {
                OriginalMaterials = renderer.sharedMaterials;
            }

            CreateRuntimeMaterials(materialNameSuffix);
        }

        private void CreateRuntimeMaterials(string materialNameSuffix)
        {
            if (OriginalMaterials == null || OriginalMaterials.Length == 0) return;

            RuntimeMaterials = new Material[OriginalMaterials.Length];
            for (int i = 0; i < OriginalMaterials.Length; i++)
            {
                if (OriginalMaterials[i] == null) continue;
                RuntimeMaterials[i] = new Material(OriginalMaterials[i])
                {
                    name = $"{OriginalMaterials[i].name}_{materialNameSuffix}"
                };
            }

            if (RuntimeMaterials.Length > 0 && RuntimeMaterials[0] != null && RuntimeMaterials[0].HasProperty("_BaseColor"))
            {
                OriginalColor = RuntimeMaterials[0].GetColor("_BaseColor");
            }
        }

        public void ApplyMaterials(bool useRuntime)
        {
            if (Renderer == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying && !Renderer) return;
#endif

            Renderer.sharedMaterials = useRuntime ? RuntimeMaterials : OriginalMaterials;
        }

        public void Dispose()
        {
            if (RuntimeMaterials == null) return;
            foreach (var material in RuntimeMaterials)
            {
                if (material != null)
                {
                    Object.DestroyImmediate(material);
                }
            }
            RuntimeMaterials = null;
        }
    }

    [SerializeField, InlineProperty] protected CardMaterialState mainCardState = new();
    [SerializeField, InlineProperty] protected CardMaterialState highlightCardState = new();
    [SerializeField, InlineProperty] protected CardMaterialState backCardState = new();

#if UNITY_EDITOR
    protected bool IsValidObject(Object obj)
    {
        try
        {
            return obj != null && !ReferenceEquals(obj, null) && obj;
        }
        catch
        {
            return false;
        }
    }

    protected bool IsPrefabValidationContext()
    {
        try
        {
            return PrefabUtility.IsPartOfPrefabAsset(gameObject) ||
                   PrefabUtility.IsPartOfPrefabInstance(gameObject) ||
                   PrefabUtility.IsPartOfNonAssetPrefabInstance(gameObject);
        }
        catch
        {
            return false;
        }
    }

    protected bool IsEditorSafe()
    {
        if (Application.isPlaying) return true;
        return IsValidObject(this) && IsValidObject(gameObject) && !IsPrefabValidationContext();
    }
#endif

    [ShowInInspector]
    public MeshRenderer MainCard
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return null;
#endif
            return mainCardState?.Renderer;
        }
    }

    [ShowInInspector]
    public MeshRenderer BackCard
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return null;
#endif
            return backCardState?.Renderer;
        }
    }

    [ShowInInspector]
    public MeshRenderer HighlightCard
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return null;
#endif
            return highlightCardState?.Renderer;
        }
    }

    [ShowInInspector]
    public Material MainCardMaterial
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return null;
#endif
            if (mainCardState?.RuntimeMaterials == null || mainCardState.RuntimeMaterials.Length == 0) return null;
            return mainCardState.RuntimeMaterials[0];
        }
    }

    [ShowInInspector]
    public Material HighlightCardMaterial
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return null;
#endif
            if (highlightCardState?.RuntimeMaterials == null || highlightCardState.RuntimeMaterials.Length == 0) return null;
            return highlightCardState.RuntimeMaterials[0];
        }
    }

    [SerializeField, ColorUsage(true, true), ReadOnly]
    public Color originalHighlightColor
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return Color.white;
#endif
            return highlightCardState?.OriginalColor ?? Color.white;
        }
    }

    [SerializeField, ColorUsage(true, true), ReadOnly]
    public Color originalMainCardColor
    {
        get
        {
#if UNITY_EDITOR
            if (!IsEditorSafe()) return Color.white;
#endif
            return mainCardState?.OriginalColor ?? Color.white;
        }
    }

    [Header("Card Settings")]
    [SerializeField, ColorUsage(true, true)]
    protected Color highlightColor = Color.white;

    [SerializeField, ColorUsage(true, true)]
    protected Color mainCardColor = Color.white;

    [ShowInInspector] public Card Card { get; private set; }

#if UNITY_EDITOR
    [ShowInInspector] private bool showHighlightPreview;
    [ShowInInspector] private bool showMainCardPreview;
#endif

    protected bool isCardHighlighted;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            InitializeAllStates();
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        ApplyAllMaterials(true);
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        ApplyAllMaterials(false);
    }

    private void OnDestroy()
    {
        DisposeAllStates();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!enabled || !IsEditorSafe()) return;

        if (!Application.isPlaying)
        {
            InitializeAllStates();

            if (showHighlightPreview)
            {
                ApplyPreviewColors();
                SetHighlightPreview(showHighlightPreview);
            }

            if (showMainCardPreview)
            {
                SetMainCardPreview(showMainCardPreview);
            }
        }
#endif
    }

    private void InitializeAllStates()
    {
        Init();
        SetActive(false);
        UpdateCardView();
    }

    protected virtual void Init()
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        mainCardState.Initialize(GetChildRenderer("MainCard"), "main");
        highlightCardState.Initialize(GetChildRenderer("HighlightCard"), "highlight");
        backCardState.Initialize(GetChildRenderer("BackCard"), "back");
    }

    protected MeshRenderer GetChildRenderer(string name)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return null;
#endif
        return transform.FindChildRecursively<MeshRenderer>(name);
    }

    protected virtual void ApplyAllMaterials(bool useRuntime)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif

        mainCardState.ApplyMaterials(useRuntime);
        highlightCardState.ApplyMaterials(useRuntime);
        backCardState.ApplyMaterials(useRuntime);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    protected virtual void DisposeAllStates()
    {
        mainCardState.Dispose();
        highlightCardState.Dispose();
        backCardState.Dispose();
    }

    public virtual void SetCard(Card newCard)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif
        Card = newCard;
        UpdateCardView();
    }

    public virtual void UpdateCardView()
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif

        if (Card != null && Card.Texture2D != null && MainCardMaterial != null)
        {
            MainCardMaterial.mainTexture = Card.Texture2D;
            if (MainCard != null)
            {
                MainCard.gameObject.SetActive(true);
            }
            if (BackCard != null)
            {
                BackCard.gameObject.SetActive(false);
            }

            if (MainCardMaterial.HasProperty("_BaseColor"))
            {
                MainCardMaterial.SetColor("_BaseColor", mainCardColor);
            }
        }
        else
        {
            ShowBackside();
        }
    }

    public virtual void ShowBackside()
    {
        if (MainCard != null)
        {
            MainCard.gameObject.SetActive(false);
        }
        if (BackCard != null)
        {
            BackCard.gameObject.SetActive(true);
        }
    }

    public virtual void SetHighlight(bool set, Color? glowColor = null)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif

        if (HighlightCard == null || HighlightCardMaterial == null)
        {
            Debug.LogWarning("Highlight card components are not properly initialized.");
            return;
        }

        HighlightCard.gameObject.SetActive(set);
        Material material = HighlightCardMaterial;

        if (set && material != null && material.HasProperty("_BaseColor"))
        {
            Color colorToSet = glowColor.HasValue ? glowColor.Value : highlightColor;
            material.SetColor("_BaseColor", colorToSet);
            material.EnableKeyword("_EMISSION");
        }
        else if (material != null)
        {
            material.DisableKeyword("_EMISSION");
        }

        isCardHighlighted = set;
    }

    public virtual void SetActive(bool show = false)
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif

        if (MainCard != null)
        {
            MainCard.gameObject.SetActive(show && Card != null);
        }
        if (BackCard != null)
        {
            BackCard.gameObject.SetActive(show && Card == null);
        }
        if (HighlightCard != null)
        {
            HighlightCard.gameObject.SetActive(false || isCardHighlighted);
        }

        TextMeshProUGUI[] texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text != null) text.enabled = show;
        }

        TextMeshPro[] tmp = gameObject.GetComponentsInChildren<TextMeshPro>(true);
        foreach (TextMeshPro text in tmp)
        {
            if (text != null) text.enabled = show;
        }
    }

    protected virtual void ApplyPreviewColors()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (HighlightCardMaterial != null && HighlightCardMaterial.HasProperty("_BaseColor"))
            {
                HighlightCardMaterial.SetColor("_BaseColor", highlightColor);
                HighlightCardMaterial.EnableKeyword("_EMISSION");
            }

            if (MainCardMaterial != null && MainCardMaterial.HasProperty("_BaseColor"))
            {
                MainCardMaterial.SetColor("_BaseColor", mainCardColor);
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }

#if UNITY_EDITOR
    public virtual void SetMainCardPreview(bool show)
    {
        if (!IsEditorSafe()) return;

        if (MainCard != null)
        {
            MainCard.gameObject.SetActive(show);
        }
        if (BackCard != null)
        {
            BackCard.gameObject.SetActive(!show);
        }
    }

    public virtual void SetHighlightPreview(bool show)
    {
        if (!IsEditorSafe()) return;

        if (HighlightCard != null)
        {
            HighlightCard.gameObject.SetActive(show);
        }
    }
#endif

    public virtual void ResetCardView()
    {
#if UNITY_EDITOR
        if (!IsEditorSafe()) return;
#endif

        Card = null;
        ShowBackside();
    }
}