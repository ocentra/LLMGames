using OcentraAI.LLMGames.Scriptable;
using System;
using System.Threading;
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

        public static string ColouredMessage(string message, Color color, bool withNewLine = false)
        {
            return withNewLine ? $"{Environment.NewLine}<color={GetColor(color)}>{message}</color>" : $"<color={GetColor(color)}>{message}</color>";
        }





        public static async Task DelayWithCancellation(CancellationTokenSource cancellationTokenSource, int millisecondsDelay)
        {
            if (cancellationTokenSource is { IsCancellationRequested: false })
            {
                await Task.Delay(millisecondsDelay, cancellationTokenSource.Token);
            }
            else
            {
                await Task.Delay(millisecondsDelay);
            }
        }
    }
}