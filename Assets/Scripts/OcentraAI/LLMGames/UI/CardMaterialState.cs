using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
[ExecuteAlways]
public class CardMaterialState
{
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
    public Material[] OriginalMaterials = Array.Empty<Material>();

    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
    public Material[] RuntimeMaterials = Array.Empty<Material>();

    [ReadOnly] public Color OriginalColor = Color.white;
    [ReadOnly] public MeshRenderer Renderer = null;

    public CardMaterialState(MeshRenderer renderer)
    {

        if (!Application.isPlaying && (renderer == null || !renderer)) return;

        Renderer = renderer;
        if (Renderer == null) return;


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
                MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();
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

        {
            OriginalMaterials = renderer.sharedMaterials;
        }



        if (OriginalMaterials is { Length: > 0 })
        {
            RuntimeMaterials = new Material[OriginalMaterials.Length];

            for (int i = 0; i < OriginalMaterials.Length; i++)
            {
                if (OriginalMaterials[i] == null) continue;

                RuntimeMaterials[i] = new Material(OriginalMaterials[i])
                {
                    name = $"{renderer.name}_{OriginalMaterials[i].name}"
                };
            }

            if (RuntimeMaterials.Length > 0 && RuntimeMaterials[0] != null && RuntimeMaterials[0].HasProperty("_BaseColor"))
            {
                OriginalColor = RuntimeMaterials[0].GetColor("_BaseColor");
            }
        }


    }




    public void ApplyMaterials(bool useRuntime)
    {
        if (Renderer == null) return;

        Renderer.sharedMaterials = useRuntime ? RuntimeMaterials.ToArray() : OriginalMaterials.ToArray();
    }

    public void Dispose()
    {
        if (RuntimeMaterials == null) return;
        foreach (Material material in RuntimeMaterials)
        {
            if (material != null)
            {
                Object.DestroyImmediate(material);
            }
        }
        RuntimeMaterials = null;
    }
}