using System;
using UnityEngine;

namespace OcentraAI.LLMGames.Utilities
{
    public static class Utility
    {
        public static string GetColor(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        public static string ColouredMessage(string message, Color color, bool withNewLine = false)
        {
            return withNewLine
                ? $"{Environment.NewLine}<color={GetColor(color)}>{message}</color>"
                : $"<color={GetColor(color)}>{message}</color>";
        }
    }
}