using System.IO;
using UnityEditor;
using UnityEngine;

public class CombineCSFilesEditor : EditorWindow
{
    private string startDirectory = "Assets";
    private string outputFile = "Assets/CombinedFiles.txt";

    [MenuItem("Tools/Combine CS Files")]
    public static void ShowWindow()
    {
        GetWindow<CombineCSFilesEditor>("Combine CS Files");
    }

    private void OnGUI()
    {
        GUILayout.Label("Combine .cs Files into a Single Document", EditorStyles.boldLabel);

        GUILayout.Label("Start Directory");
        if (GUILayout.Button("Select Start Directory"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Start Directory", startDirectory, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                startDirectory = selectedPath.Replace(Application.dataPath, "Assets");
            }
        }
        EditorGUILayout.TextField(startDirectory);

        GUILayout.Label("Output File");
        if (GUILayout.Button("Select Output File"))
        {
            string selectedPath = EditorUtility.SaveFilePanel("Select Output File", "", "CombinedFiles", "txt");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                outputFile = selectedPath.Replace(Application.dataPath, "Assets");
            }
        }
        EditorGUILayout.TextField(outputFile);

        if (GUILayout.Button("Combine Files"))
        {
            CombineFiles();
        }
    }

    private void CombineFiles()
    {
        try
        {
            // Recursively get all .cs files
            var csFiles = Directory.GetFiles(startDirectory, "*.cs", SearchOption.AllDirectories);

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                foreach (var file in csFiles)
                {
                    writer.WriteLine($"// File: {Path.GetFileName(file)}\n");
                    writer.WriteLine(File.ReadAllText(file));
                    writer.WriteLine("\n// ---\n"); // Separator between files
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"All .cs files have been combined into {outputFile}", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}
