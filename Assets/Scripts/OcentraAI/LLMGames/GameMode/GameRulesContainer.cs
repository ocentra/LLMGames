using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace OcentraAI.LLMGames.GameModes
{
    [System.Serializable]
    public class GameRulesContainer
    {
        [OdinSerialize, HideLabel]
        public string Player;

        [OdinSerialize, HideLabel]
        public string LLM;
    }



}