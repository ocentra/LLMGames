using ThreeCardBrag;
using UnityEditor;
using UnityEngine;

public class CardAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            Card card = AssetDatabase.LoadAssetAtPath<Card>(assetPath);
            if (card != null)
            {
                string resourcesPath = "Assets/Resources/Cards/";
                if (!assetPath.StartsWith(resourcesPath))
                {
                    string assetName = System.IO.Path.GetFileName(assetPath);
                    string newAssetPath = resourcesPath + assetName;

                    // Ensure the directory exists
                    System.IO.Directory.CreateDirectory(resourcesPath);

                    // Move the asset to the new path
                    AssetDatabase.MoveAsset(assetPath, newAssetPath);
                    Debug.Log($"Moved card to: {newAssetPath}");
                }
            }
        }
    }
}