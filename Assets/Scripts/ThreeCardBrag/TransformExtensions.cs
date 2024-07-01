using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Recursively searches for a child Transform by name.
    /// </summary>
    /// <param name="parent">The parent Transform to start the search from.</param>
    /// <param name="name">The name of the child Transform to search for.</param>
    /// <returns>The found child Transform, or null if not found.</returns>
    public static Transform FindChildRecursively(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            var result = child.FindChildRecursively(name);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Recursively searches for a child component of a specified type.
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
                return component;
            var result = child.FindChildRecursively<T>();
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Recursively searches for a child component of a specified type with a specified name.
    /// </summary>
    /// <typeparam name="T">The type of component to search for.</typeparam>
    /// <param name="parent">The parent Transform to start the search from.</param>
    /// <param name="name">The name of the child Transform to search for.</param>
    /// <returns>The found component of type T with the specified name, or null if not found.</returns>
    public static T FindChildRecursively<T>(this Transform parent, string name) where T : Component
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }
            var result = child.FindChildRecursively<T>(name);
            if (result != null)
                return result;
        }
        return null;
    }


}
