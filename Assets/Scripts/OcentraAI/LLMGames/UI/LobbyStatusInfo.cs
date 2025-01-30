using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class LobbyStatusInfo
    {
        [HideInInspector]
        public string MaterialName;

        [ReadOnly]
        public string StatusTypeName;

        [FoldoutGroup("Material Details", true)]
        [Tooltip("The material applied to this part of the 3D object.")]
        public Material Material;

        [FoldoutGroup("Material Details", true)]
        [ReadOnly, ShowInInspector]
        public int MaterialIndex;

        [FoldoutGroup("Colors", true)]
        [ColorUsage(true, true)]
        public Color ActiveColor = Color.green;

        [FoldoutGroup("Colors", true)]
        [ColorUsage(true, true)]
        public Color InActiveColor = Color.red;

        [HideInInspector]
        public MaterialPropertyBlock PropertyBlock;

        public LobbyStatusInfo(Material material, int materialIndex, string statusTypeName)
        {
            Material = material;
            MaterialName = material.name;
            MaterialIndex = materialIndex;
            StatusTypeName = statusTypeName;
            PropertyBlock = new MaterialPropertyBlock();
        }

        [FoldoutGroup("Colors", true)]
        [Button("Copy Colors")]
        private void CopyColors()
        {
            ColorData colorData = new ColorData
            {
                ActiveColor = ActiveColor,
                InActiveColor = InActiveColor
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
                ActiveColor = colorData.ActiveColor;
                InActiveColor = colorData.InActiveColor;
               
            }

        }

        [Serializable]
        private class ColorData
        {
            public Color ActiveColor;
            public Color InActiveColor;
        }
    }
}
