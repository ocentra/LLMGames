using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class Button3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Range(0f, 1f), OnValueChanged(nameof(OnPercentageChanged)), ShowIf(nameof(isForSelf))]
        [SerializeField] private float blendShapePercentage;

        [Required][SerializeField] private BoxCollider boxCollider;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private string buttonName = "Button";

        [Header("Text Settings")]
        [ShowIf(nameof(isForSelf))]
        [SerializeField] private TextMeshPro buttonText;

        [HideIf(nameof(isForSelf))]
        [SerializeField] private CardView[] cardViewsToHighlight;

        [ColorUsage(true, true)]
        [SerializeField] private Color disabledColor = Color.black;

        [ColorUsage(true, true)]
        [SerializeField] private Color highlightedColor = Color.yellow;

        [ColorUsage(true, true), ShowIf(nameof(isForSelf))]
        [SerializeField] private Color highlightedColor2 = Color.cyan;

        [ColorUsage(true, true)]
        [SerializeField] private Color pressedColor = Color.gray;

        [SerializeField] private Color normalColor = Color.white;

        [Header("Material Indices")]
        [SerializeField] private int baseMaterialIndex = 0;
        [SerializeField] private int highlightMaterialIndex1 = 1;
        [SerializeField] private int highlightMaterialIndex2 = 2;

        [ShowInInspector] protected bool ApplyHighlight2Material = true;

        [ShowInInspector, ReadOnly] protected bool Interactable { get; set; } = true;

        [Header("Button Settings")]
        [SerializeField] private bool isForSelf = false;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private float maxXSize = 2f;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private float minXSize = 0.5f;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private Material[] modifiedMaterials;

        [Header("Self Material Settings")]
        [ShowIf(nameof(isForSelf))]
        [SerializeField] private Renderer objectRenderer;

        public UnityEvent onClick;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private Material[] originalMaterials;



        [ShowIf(nameof(isForSelf))]
        [SerializeField] private bool previewHDR;

        [ShowInInspector] private SkinnedMeshRenderer skinnedMeshRenderer;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private readonly float TextRectMaxSize = 125f;

        [ShowIf(nameof(isForSelf))]
        [SerializeField] private readonly float TextRectMinSize = 50f;

        [SerializeField] private bool applyOffset = false;


        public virtual void OnPointerClick(PointerEventData eventData)
        {
           
            if (!isForSelf)
            {
                SetCardViewColor(pressedColor);
            }
            else if (modifiedMaterials is { Length: > 0 })
            {
                SetHighlightColors(pressedColor, pressedColor);
                SetEmission(true);
            }

            if (!Interactable) return;
            onClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Interactable)
            {
                if (!isForSelf)
                {
                    SetCardViewColor(highlightedColor);
                }
                else if (modifiedMaterials is { Length: > 0 })
                {
                    SetHighlightColors(highlightedColor, highlightedColor2);
                    SetEmission(true);
                }
            }
            else
            {
                if (!isForSelf)
                {
                    SetCardViewColor(disabledColor);
                }
                else if (modifiedMaterials is { Length: > 0 })
                {
                    SetHighlightColors(disabledColor, Color.black);
                    SetEmission();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isForSelf)
            {
                SetCardViewColor(normalColor);
            }
            else if (modifiedMaterials is { Length: > 0 })
            {
                SetHighlightColors(normalColor, normalColor);
                SetEmission();
            }
        }

        private void OnValidate()
        {
            Init();
        }

        private void Start()
        {
            previewHDR = false;
            Init();
        }

        private void OnDisable()
        {
            ApplyMaterials(true);
            Init();
        }

        private void Reset()
        {
            ApplyMaterials(true);
            Init();
        }

        private void ApplyMaterials(bool original = false)
        {
            if (objectRenderer == null) return;

            Material[] materialsToApply = original ? originalMaterials : modifiedMaterials;
            if (materialsToApply is { Length: > 0 })
            {
                objectRenderer.materials = materialsToApply;
            }
        }

        protected virtual void Init()
        {

            if (!isForSelf)
            {
                cardViewsToHighlight = transform.GetComponentsInChildren<CardView>(true);
                SetCardViewColor(normalColor, false);
                return;
            }

            if (objectRenderer == null)
            {
                objectRenderer = GetComponent<Renderer>();
            }

            if (objectRenderer == null)
            {
                objectRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (objectRenderer is SkinnedMeshRenderer smr)
            {
                skinnedMeshRenderer = smr;
            }

            boxCollider = GetComponent<BoxCollider>();

            if (boxCollider == null)
            {
                boxCollider = objectRenderer?.transform.GetComponent<BoxCollider>();
            }

            if (boxCollider == null && objectRenderer != null)
            {
                boxCollider = objectRenderer.gameObject.AddComponent<BoxCollider>();
            }

            FindOriginals();
            CopyOriginals();

            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TextMeshPro>();
            }

            if (buttonText == null && transform.parent != null)
            {
                buttonText = transform.parent.transform.GetComponentInChildren<TextMeshPro>();
            }

            SetButtonName(buttonName);

            UpdateBlendShape();

            if (modifiedMaterials is { Length: > 0 })
            {
                SetHighlightColors(previewHDR ? highlightedColor : normalColor,
                    previewHDR ? highlightedColor2 : normalColor);
                SetEmission();
            }
        }

        private void FindOriginals()
        {
            if (objectRenderer == null) return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(objectRenderer.gameObject);

                if (prefab != null)
                {
                    Renderer prefabRenderer = prefab.GetComponent<Renderer>();
                    originalMaterials = prefabRenderer != null
                        ? prefabRenderer.sharedMaterials
                        : objectRenderer.sharedMaterials;
                }
                else
                {
                    originalMaterials = objectRenderer.sharedMaterials;
                }

                if (originalMaterials is { Length: > 0 })
                {
                    EditorUtility.SetDirty(this);
                    EditorUtility.SetDirty(objectRenderer.gameObject);
                }
            }
#endif
        }

        private void CopyOriginals()
        {
            if (originalMaterials == null || originalMaterials.Length == 0) return;

            modifiedMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                if (originalMaterials[i] == null) continue;

                string materialName = $"{originalMaterials[i].name}_{gameObject.name}";

                if (modifiedMaterials[i] == null || modifiedMaterials[i].name != materialName)
                {
                    Material mat = new Material(originalMaterials[i]) { name = materialName };

                    if (i > 0 && mat.HasProperty("_EmissionColor"))
                    {
                        mat.DisableKeyword("_EMISSION");
                    }

                    modifiedMaterials[i] = mat;
                }
            }
        }

        [Button]
        [ShowIf(nameof(isForSelf))]
        public void SetPercentage(float percentage)
        {
            if (!isForSelf) return;

            blendShapePercentage = Mathf.Clamp01(percentage);
            UpdateBlendShape();
        }

        private void UpdateBlendShape()
        {
            if (skinnedMeshRenderer != null &&
                skinnedMeshRenderer.sharedMesh != null &&
                skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(0, blendShapePercentage * 100f);
                UpdateCollider();
                UpdateButtonText();
            }
        }



        private void UpdateCollider()
        {
            if (boxCollider == null || skinnedMeshRenderer == null) return;

            Bounds meshBounds = skinnedMeshRenderer.localBounds;
            boxCollider.center = meshBounds.center;
            boxCollider.size = meshBounds.size;

            float xSize = Mathf.Lerp(minXSize, maxXSize, blendShapePercentage);
            Vector3 newSize = boxCollider.size;
            newSize.x = xSize;

            if (applyOffset)
            {
                float offset = Mathf.Lerp(minXSize * 0.5f, maxXSize * 0.5f, blendShapePercentage);
                Vector3 newCenter = boxCollider.center;
                newCenter.x += offset;
                boxCollider.center = newCenter;
            }

            boxCollider.size = newSize;
        }

        private void UpdateButtonText()
        {
            if (buttonText == null) return;

            float width = Mathf.Lerp(TextRectMinSize, TextRectMaxSize, blendShapePercentage);
            RectTransform rectTransform = buttonText.rectTransform;
            if (rectTransform != null)
            {
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.x = width;
                rectTransform.sizeDelta = sizeDelta;
            }
        }

        private void OnPercentageChanged()
        {
            UpdateBlendShape();
        }

        public virtual void SetInteractable(bool interactable, bool enableCollider = true)
        {
            Interactable = interactable;
            if (boxCollider != null)
            {
                boxCollider.enabled = enableCollider;
            }
        }

        private void SetCardViewColor(Color color, bool set = true)
        {
            if (cardViewsToHighlight == null) return;

            foreach (CardView cardView in cardViewsToHighlight)
            {
                if (cardView != null)
                {
                    cardView.SetHighlight(set, color);
                }
            }
        }

        private void SetHighlightColors(Color color1, Color color2)
        {
            if (modifiedMaterials is not { Length: > 1 })
            {
                Debug.LogWarning($"No materials available for button {gameObject.name}");
                return;
            }

            if (baseMaterialIndex < modifiedMaterials.Length)
            {
                ApplyColorToMaterial(modifiedMaterials[baseMaterialIndex], color1);
            }

            if (highlightMaterialIndex1 < modifiedMaterials.Length)
            {
                ApplyColorToMaterial(modifiedMaterials[highlightMaterialIndex1], color1);
            }

            if (highlightMaterialIndex2 < modifiedMaterials.Length)
            {
                Material modifiedMaterial = modifiedMaterials[highlightMaterialIndex2];
                ApplyColorToMaterial(modifiedMaterial, ApplyHighlight2Material ? color2 : Color.black);
            }

            ApplyMaterials();
        }

        private void ApplyColorToMaterial(Material material, Color color)
        {
            if (material == null) return;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", color);
            }
        }

        private void SetEmission(bool enable = false)
        {
            if (modifiedMaterials is not { Length: > 1 })
            {
                return;
            }

            if (highlightMaterialIndex1 >= 0 && highlightMaterialIndex1 < modifiedMaterials.Length)
            {
                ApplyEmissionToMaterial(modifiedMaterials[highlightMaterialIndex1], enable);
            }

            if (highlightMaterialIndex2 >= 0 && highlightMaterialIndex2 < modifiedMaterials.Length)
            {
                ApplyEmissionToMaterial(modifiedMaterials[highlightMaterialIndex2], enable);
            }
        }

        private void ApplyEmissionToMaterial(Material mat, bool enable)
        {
            if (mat == null || !mat.HasProperty("_EmissionColor")) return;

            if (enable)
            {
                mat.EnableKeyword("_EMISSION");
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
            }
        }

        public void SetButtonName(string newName)
        {
            buttonName = newName;
            if (buttonText != null)
            {
                buttonText.text = buttonName;
            }
        }

        private void OnDestroy()
        {
            Reset();

            if (modifiedMaterials != null)
            {
                foreach (Material material in modifiedMaterials)
                {
                    if (material != null)
                    {
                        DestroyImmediate(material);
                    }
                }
            }
        }

        // Serialize the properties into a dictionary
        public Dictionary<string, object> SerializeToDictionary()
        {
            return new Dictionary<string, object>
    {
        { nameof(blendShapePercentage), blendShapePercentage },
        { nameof(buttonName), buttonName },
        { nameof(disabledColor), disabledColor },
        { nameof(highlightedColor), highlightedColor },
        { nameof(highlightedColor2), highlightedColor2 },
        { nameof(pressedColor), pressedColor },
        { nameof(normalColor), normalColor },
        { nameof(baseMaterialIndex), baseMaterialIndex },
        { nameof(highlightMaterialIndex1), highlightMaterialIndex1 },
        { nameof(highlightMaterialIndex2), highlightMaterialIndex2 },
        { nameof(isForSelf), isForSelf },
        { nameof(maxXSize), maxXSize },
        { nameof(minXSize), minXSize },
        { nameof(applyOffset), applyOffset }
    };
        }

        // Deserialize and apply properties from a dictionary
        public void DeserializeFromDictionary(Dictionary<string, object> data)
        {
            if (data == null) return;

            blendShapePercentage = (float)data[nameof(blendShapePercentage)];
            buttonName = (string)data[nameof(buttonName)];
            disabledColor = (Color)data[nameof(disabledColor)];
            highlightedColor = (Color)data[nameof(highlightedColor)];
            highlightedColor2 = (Color)data[nameof(highlightedColor2)];
            pressedColor = (Color)data[nameof(pressedColor)];
            normalColor = (Color)data[nameof(normalColor)];
            baseMaterialIndex = (int)data[nameof(baseMaterialIndex)];
            highlightMaterialIndex1 = (int)data[nameof(highlightMaterialIndex1)];
            highlightMaterialIndex2 = (int)data[nameof(highlightMaterialIndex2)];
            isForSelf = (bool)data[nameof(isForSelf)];
            maxXSize = (float)data[nameof(maxXSize)];
            minXSize = (float)data[nameof(minXSize)];
            applyOffset = (bool)data[nameof(applyOffset)];


        }



    }
}