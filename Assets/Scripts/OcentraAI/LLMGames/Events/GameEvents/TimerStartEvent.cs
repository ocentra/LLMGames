using System.Threading;

namespace OcentraAI.LLMGames.Events
{
    public class TimerStartEvent : EventArgsBase
    {
        public float Duration { get; }
        public int PlayerIndex { get; set; }
        public ulong PlayerId { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public TimerStartEvent(ulong playerId, int playerIndex, float duration, CancellationTokenSource cancellationTokenSource)
        {
            PlayerIndex = playerIndex;
            Duration = duration;
            CancellationTokenSource = cancellationTokenSource;
            PlayerId = playerId;
        }

    }
}