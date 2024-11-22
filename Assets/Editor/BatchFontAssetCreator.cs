using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;

public class BatchFontAssetCreator : EditorWindow
{
    private string folderPath = "Assets/"; // Default folder path within the Unity project

    [MenuItem("Tools/Batch Font Asset Creator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BatchFontAssetCreator), false, "Batch Font Asset Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch TextMesh Pro Font Asset Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Select Font Folder"))
        {
            // Open folder panel to select a folder
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder with Fonts", Application.dataPath, "");

            // Ensure the selected folder is within the Assets directory
            if (!string.IsNullOrEmpty(selectedPath) && selectedPath.Contains(Application.dataPath))
            {
                folderPath = "Assets" + selectedPath.Replace(Application.dataPath, "").Replace("\\", "/");
                Debug.Log("Selected folder: " + folderPath);
            }
            else
            {
                Debug.LogWarning("Please select a folder inside the Assets directory.");
                folderPath = "Assets/";
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Selected Folder: " + folderPath, EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create TextMesh Pro Font Assets"))
        {
            CreateFontAssets();
        }
    }

    private void CreateFontAssets()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("No folder selected!");
            return;
        }

        // Search for all TTF files in the selected folder and its subfolders
        string[] fontFiles = Directory.GetFiles(folderPath, "*.ttf", SearchOption.AllDirectories);

        if (fontFiles.Length == 0)
        {
            Debug.LogError("No .ttf font files found in the selected folder.");
            return;
        }

        foreach (string fontFile in fontFiles)
        {
            // Generate path for the TextMesh Pro font asset
            string fontAssetPath = fontFile.Replace(".ttf", "_TMP.asset");

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);

            if (fontAsset == null)
            {
                // Load the .ttf font file
                Font font = AssetDatabase.LoadAssetAtPath<Font>(fontFile);
                if (font == null)
                {
                    Debug.LogError("Could not load font at path: " + fontFile);
                    continue;
                }

                // Create TextMesh Pro font asset
                fontAsset = TMP_FontAsset.CreateFontAsset(font);
                AssetDatabase.CreateAsset(fontAsset, fontAssetPath);
                AssetDatabase.SaveAssets();

                Debug.Log("Created TextMesh Pro Font Asset: " + fontAssetPath);
            }
            else
            {
                Debug.Log("Font Asset already exists for: " + fontFile);
            }
        }

        Debug.Log("Font Asset creation process completed.");
        AssetDatabase.Refresh();
    }
}
