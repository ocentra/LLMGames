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


        [HideInInspector] public MaterialPropertyBlock PropertyBlock;

        public MaterialInfo(Material material, int materialIndex)
        {
            Material = material;
            MaterialName = material.name;
            MaterialIndex = materialIndex;
            PropertyBlock = new MaterialPropertyBlock();
        }
    }
}