using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace OcentraAI.LLMGames.GameModes
{
    [Serializable]
    public class GameRulesContainer
    {
        [OdinSerialize] [HideLabel] public string LLM;

        [OdinSerialize] [HideLabel] public string Player;
    }
}