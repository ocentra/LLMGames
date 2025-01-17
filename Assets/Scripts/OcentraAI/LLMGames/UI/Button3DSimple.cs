using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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

        [SerializeField, FoldoutGroup("ButtonInfo/Material Info Configuration", false)]
        protected ButtonState State;

        [SerializeField, FoldoutGroup("ButtonInfo/Material Info Configuration", false)]
        protected IButton3DSimple LastPressedButton;



        public const string BaseColor = "_BaseColor";
        public const string EmissionColor = "_EmissionColor";

        private readonly Guid guid = Guid.NewGuid();

        protected override void OnValidate()
        {
            Init();
            ApplyMaterialColors();
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
            if (!ButtonsOnThisGroup.Contains(e.Button3DSimple) || (Button3DSimple)e.Button3DSimple == this)
            {
                return;
            }

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
                ObjectRenderer = GetComponentInChildren<Renderer>();
            }

            if (ObjectRenderer == null)
            {
                Debug.LogWarning("No Renderer found on the object.");
                return;
            }



            if (ObjectRenderer != null)
            {

                if (ObjectRenderer is SkinnedMeshRenderer smr)
                {
                    SkinnedMeshRenderer = smr;

                    if (SkinnedMeshRenderer.sharedMesh != null)
                    {
                        int count = SkinnedMeshRenderer.sharedMesh.blendShapeCount;

                        for (int i = 0; i < count; i++)
                        {
                            string blendShapeName = SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                            float blendShapeWeight = SkinnedMeshRenderer.GetBlendShapeWeight(i);

                            BlendShapeInfo newInfo = new BlendShapeInfo(
                                i,
                                blendShapeName,
                                blendShapeWeight,
                                UpdateBlendShape
                            );

                            if (BlendShapeInfos.TryGetValue(i, out BlendShapeInfo info))
                            {
                                info.BlendShapeName = blendShapeName;
                                info.OnValueChanged += UpdateBlendShape;
                            }

                            BlendShapeInfos.TryAdd(i, newInfo);
                        }


                    }
                }



                Material[] sharedMaterials = ObjectRenderer.sharedMaterials;
                MaterialInfos ??= new List<MaterialInfo>();

                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    Material mat = sharedMaterials[i];
                    if (mat == null) continue;

                    MaterialInfo existingInfo = MaterialInfos.Find(m => m.MaterialName == mat.name);
                    if (existingInfo == null)
                    {
                        MaterialInfos.Add(new MaterialInfo(mat, i));
                    }
                    else if (existingInfo.PropertyBlock == null)
                    {
                        existingInfo.PropertyBlock = new MaterialPropertyBlock();
                    }
                }
            }

            MaterialInfos.RemoveAll(m => GetMaterialIndexByName(m.MaterialName) == -1);

            if (ButtonText == null)
            {
                ButtonText = transform.FindChildRecursively<TextMeshPro>(nameof(ButtonText));
            }
            if (ButtonText == null)
            {
                ButtonText = GetComponentInChildren<TextMeshPro>();
            }

            SetButtonName(ButtonName);
        }

        protected virtual void SetButtonsOnThisGroup()
        {
            if (Parent != null)
            {
                ButtonsOnThisGroup = Parent.GetComponentsInChildren<Button3DSimple>().ToList();
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
            if (SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh != null && SkinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
            {
                foreach (KeyValuePair<int, BlendShapeInfo> blendShapeInfo in BlendShapeInfos)
                {
                    if (blendShapeInfo.Value.BlendShapeName.Contains("Square"))
                    {
                        float targetValue = (Button3DSimple)LastPressedButton == this ? 100 : OriginalShapeValue;
                        AnimateBlendShapeWeight(blendShapeInfo.Key, targetValue, 0.5f, SkinnedMeshRenderer).Forget();
                        break;
                    }
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
            if (!ButtonsOnThisGroup.Contains(this))
            {
                return;
            }

            LastPressedButton = this;
            State = ButtonState.Pressed;
            ApplyMaterialColors();
            EventBus.Instance.Publish(new Button3DSimpleClickEvent(this));

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

            foreach (MaterialInfo info in MaterialInfos)
            {
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

        private int GetMaterialIndexByName(string materialName)
        {
            Material[] sharedMaterials = ObjectRenderer.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                if (sharedMaterials[i] != null && sharedMaterials[i].name == materialName)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SetButtonName(string newName = "")
        {
            if (newName != null)
            {
                ButtonName = newName;
                if (ButtonText != null)
                {
                    ButtonText.text = ButtonName;
                }
            }


        }



        public virtual void SetInteractable(bool interactable, bool enableCollider = true)
        {
            Interactable = interactable;
            State = ButtonState.Disabled;
        }

        public enum ButtonState
        {
            Normal,
            Highlighted,
            Pressed,
            Disabled
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

