using Sirenix.OdinInspector;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    [System.Serializable]
    public class Info
    {
        public bool Edit = false;
        [TextArea(5, 15), RichText(nameof(Edit)), PropertyOrder(-1)]
        public string Value;

        public Info(string value)
        {
            Value = value;
        }
    }
}