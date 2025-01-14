using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class BlendShapeInfo
    {
        [ReadOnly, ShowInInspector]
        public string BlendShapeName;

        [ShowInInspector, Range(0, 100), OnValueChanged(nameof(OnPercentageChanged))]
        public float BlendShapeValue;


        protected int Index;

        public event Action OnValueChanged;

        public BlendShapeInfo(int index, string blendShapeName, float blendShapeValue, Action onValueChanged)
        {
            BlendShapeName = blendShapeName;
            BlendShapeValue = blendShapeValue;
            Index = index;
            OnValueChanged = onValueChanged;
        }

        private void OnPercentageChanged()
        {
            OnValueChanged?.Invoke();
        }
    }
}