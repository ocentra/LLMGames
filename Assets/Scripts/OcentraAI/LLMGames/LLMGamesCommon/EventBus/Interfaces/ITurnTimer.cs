using Cysharp.Threading.Tasks;
using System.Threading;

namespace OcentraAI.LLMGames.Manager
{
    public interface ITurnTimer
    {
        void StartTimer(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource);
        void PauseTimer(CancellationTokenSource tokenSource);
        void ResumeTimer(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource);
        void StopTimer(UniTaskCompletionSource<bool> timerCompletionSource);

        
    }
}