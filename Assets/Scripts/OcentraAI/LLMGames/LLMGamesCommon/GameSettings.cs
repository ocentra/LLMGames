using OcentraAI.LLMGames.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.Scriptable.ScriptableSingletons
{
    [CreateAssetMenu(fileName = nameof(GameSettings), menuName = "LLMGames/GameSettings")]
    [GlobalConfig("Assets/Resources/")]
    public class GameSettings : CustomGlobalConfig<GameSettings>
    {
        [ShowInInspector] [Required] public bool DevModeEnabled = true;

        [ShowInInspector] [Required] public bool FileLog;

        [ShowInInspector] [Required] public bool UnityLog;

        public bool UnityLogError = false;

        [ShowInInspector] [Required] public bool UnityLogWarning;

        [Button]
        private void ResetSettings()
        {
            UnityLog = false;
            FileLog = false;
            SaveChanges();
        }
    }
}