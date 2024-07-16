using System.Threading.Tasks;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public static class Utility
    {
        public static async Task Delay(float seconds)
        {
            await Task.Delay((int)(seconds * 1000));
        }

        public static string GetColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
    }
}