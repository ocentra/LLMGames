using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable.ScriptableSingletons
{
    [CreateAssetMenu(fileName = nameof(GameSettings), menuName = "ThreeCardBrag/GameSettings")]
    [GlobalConfig("Assets/Resources/")]
    public class GameSettings : CustomGlobalConfig<GameSettings>
    {
        [ShowInInspector, Required]
        public bool UnityLog = false;

        [ShowInInspector, Required]
        public bool UnityLogWarning = false;

        public bool UnityLogError = false;

        [ShowInInspector, Required]
        public bool FileLog = false;

        [ShowInInspector, Required] public bool DevModeEnabled = true;

        [Button]
        private void ResetSettings()
        {
            UnityLog = false;
            FileLog = false;
            SaveChanges();
        }




    }
}