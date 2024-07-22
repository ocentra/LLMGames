using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CombineCsFilesEditor : OdinEditorWindow
{
    [FolderPath(AbsolutePath = true), ReadOnly]
    public string StartDirectory = "Assets";

    [ReadOnly]
    public string OutputFile = "Assets/CombinedFiles.txt";

    [ReadOnly]
    public List<string> SelectedFiles = new List<string>();
    private readonly Dictionary<string, bool> fileSelectionMap = new Dictionary<string, bool>();
    private readonly Dictionary<string, bool> folderFoldoutMap = new Dictionary<string, bool>();
    private Vector2 ScrollPosition { get; set; }
    private string ScriptFilePath { get; set; }

    private string SearchQuery { get; set; } = ""; // Search query
    private string SearchResult { get; set; } = null; // Search result path

    [MenuItem("Tools/Combine CS Files")]
    public static void ShowWindow()
    {
        GetWindow<CombineCsFilesEditor>("Combine CS Files").Show();
    }

    protected override void OnEnable()
    {
        ScriptFilePath = NormalizePath(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
        StartDirectory = NormalizePath(PlayerPrefs.GetString("CombineCSFilesEditor_startDirectory", "Assets"));
        OutputFile = NormalizePath(PlayerPrefs.GetString("CombineCSFilesEditor_outputFile", "Assets/CombinedFiles.txt"));
        RefreshFileList();
    }

    private void OpenSaveFileDialog()
    {
        string path = EditorUtility.SaveFilePanel("Select Output File", "Assets", "CombinedFiles.txt", "txt");
        if (!string.IsNullOrEmpty(path))
        {
            OutputFile = NormalizePath(path);
            Repaint();
        }
    }

    public void CombineFiles()
    {
        try
        {
            IEnumerable<string> csFiles = fileSelectionMap.Where(f => f.Value).Select(f => f.Key);

            IEnumerable<string> enumerable = csFiles as string[] ?? csFiles.ToArray();
            if (!enumerable.Any())
            {
                csFiles = Directory.GetFiles(StartDirectory, "*.cs", SearchOption.AllDirectories);
            }

            using (StreamWriter writer = new(OutputFile))
            {
                foreach (var file in enumerable)
                {
                    writer.WriteLine($"// File: {Path.GetFileName(file)}\n");
                    writer.WriteLine(File.ReadAllText(file));
                    writer.WriteLine("\n// ---\n"); // Separator between files
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"All .cs files have been combined into {OutputFile}", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    protected override void OnDisable()
    {
        PlayerPrefs.SetString("CombineCSFilesEditor_startDirectory", StartDirectory);
        PlayerPrefs.SetString("CombineCSFilesEditor_outputFile", OutputFile);
    }

    private void RefreshFileList()
    {
        fileSelectionMap.Clear();
        folderFoldoutMap.Clear();
        SelectedFiles.Clear();
        LoadFilesRecursively(StartDirectory);
    }

    private bool LoadFilesRecursively(string directory)
    {
        bool hasCsFiles = false;

        try
        {
            string[] directories = Directory.GetDirectories(directory);
            string[] files = Directory.GetFiles(directory, "*.cs");

            foreach (string file in files)
            {
                fileSelectionMap[NormalizePath(file)] = false;
                hasCsFiles = true;
            }

            foreach (string dir in directories)
            {
                folderFoldoutMap[NormalizePath(dir)] = false;
                if (LoadFilesRecursively(NormalizePath(dir)))
                {
                    hasCsFiles = true;
                }
            }
        }
        catch (System.UnauthorizedAccessException)
        {
            // Skip directories that we cannot access
        }

        return hasCsFiles;
    }

    private bool manualSelectionFoldout = true; // Variable to manage the foldout state

    protected override void OnImGUI()
    {
        using (var propertyTree = PropertyTree.Create(this))
        {
            GUILayout.BeginVertical();
            foreach (InspectorProperty property in propertyTree.EnumerateTree(false))
            {
                if (property.Name == nameof(OutputFile))
                {
                    GUILayout.BeginHorizontal();
                    property.Draw();
                    if (GUILayout.Button("Select Output File"))
                    {
                        OpenSaveFileDialog();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    property.Draw();
                }
            }

            GUILayout.BeginHorizontal();
            SearchQuery = GUILayout.TextField(SearchQuery, GUILayout.Width(200));
            if (GUILayout.Button("Search"))
            {
                SearchResult = FindFileAndDisplay(SearchQuery);
                if (SearchResult == null)
                {
                    EditorUtility.DisplayDialog("Search Result", $"No file named {SearchQuery} found.", "OK");
                }
                Repaint();
            }
            GUILayout.EndHorizontal();

            manualSelectionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(manualSelectionFoldout, "Manual File Selection");
            if (manualSelectionFoldout)
            {
                ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, false, false, GUILayout.ExpandHeight(true));
                DisplayFileTree(StartDirectory);
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(10);

            if (GUILayout.Button("Combine Files", GUILayout.Height(40)))
            {
                CombineFiles();
            }

            GUILayout.EndVertical();
        }
    }

    private string FindFileAndDisplay(string filename)
    {
        string foundFile = fileSelectionMap.Keys
            .FirstOrDefault(file => Path.GetFileNameWithoutExtension(file)
                .Equals(filename, System.StringComparison.OrdinalIgnoreCase));

        if (foundFile != null)
        {
            foreach (var key in folderFoldoutMap.Keys.ToList())
            {
                folderFoldoutMap[key] = false;
            }

            var directoriesToExpand = new HashSet<string>();

            string parentDirectory = NormalizePath(Path.GetDirectoryName(foundFile));
            while (!string.IsNullOrEmpty(parentDirectory) && parentDirectory != NormalizePath(StartDirectory))
            {
                directoriesToExpand.Add(parentDirectory);
                parentDirectory = NormalizePath(Directory.GetParent(parentDirectory)?.FullName);
            }

            directoriesToExpand.Add(NormalizePath(StartDirectory));



            foreach (var dir in directoriesToExpand)
            {
                folderFoldoutMap[dir] = true;
            }

            SearchResult = foundFile;

    
            string directoriesToExpandLog = "Directories to expand:\n" + string.Join("\n", directoriesToExpand);
            string folderFoldoutMapLog = "State of folderFoldoutMap:\n" + string.Join("\n", folderFoldoutMap.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

            //Debug.Log(directoriesToExpandLog);
            //Debug.Log(folderFoldoutMapLog);
        }

        return foundFile;
    }

    private void DisplayFileTree(string directory)
    {
        directory = NormalizePath(directory);
        if (!DirectoryContainsCsFiles(directory)) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        bool foldout = folderFoldoutMap.ContainsKey(directory) && folderFoldoutMap[directory];
        foldout = EditorGUILayout.Foldout(foldout, Path.GetFileName(directory), true);
        folderFoldoutMap[directory] = foldout;

        bool folderSelected = IsFolderSelected(directory);
        EditorGUI.BeginChangeCheck();
        bool newFolderSelection = EditorGUILayout.Toggle(folderSelected, GUILayout.Width(20));
        if (EditorGUI.EndChangeCheck())
        {
            SetFolderSelection(directory, newFolderSelection);
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        if (foldout)
        {
            try
            {
                var directories = Directory.GetDirectories(directory);
                var files = Directory.GetFiles(directory, "*.cs");

                foreach (var dir in directories)
                {
                    DisplayFileTree(NormalizePath(dir));
                }

                foreach (var file in files)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20); // Indent files

                    bool fileSelected = fileSelectionMap.ContainsKey(file) && fileSelectionMap[file];
                    EditorGUI.BeginChangeCheck();
                    bool newFileSelection = EditorGUILayout.Toggle(fileSelected, GUILayout.Width(20));
                    if (EditorGUI.EndChangeCheck())
                    {
                        fileSelectionMap[file] = newFileSelection;
                        UpdateSelectedFiles(file, newFileSelection);
                        Repaint();
                    }

                    var style = new GUIStyle(EditorStyles.label);
                    if (SearchResult != null && NormalizePath(file) == NormalizePath(SearchResult))
                    {
                        style.normal.textColor = Color.yellow; // Highlight searched file
                    }
                    else
                    {
                        style.normal.textColor = fileSelected ? Color.green : EditorStyles.label.normal.textColor;
                    }

                    if (GUILayout.Button(Path.GetFileName(file), style))
                    {
                        bool newState = !fileSelectionMap[file];
                        fileSelectionMap[file] = newState;
                        UpdateSelectedFiles(file, newState);
                        Repaint();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                // Skip directories that we cannot access
            }
        }

        EditorGUILayout.EndVertical();
    }


    private bool DirectoryContainsCsFiles(string directory)
    {
        directory = NormalizePath(directory);

        if (Directory.GetFiles(directory, "*.cs").Any())
        {
            return true;
        }

        foreach (var subdirectory in Directory.GetDirectories(directory))
        {
            if (DirectoryContainsCsFiles(NormalizePath(subdirectory)))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsFolderSelected(string directory)
    {
        directory = NormalizePath(directory);
        folderFoldoutMap.TryAdd(directory, false);

        var directories = Directory.GetDirectories(directory);
        var files = Directory.GetFiles(directory, "*.cs");

        bool hasSelected = false;
        bool hasUnselected = false;

        foreach (var file in files)
        {
            if (fileSelectionMap.ContainsKey(file) && fileSelectionMap[file])
            {
                hasSelected = true;
            }
            else
            {
                hasUnselected = true;
            }
        }

        foreach (var dir in directories)
        {
            bool subFolderSelected = IsFolderSelected(NormalizePath(dir));
            if (subFolderSelected)
            {
                hasSelected = true;
            }
            else
            {
                hasUnselected = true;
            }
        }

        if (hasSelected && hasUnselected)
        {
            EditorGUI.showMixedValue = true;
        }
        else
        {
            EditorGUI.showMixedValue = false;
        }

        return hasSelected && !hasUnselected;
    }

    private void SetFolderSelection(string directory, bool isSelected)
    {
        directory = NormalizePath(directory);
        var directories = Directory.GetDirectories(directory);
        var files = Directory.GetFiles(directory, "*.cs");

        foreach (var file in files)
        {
            fileSelectionMap[NormalizePath(file)] = isSelected;
            UpdateSelectedFiles(NormalizePath(file), isSelected);
        }

        foreach (var dir in directories)
        {
            fileSelectionMap[NormalizePath(dir)] = isSelected;
            SetFolderSelection(NormalizePath(dir), isSelected);
        }

        UpdateParentFolderState(directory);
    }

    private void UpdateParentFolderState(string directory)
    {
        directory = NormalizePath(directory);
        var parentDirectoryInfo = Directory.GetParent(directory);
        if (parentDirectoryInfo == null) return;

        string parentDirectory = NormalizePath(parentDirectoryInfo.FullName.Replace(Application.dataPath.Replace("\\", "/"), "Assets"));

        if (Directory.Exists(parentDirectory))
        {
            IsFolderSelected(parentDirectory);
            UpdateParentFolderState(parentDirectory);
        }
    }

    private void UpdateSelectedFiles(string file, bool isSelected)
    {
        file = NormalizePath(file);
        if (isSelected)
        {
            if (!SelectedFiles.Contains(file))
            {
                SelectedFiles.Add(file);
            }
        }
        else
        {
            if (SelectedFiles.Contains(file))
            {
                SelectedFiles.Remove(file);
            }
        }
    }

    private string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }
}
