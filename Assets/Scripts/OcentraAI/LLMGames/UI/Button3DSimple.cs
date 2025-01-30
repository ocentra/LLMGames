using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]

    public class Button3DSimple : MonoBehaviourBase<Button3DSimple>, IButton3DSimple, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IEquatable<Button3DSimple>
    {

        [SerializeField, FoldoutGroup("ButtonInfo", false)]
        protected string ButtonName = "Button";


        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false)]
        protected TextMeshPro ButtonText;

        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false)]
        protected Renderer ObjectRenderer;

        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false)]
        protected SkinnedMeshRenderer SkinnedMeshRenderer;

        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false)]
        protected Transform Parent;

        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false)]
        protected bool Interactable { get; set; } = true;

        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false), ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
        protected List<Button3DSimple> ButtonsOnThisGroup = new List<Button3DSimple>();
        

        [SerializeField, FoldoutGroup("ButtonInfo/BlendShape Info Configuration", false)]
        protected Dictionary<int, BlendShapeInfo> BlendShapeInfos = new Dictionary<int, BlendShapeInfo>();

        [SerializeField, FoldoutGroup("ButtonInfo/BlendShape Info Configuration", false)]
        protected float OriginalShapeValue = 0;


        [SerializeField, FoldoutGroup("ButtonInfo/Material Info Configuration", false)]
        protected List<MaterialInfo> MaterialInfos;

        [SerializeField, FoldoutGroup("ButtonInfo/Material Info Configuration", false), OnValueChanged(nameof(ApplyMaterialColors))]
        protected ButtonState State;

        [SerializeField, FoldoutGroup("ButtonInfo/Material Info Configuration", false)]
        protected IButton3DSimple LastPressedButton;
        
        public const string BaseColor = "_BaseColor";
        public const string EmissionColor = "_EmissionColor";

        private readonly Guid guid = Guid.NewGuid();

        [SerializeField, FoldoutGroup("ButtonInfo/Basic Info", false)]
        public UnityEvent OnClick;

        protected override void OnValidate()
        {
            Init();
            CacheOriginalShapeValue();
        }

        protected override void Start()
        {
            State = ButtonState.Normal;
            Init();
            CacheOriginalShapeValue();
        }

        public override void SubscribeToEvents()
        {
            if (!ButtonsOnThisGroup.Contains(this))
            {
                return;
            }
            EventRegistrar.Subscribe<Button3DSimpleClickEvent>(OnButton3DSimpleClick);
            base.SubscribeToEvents();
        }

        protected virtual void OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {
            if (State == ButtonState.Disabled) return;

            bool containsEventButton = false;
            for (int i = 0; i < ButtonsOnThisGroup.Count; i++)
            {
                if (ButtonsOnThisGroup[i] == (Button3DSimple)e.Button3DSimple)
                {
                    containsEventButton = true;
                    break;
                }
            }

            if (!containsEventButton || (Button3DSimple)e.Button3DSimple == this) return;

            LastPressedButton = e.Button3DSimple;
            State = ButtonState.Normal;
            ApplyMaterialColors(); 
        }

        protected virtual void Init()
        {
            Parent = transform.parent;
            SetButtonsOnThisGroup();

            if (ObjectRenderer == null)
            {
                ObjectRenderer = GetComponentInChildren<Renderer>(true);
                if (ObjectRenderer == null)
                {
                    Debug.LogError($"No Renderer found on {name} or its children!", this);
                    return;
                }
            }

            if (ObjectRenderer is SkinnedMeshRenderer smr)
            {
                SkinnedMeshRenderer = smr;
                InitializeBlendShapes();
            }

            if (MaterialInfos == null)
            {
                MaterialInfos = new List<MaterialInfo>();
            }

            InitializeMaterials(ObjectRenderer, MaterialInfos);
            CleanupMaterialInfos();
            InitializeTextComponent();
            ApplyMaterialColors();
        }

        private void InitializeBlendShapes()
        {
            if (SkinnedMeshRenderer == null || SkinnedMeshRenderer.sharedMesh == null) return;

            if (BlendShapeInfos == null)
            {
                BlendShapeInfos = new Dictionary<int, BlendShapeInfo>();
            }
            int blendShapeCount = SkinnedMeshRenderer.sharedMesh.blendShapeCount;

            for (int i = 0; i < blendShapeCount; i++)
            {
                string shapeName = SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                float shapeWeight = SkinnedMeshRenderer.GetBlendShapeWeight(i);

                if (!BlendShapeInfos.TryGetValue(i, out BlendShapeInfo info))
                {
                    info = new BlendShapeInfo(i, shapeName, shapeWeight, UpdateBlendShape);
                    BlendShapeInfos.Add(i, info);
                }
                else
                {
                    info.OnValueChanged -= UpdateBlendShape;
                }

                info.BlendShapeName = shapeName;
                info.BlendShapeValue = shapeWeight;
                info.OnValueChanged += UpdateBlendShape;
            }
        }

        protected void InitializeMaterials(Renderer r, List<MaterialInfo> materialInfos)
        {
            if (r == null) return;

            Material[] sharedMaterials = r.sharedMaterials;


            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material mat = sharedMaterials[i];
                if (mat == null) continue;

                MaterialInfo existingInfo = null;
                foreach (MaterialInfo info in materialInfos)
                {
                    if (info.Material == mat)
                    {
                        existingInfo = info;
                        break;
                    }
                }

                if (existingInfo == null)
                {
                    MaterialInfo newInfo = new MaterialInfo(mat, i)
                    {
                        MaterialIndex = i,
                        MaterialName = mat.name
                    };
                    materialInfos.Add(newInfo);
                }
                else
                {
                    existingInfo.MaterialIndex = i;
                    existingInfo.MaterialName = mat.name;
                }
            }

            foreach (MaterialInfo info in materialInfos)
            {
                if (info.PropertyBlock == null)
                {
                    info.PropertyBlock = new MaterialPropertyBlock();
                }
            }
        }

        private void CleanupMaterialInfos()
        {
            if (ObjectRenderer == null) return;

            Material[] validMaterials = ObjectRenderer.sharedMaterials;
            for (int i = MaterialInfos.Count - 1; i >= 0; i--)
            {
                MaterialInfo info = MaterialInfos[i];
                if (info.Material == null || !MaterialExistsInArray(validMaterials, info.Material))
                {
                    MaterialInfos.RemoveAt(i);
                }
            }
        }

        private bool MaterialExistsInArray(Material[] materials, Material target)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == target)
                {
                    return true;
                }
            }
            return false;
        }
        private void InitializeTextComponent()
        {
            if (ButtonText == null)
            {
                ButtonText = transform.FindChildRecursively<TextMeshPro>(nameof(ButtonText));
                if (ButtonText is null)
                {
                    ButtonText = GetComponentInChildren<TextMeshPro>();
                }
            }
        }

        protected virtual void SetButtonsOnThisGroup()
        {
            ButtonsOnThisGroup.Clear();
            if (Parent == null) return;

            Button3DSimple[] buttons = Parent.GetComponentsInChildren<Button3DSimple>();
            for (int i = 0; i < buttons.Length; i++)
            {
                ButtonsOnThisGroup.Add(buttons[i]);
            }
        }


        private void UpdateBlendShape()
        {
            if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh != null && SkinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                foreach (KeyValuePair<int, BlendShapeInfo> blendShapeInfo in BlendShapeInfos)
                {
                    if (blendShapeInfo.Value.BlendShapeName.Contains("Square"))
                    {
                        OriginalShapeValue = blendShapeInfo.Value.BlendShapeValue;

                    }
                    SkinnedMeshRenderer.SetBlendShapeWeight(blendShapeInfo.Key, blendShapeInfo.Value.BlendShapeValue);
                }

            }
        }

        private void CacheOriginalShapeValue()
        {
            if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh != null && SkinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                foreach (KeyValuePair<int, BlendShapeInfo> blendShapeInfo in BlendShapeInfos)
                {
                    if (blendShapeInfo.Value.BlendShapeName.Contains("Square"))
                    {
                        OriginalShapeValue = blendShapeInfo.Value.BlendShapeValue;

                    }

                }

            }
        }

        private void SetSquareShape()
        {
            if (SkinnedMeshRenderer == null || SkinnedMeshRenderer.sharedMesh == null) return;

            foreach (KeyValuePair<int, BlendShapeInfo> blendShapeInfo in BlendShapeInfos)
            {
                if (blendShapeInfo.Value.BlendShapeName.Contains("Square"))
                {
                    bool isActive = LastPressedButton != null && LastPressedButton.Equals(this);
                    float targetValue = isActive ? 100 : OriginalShapeValue;
                    AnimateBlendShapeWeight(blendShapeInfo.Key, targetValue, 0.5f, SkinnedMeshRenderer).Forget();
                    break;
                }
            }
        }


        private async UniTask AnimateBlendShapeWeight(int blendShapeKey, float targetValue, float duration, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
            {
                return;
            }

            float currentWeight = skinnedMeshRenderer.GetBlendShapeWeight(blendShapeKey);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newWeight = Mathf.Lerp(currentWeight, targetValue, elapsedTime / duration);

                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeKey, newWeight);
                }


                await UniTask.Yield();
            }

            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendShapeKey, targetValue);
            }

        }



        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!ButtonsOnThisGroup.Contains(this)) return;

            LastPressedButton = this;
            State = ButtonState.Pressed;
            ApplyMaterialColors();
            EventBus.Instance.Publish(new Button3DSimpleClickEvent(this));
            OnClick?.Invoke();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (State == ButtonState.Disabled) return;
            State = ButtonState.Highlighted;
            ApplyMaterialColors();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (State == ButtonState.Disabled) return;
            State = ButtonState.Normal;
            ApplyMaterialColors();
        }

        protected void ApplyMaterialColors()
        {
            if (MaterialInfos == null || ObjectRenderer == null) return;

            for (int index = 0; index < MaterialInfos.Count; index++)
            {
                MaterialInfo info = MaterialInfos[index];
                if (info.PropertyBlock == null)
                {
                    info.PropertyBlock = new MaterialPropertyBlock();
                }

                switch (State)
                {
                    case ButtonState.Normal:
                        info.PropertyBlock.SetColor(BaseColor, info.NormalColor);
                        info.PropertyBlock.SetColor(EmissionColor, Color.clear);
                        break;
                    case ButtonState.Highlighted:
                        info.PropertyBlock.SetColor(BaseColor, info.HighlightColor);
                        info.PropertyBlock.SetColor(EmissionColor, info.HighlightColor);
                        break;
                    case ButtonState.Pressed:
                        info.PropertyBlock.SetColor(BaseColor, info.PressedColor);
                        info.PropertyBlock.SetColor(EmissionColor, Color.clear);
                        break;
                    case ButtonState.Disabled:
                        info.PropertyBlock.SetColor(BaseColor, info.DisabledColor);
                        info.PropertyBlock.SetColor(EmissionColor, Color.clear);
                        break;
                }

                ObjectRenderer.SetPropertyBlock(info.PropertyBlock, info.MaterialIndex);
            }


            SetSquareShape();
        }


        [Button]
        public void SetButtonName()
        {
            if (ButtonText != null)
            {
                ButtonText.text = ButtonName;
            }
            
        }



        public virtual void SetInteractable(bool interactable, bool enableCollider = true)
        {
            Interactable = interactable;
            State = interactable? ButtonState.Normal: ButtonState.Disabled;
            ApplyMaterialColors();
        }
        
        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Button3DSimple other && Equals(other);
        }

        public bool Equals(Button3DSimple other)
        {
            if (other == null || gameObject == null || other.gameObject == null) return false;

            return guid == other.guid;
        }

        public static bool operator ==(Button3DSimple left, Button3DSimple right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(Button3DSimple left, Button3DSimple right)
        {
            return !(left == right);
        }

    }
}

