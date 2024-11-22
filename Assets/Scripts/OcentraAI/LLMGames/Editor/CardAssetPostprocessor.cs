using OcentraAI.LLMGames.Scriptable;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CardAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            Card card = AssetDatabase.LoadAssetAtPath<Card>(assetPath);
            if (card != null)
            {
                string resourcesPath = "Assets/Resources/Cards/";
                if (!assetPath.StartsWith(resourcesPath))
                {
                    string assetName = Path.GetFileName(assetPath);
                    string newAssetPath = resourcesPath + assetName;

                    // Ensure the directory exists
                    Directory.CreateDirectory(resourcesPath);

                    // Move the asset to the new path
                    AssetDatabase.MoveAsset(assetPath, newAssetPath);
                    Debug.Log($"Moved card to: {newAssetPath}");
                }
            }
        }
    }
}