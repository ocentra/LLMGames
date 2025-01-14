using OcentraAI.LLMGames.Commons;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace OcentraAI.LLMGames.GameModes
{
    [CreateAssetMenu(fileName = nameof(GameModeManager), menuName = "GameMode/GameModeManager")]
    public class GameModeManager : ScriptableSingletonBase<GameModeManager>
    {
        public List<GameMode> AllGameModes = new List<GameMode>();
        [InlineEditor] public GameInfo GameInfo;
        private ScriptableObject[] allScriptableObjects;

        protected override void OnEnable()
        {
            allScriptableObjects = Resources.LoadAll<ScriptableObject>("");

            foreach (ScriptableObject scriptableObject in allScriptableObjects)
            {
                if (scriptableObject is GameMode gameMode)
                {
                    if (!AllGameModes.Contains(gameMode))
                    {
                        AllGameModes.Add(gameMode);
                    }
                   
                }
            }
            foreach (ScriptableObject scriptableObject in allScriptableObjects)
            {
                if (scriptableObject is GameInfo info)
                {
                    GameInfo = info;
                }
            }

            base.OnEnable();
        }

        public bool TryGetGameMode(int id, out GameMode gameMode)
        {
            foreach (GameMode mode in AllGameModes)
            {
                if (id == mode.GameModeType.GenreId)
                {
                    gameMode= mode;
                    return true;
                }
            }

            gameMode = null;
            return false;
        }
    }
}