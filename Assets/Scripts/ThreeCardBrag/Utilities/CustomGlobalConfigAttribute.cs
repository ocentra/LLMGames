using System;
using System.ComponentModel;
using UnityEngine;

namespace ThreeCardBrag.Utilities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomGlobalConfigAttribute : Attribute
    {
        private readonly string assetPath;


        [Obsolete("It's a bit more complicated than that as it's not always possible to know the full path, so try and make due without it if you can, only using the AssetDatabase.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string FullPath => Application.dataPath + "/" + AssetPath;


        public string AssetPath => assetPath.Trim().TrimEnd('/', '\\').TrimStart('/', '\\').Replace('\\', '/') + "/";

        internal string AssetPathWithAssetsPrefix => AssetPath.StartsWith("Assets/") ? assetPath : "Assets/" + assetPath;

        internal string AssetPathWithoutAssetsPrefix => AssetPath.StartsWith("Assets/") ? assetPath.Substring("Assets/".Length) : assetPath;


        public string ResourcesPath
        {
            get
            {
                if (IsInResourcesFolder)
                {
                    int num = AssetPath.LastIndexOf("/resources/", StringComparison.InvariantCultureIgnoreCase);
                    if (num >= 0)
                        return AssetPath.Substring(num + "/resources/".Length);
                }
                return "";
            }
        }


        [Obsolete("This option is obsolete and will have no effect - a GlobalConfig will always have an asset generated now; use a POCO singleton or a ScriptableSingleton<T> instead. Asset-less config objects that are recreated every reload cause UnityEngine.Object leaks.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool UseAsset { get; set; }


        public bool IsInResourcesFolder => AssetPath.Contains("/resources/", StringComparison.OrdinalIgnoreCase);

        public CustomGlobalConfigAttribute()
          : this("Assets/Resources/Global Settings")
        {
        }

        public CustomGlobalConfigAttribute(string assetPath) => this.assetPath = assetPath;
    }
}