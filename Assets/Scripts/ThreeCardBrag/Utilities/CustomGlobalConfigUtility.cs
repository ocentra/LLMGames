using Sirenix.Utilities.Editor;
using System;
using System.IO;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace ThreeCardBrag.Utilities
{
    public static class CustomGlobalConfigUtility<T> where T : ScriptableObject
    {
        private static T instance;


        public static bool HasInstanceLoaded => instance != null;

        public static T GetInstance(
          string defaultAssetFolderPath,
          string defaultFileNameWithoutExtension = null)
        {
            if (instance == null)
            {
                LoadInstanceIfAssetExists(defaultAssetFolderPath, defaultFileNameWithoutExtension);
                T instance1 = instance;
                string str1 = defaultFileNameWithoutExtension ?? TypeExtensions.GetNiceName(typeof(T));
                string str2 = Application.dataPath + "/" + defaultAssetFolderPath + str1 + ".asset";
                if (instance1 == null && EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
                {
                    Debug.LogWarning(defaultAssetFolderPath + str1 + ".asset was prevented from being generated because the PREVENT_SIRENIX_FILE_GENERATION key was defined in Unity's EditorPrefs.");
                    instance = ScriptableObject.CreateInstance<T>();
                    return instance;
                }
                if (instance1 == null && File.Exists(str2) && EditorSettings.serializationMode == SerializationMode.ForceText)
                {
                    if (AssetScriptGuidUtility.TryUpdateAssetScriptGuid(str2, typeof(T)))
                    {
                        Debug.Log("Could not load config asset at first, but successfully detected forced text asset serialization, and corrected the config asset m_Script guid.");
                        LoadInstanceIfAssetExists(defaultAssetFolderPath, defaultFileNameWithoutExtension);
                        instance1 = instance;
                    }
                    else
                        Debug.LogWarning("Could not load config asset, and failed to auto-correct config asset m_Script guid.");
                }
                if (instance1 == null)
                {
                    instance1 = ScriptableObject.CreateInstance<T>();
                    string path1 = defaultAssetFolderPath;
                    if (!path1.StartsWith("Assets/"))
                        path1 = "Assets/" + path1.TrimStart('/');
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(new DirectoryInfo(path1).FullName);
                        AssetDatabase.Refresh();
                    }
                    string str3 = str1;
                    string path2 = !defaultAssetFolderPath.StartsWith("Assets/") ? "Assets/" + defaultAssetFolderPath + str3 + ".asset" : defaultAssetFolderPath + str3 + ".asset";
                    if (File.Exists(str2))
                    {
                        Debug.LogWarning("Could not load config asset of type " + str3 + " from project path '" + path2 + "', but an asset file already exists at the path, so could not create a new asset either. The config asset for '" + str3 + "' has been lost, probably due to an invalid m_Script guid. Set forced text serialization in Edit -> Project Settings -> Editor -> Asset Serialization -> Mode and trigger a script reload to allow Odin to auto-correct this.");
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(instance1, path2);
                        AssetDatabase.SaveAssets();
                        instance = instance1;
                        if (instance1 is IGlobalConfigEvents globalConfigEvents)
                            globalConfigEvents.OnConfigAutoCreated();
                        EditorUtility.SetDirty(instance1);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                instance = instance1;
                if (instance is IGlobalConfigEvents instance2)
                    instance2.OnConfigInstanceFirstAccessed();
            }
            return instance;
        }

        internal static void LoadInstanceIfAssetExists(
            string assetPath,
            string defaultFileNameWithoutExtension = null)
        {
            string str1 = defaultFileNameWithoutExtension ?? TypeExtensions.GetNiceName(typeof(T));
            if (assetPath.Contains("/resources/", StringComparison.OrdinalIgnoreCase))
            {
                string str2 = assetPath;
                int num = str2.LastIndexOf("/resources/", StringComparison.OrdinalIgnoreCase);
                if (num >= 0)
                    str2 = str2.Substring(num + "/resources/".Length);
                string str3 = str1;
                instance = Resources.Load<T>(str2 + str3);
            }
            else
            {
                string str4 = str1;
                instance = AssetDatabase.LoadAssetAtPath<T>(assetPath + str4 + ".asset");
                if (instance == null)
                    instance = AssetDatabase.LoadAssetAtPath<T>("Assets/" + assetPath + str4 + ".asset");
            }
            if (!(instance == null))
                return;
            string[] assets = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (assets.Length == 0)
                return;
            instance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }
    }


}
