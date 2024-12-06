using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace OcentraAI.LLMGames.Extensions
{
    public static class TransformExtensions
    {

        public static string GetFullPath(this Transform transform)
        {
            if (transform == null) return "null";

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = $"{transform.name}/{path}";
            }
            return path;
        }

        /// <summary>
        ///     Recursively searches for a child Transform by name.
        /// </summary>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <param name="childName">The name of the child Transform to search for.</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive (default is false).</param>
        /// <returns>The found child Transform, or null if not found.</returns>
        public static Transform FindChildRecursively(this Transform parent, string childName,
            bool caseSensitive = false)
        {
            childName = childName.Trim(); 

            foreach (Transform child in parent)
            {
                string trimmedChildName = child.name.Trim();

                if (caseSensitive)
                {
                    if (trimmedChildName == childName)
                    {
                        return child;
                    }
                }
                else
                {
                    if (string.Equals(trimmedChildName, childName, StringComparison.OrdinalIgnoreCase))
                    {
                        return child;
                    }
                }

                Transform found = child.FindChildRecursively(childName, caseSensitive);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }


        /// <summary>
        ///     Recursively searches for a child component of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <returns>The found component of type T, or null if not found.</returns>
        public static T FindChildRecursively<T>(this Transform parent) where T : Component
        {
            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                T result = child.FindChildRecursively<T>();
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }


        public static void DestroyChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            foreach (Transform child in children)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }


        /// <summary>
        ///     Recursively searches for a child component of a specified type with a specified name.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <param name="name">The name of the child Transform to search for.</param>
        /// <param name="ignoreCase">ignoreCase </param>
        /// <returns>The found component of type T with the specified name, or null if not found.</returns>
        public static T FindChildRecursively<T>(this Transform parent, string name, bool ignoreCase = false)
            where T : Component
        {
            foreach (Transform child in parent)
            {
                bool equals = ignoreCase
                    ? string.Equals(child.name, name, StringComparison.CurrentCultureIgnoreCase)
                    : child.name == name;

                if (equals)
                {
                    T component = child.GetComponent<T>();
                    if (component != null)
                    {
                        return component;
                    }
                }

                T result = child.FindChildRecursively<T>(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     Attempts to find a GameObject by name from a collection of GameObjects.
        /// </summary>
        /// <param name="nameOfGameobject">The name of the GameObject to find.</param>
        /// <param name="allGameObjects">The collection of GameObjects to search through.</param>
        /// <param name="go">The found GameObject, if found.</param>
        /// <returns>True if the GameObject is found, otherwise false.</returns>
        public static bool TryFindGameObjectOfName(string nameOfGameobject, IEnumerable<GameObject> allGameObjects,
            out GameObject go)
        {
            go = null;
            foreach (GameObject gameObject in allGameObjects)
            {
                if (gameObject.name == nameOfGameobject)
                {
                    go = gameObject;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Recursively searches for a child GameObject by name.
        /// </summary>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <param name="childName">The name of the child GameObject to search for.</param>
        /// <returns>The found child GameObject, or null if not found.</returns>
        public static GameObject RecursiveFindChildGameObject(this Transform parent, string childName,
            bool caseSensitive = false)
        {
            Transform childTransform = parent.FindChildRecursively(childName, caseSensitive);
            return childTransform != null ? childTransform.gameObject : null;
        }

        /// <summary>
        ///     Recursively searches for a child Transform by name.
        /// </summary>
        /// <param name="parent">The parent GameObject to start the search from.</param>
        /// <param name="childName">The name of the child Transform to search for.</param>
        /// <returns>The found child Transform, or null if not found.</returns>
        public static Transform RecursiveFindChild(GameObject parent, string childName)
        {
            return parent.transform.FindChildRecursively(childName);
        }

        /// <summary>
        ///     Attempts to find a component of a specified type on a child GameObject with a specified name.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <param name="childName">The name of the child GameObject to search for.</param>
        /// <param name="foundChild">The found component, if found.</param>
        /// <returns>True if the component is found, otherwise false.</returns>
        public static bool TryFindComponent<T>(this Transform parent, string childName, out T foundChild)
            where T : Component
        {
            Transform transform = parent.FindChildRecursively(childName);
            if (transform != null)
            {
                foundChild = transform.GetComponent<T>();
                return foundChild != null;
            }

            foundChild = null;
            return false;
        }

        /// <summary>
        ///     Finds a child GameObject with the same name as the component variable and gets the component from it.
        ///     If the component reference is already set (not null), this method does nothing.
        /// </summary>
        /// <typeparam name="T">The type of component to get.</typeparam>
        /// <param name="transform">The transform to start the search from.</param>
        /// <param name="component">A reference to the variable where the found component will be stored.</param>
        /// <param name="componentName"></param>
        /// <remarks>
        ///     This method searches recursively through all children of the given transform.
        ///     If a matching child is found but it doesn't have the specified component, the 'component' parameter will remain
        ///     null.
        ///     This method uses reflection to get the variable name, which may have a small performance impact.
        /// </remarks>
        public static void FindChildWithComponent<T>(this Transform transform, ref T component, string componentName)
            where T : Component
        {
            if (component == null)
            {
                Transform childTransform = transform.FindChildRecursively(componentName);
                if (childTransform != null)
                {
                    component = childTransform.GetComponent<T>();
                }
            }
        }

        private static string GetCallerMemberName([CallerMemberName] string memberName = "")
        {
            return memberName;
        }

        /// <summary>
        ///     Retrieves the first child Transform with the specified name.
        /// </summary>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <param name="childName">The name of the child Transform to search for.</param>
        /// <returns>The found child Transform, or null if not found.</returns>
        public static Transform GetChildWithName(this Transform parent, string childName)
        {
            return parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.name == childName);
        }

        /// <summary>
        ///     Clears all child GameObjects of a Transform.
        /// </summary>
        /// <param name="transform">The parent Transform.</param>
        /// <returns>The parent Transform.</returns>
        public static Transform ClearChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }

            return transform;
        }

        /// <summary>
        ///     Gets a list of child GameObjects of a Transform.
        /// </summary>
        /// <param name="transform">The parent Transform.</param>
        /// <returns>A list of child GameObjects.</returns>
        public static List<GameObject> GetChildrenGameObject(this Transform transform)
        {
            return (from Transform child in transform select child.gameObject).ToList();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Retrieves an array of assets of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of assets to retrieve.</typeparam>
        /// <returns>An array of assets.</returns>
        public static T[] GetAssetArrayOfType<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets($"t: {typeof(T).Name}")
                .ToList()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToArray();
        }

        /// <summary>
        ///     Retrieves a list of assets of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of assets to retrieve.</typeparam>
        /// <returns>A list of assets.</returns>
        public static List<T> GetAssetListOfType<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets($"t: {typeof(T).Name}")
                .ToList()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToList();
        }

        /// <summary>
        ///     Gets the full path of the currently selected directory in the Unity editor.
        /// </summary>
        /// <returns>The full path of the selected directory.</returns>
        public static string GetClickedDirFullPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            return path;
        }


        /// <summary>
        /// Recursively finds all child components of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="parent">The parent Transform to start the search from.</param>
        /// <returns>A list of all components of type T found in the hierarchy.</returns>
        public static List<T> FindAllChildrenOfType<T>(this Transform parent) where T : Component
        {
            List<T> results = new List<T>();

            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    results.Add(component);
                }

                // Recursive call to check this child's children
                results.AddRange(child.FindAllChildrenOfType<T>());
            }

            return results;
        }


        /// <summary>
        ///     Retrieves a list of assets at a specified path.
        /// </summary>
        /// <typeparam name="T">The type of assets to retrieve.</typeparam>
        /// <param name="path">The path to search for assets.</param>
        /// <returns>A list of assets.</returns>
        public static List<T> GetAtPath<T>(string path)
        {
            List<object> al = new List<object>();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
            foreach (string fileName in fileEntries)
            {
                string name = Path.GetFileName(fileName);
                string localPath = Path.Combine("Assets/", $"{path}/{name}");

                Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                {
                    al.Add(t);
                }
            }

            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
            {
                result[i] = (T)al[i];
            }

            return result.ToList();
        }
#endif
    }
}