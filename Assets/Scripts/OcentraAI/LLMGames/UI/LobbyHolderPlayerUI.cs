using log4net.ObjectRenderer;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    public class LobbyHolderPlayerUI : MonoBehaviourBase<LobbyHolderPlayerUI>
    {
        [SerializeField] protected Button3DSimple ButtonQuestion;
        [SerializeField] protected MeshRenderer Icon;
        [SerializeField] protected TextMeshPro PlayerName;
        [SerializeField] protected List<MaterialInfo> MaterialInfos;
        [SerializeField, OnValueChanged(nameof(ApplyMaterialColors))]
        protected ButtonState State;

        public const string BaseColor = "_BaseColor";
        public const string EmissionColor = "_EmissionColor";
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnValidate()
        {
            Init();
            base.OnValidate();

        }

        protected void Init()
        {
            ButtonQuestion = transform.FindChildRecursively<Button3DSimple>(nameof(ButtonQuestion));
            PlayerName = transform.FindChildRecursively<TextMeshPro>(nameof(PlayerName));
            Icon = transform.FindChildRecursively<MeshRenderer>(nameof(Icon));

            if (MaterialInfos == null)
            {
                MaterialInfos = new List<MaterialInfo>();
            }

            if (Icon != null)
            {
                InitializeMaterials(Icon, MaterialInfos);
            }


        }


        public void SetData(Sprite icon, string playerName)
        {
            PlayerName.text = playerName;

            if (Icon != null && icon != null)
            {
                foreach (MaterialInfo materialInfo in MaterialInfos)
                {
                    if (materialInfo.PropertyBlock == null)
                    {
                        materialInfo.PropertyBlock = new MaterialPropertyBlock();
                    }

                    materialInfo.PropertyBlock.SetTexture("_MainTex", icon.texture);
                    Icon.SetPropertyBlock(materialInfo.PropertyBlock, materialInfo.MaterialIndex);
                }
            }
        }


        protected void ApplyMaterialColors()
        {
            if (MaterialInfos == null || Icon == null) return;

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

                Icon.SetPropertyBlock(info.PropertyBlock, info.MaterialIndex);
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
    }
}