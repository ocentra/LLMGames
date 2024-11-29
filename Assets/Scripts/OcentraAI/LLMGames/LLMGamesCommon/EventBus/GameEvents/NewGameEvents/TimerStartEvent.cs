using Cysharp.Threading.Tasks;
using System.Threading;

namespace OcentraAI.LLMGames.Events
{
    public class TimerStartEvent : EventArgsBase
    {
        public float Duration { get; }
        public UniTaskCompletionSource<bool> TimCompletionSource { get; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public int PlayerIndex { get; set; }

        public TimerStartEvent(int playerIndex, float duration, UniTaskCompletionSource<bool> timCompletionSource, CancellationTokenSource cancellationTokenSource)
        {
            PlayerIndex = playerIndex;
            Duration = duration;
            TimCompletionSource = timCompletionSource;
            CancellationTokenSource = cancellationTokenSource;
        }

    }
}