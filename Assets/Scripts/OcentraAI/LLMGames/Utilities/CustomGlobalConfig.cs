using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace OcentraAI.LLMGames.Utilities
{
    public abstract class CustomGlobalConfig<T> : SerializedScriptableObject, ICustomGlobalConfigEvents where T : SerializedScriptableObject
    {
        private static CustomGlobalConfigAttribute configAttribute;
        private static T instance;

        public static CustomGlobalConfigAttribute ConfigAttribute
        {
            get
            {
                if (configAttribute == null)
                {
                    configAttribute = typeof(T).GetCustomAttribute<CustomGlobalConfigAttribute>() ?? new CustomGlobalConfigAttribute(TypeExtensions.GetNiceName(typeof(T)));
                }
                return configAttribute;
            }
        }
        
        public static T Instance => CustomGlobalConfigUtility<T>.GetInstance(ConfigAttribute.AssetPath);

        protected virtual void OnConfigInstanceFirstAccessed() { }

        protected virtual void OnConfigAutoCreated() { }

        void ICustomGlobalConfigEvents.OnConfigAutoCreated() => OnConfigAutoCreated();

        void ICustomGlobalConfigEvents.OnConfigInstanceFirstAccessed() => OnConfigInstanceFirstAccessed();
    }

  
}
