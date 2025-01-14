using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [Serializable]
    public class ChildElement<TData>
    {
        [ShowInInspector, ReadOnly] public Transform Child;
        [ShowInInspector, ReadOnly] public BoxCollider BoxCollider;
        [ShowInInspector, ReadOnly] public readonly Vector3 ColliderOriginalSize;
        [ShowInInspector, ReadOnly] public readonly Vector3 OriginalSize;
        [ShowInInspector, ReadOnly] public readonly int InstanceID;
        [ShowInInspector, ReadOnly] public float ElementSizeX;
        [ShowInInspector, ReadOnly] public float ScaleFactor;

        [ShowInInspector, ReadOnly] public int Index;
        [ShowInInspector, ReadOnly] public int InstanceId;

        [ShowInInspector, ReadOnly,HideLabel] public TData FilterContextData { get; private set; }
        public ChildElement() { }
        public ChildElement(Transform child, int index, BoxCollider boxCollider, TData filterContextData)
        {
            Child = child;
            BoxCollider = boxCollider;
            InstanceID = child.gameObject.GetInstanceID();
            ColliderOriginalSize = boxCollider != null ? boxCollider.size : Vector3.one;
            OriginalSize = child.localScale;
            Index = index;
            FilterContextData = filterContextData;
        }


        public void SetActive(bool active)
        {
            Child.gameObject.SetActive(active);
        }

        public void Resize(float maxFactor)
        {
            if (BoxCollider == null) return;

            ScaleFactor = maxFactor / ColliderOriginalSize.y;

            Child.localScale = Vector3.one * ScaleFactor;

            ElementSizeX = ColliderOriginalSize.x * ScaleFactor;

        }

        public void Update(Transform child, BoxCollider boxCollider, TData filterContextData)
        {
            Child = child;
            BoxCollider = boxCollider;
            FilterContextData = filterContextData;
          
        }

       
    }
}