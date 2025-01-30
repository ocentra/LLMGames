using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class MaterialInfo
    {
        [HideInInspector]
        public string MaterialName;

        [FoldoutGroup("Material Details", true)]
        [Tooltip("The material applied to this part of the 3D object.")]
        public Material Material;

        [FoldoutGroup("Material Details", true)]
        [ReadOnly, ShowInInspector]
        public int MaterialIndex;

        [FoldoutGroup("Colors", true)]
        [ColorUsage(true, true)]
        public Color NormalColor = Color.white;

        [FoldoutGroup("Colors", true)]
        [ColorUsage(true, true)]
        public Color HighlightColor = Color.yellow;

        [FoldoutGroup("Colors", true)]
        [ColorUsage(true, true)]
        public Color PressedColor = Color.gray;

        [FoldoutGroup("Colors", true)]
        [ColorUsage(true, true)]
        public Color DisabledColor = Color.red;

        [HideInInspector]
        public MaterialPropertyBlock PropertyBlock;

        public MaterialInfo(Material material, int materialIndex)
        {
            Material = material;
            MaterialName = material.name;
            MaterialIndex = materialIndex;
            PropertyBlock = new MaterialPropertyBlock();
        }

        [FoldoutGroup("Colors", true)]
        [Button("Copy Colors")]
        private void CopyColors()
        {
            ColorData colorData = new ColorData
            {
                NormalColor = NormalColor,
                HighlightColor = HighlightColor,
                PressedColor = PressedColor,
                DisabledColor = DisabledColor
            };

            string copiedColors = JsonUtility.ToJson(colorData);
            GUIUtility.systemCopyBuffer = copiedColors;
        }

        [FoldoutGroup("Colors", true)]
        [Button("Paste Colors")]
        private void PasteColors()
        {
            string json = GUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(json))
            {
                ColorData colorData = JsonUtility.FromJson<ColorData>(json);
                NormalColor = colorData.NormalColor;
                HighlightColor = colorData.HighlightColor;
                PressedColor = colorData.PressedColor;
                DisabledColor = colorData.DisabledColor;
            }

        }

        [Serializable]
        private class ColorData
        {
            public Color NormalColor;
            public Color HighlightColor;
            public Color PressedColor;
            public Color DisabledColor;
        }
    }
}
