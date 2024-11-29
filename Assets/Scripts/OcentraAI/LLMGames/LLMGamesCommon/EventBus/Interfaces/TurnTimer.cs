using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Threading;

namespace OcentraAI.LLMGames.Manager
{
    public class TurnTimer : ITurnTimer
    {
        private GameLoggerScriptable GameLoggerScriptable => GameLoggerScriptable.Instance;
        public float Duration { get; }
        public float RemainingTime { get; private set; }

        private TimerState State { get; set; }

        private enum TimerState
        {
            Stopped,
            Running,
            Paused
        }

        public TurnTimer(float duration)
        {
            Duration = duration;
            RemainingTime = duration;
            State = TimerState.Stopped;
        }

        public void StartTimer(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            StartTimerInternal(token, timerCompletionSource).Forget();
        }

        public void StopTimer(UniTaskCompletionSource<bool> timerCompletionSource)
        {
            StopTimerInternal(timerCompletionSource);
        }

        public void PauseTimer(CancellationTokenSource tokenSource)
        {
            PauseTimerInternal(tokenSource);
        }

        public void ResumeTimer(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            ResumeTimerInternal(token, timerCompletionSource);
        }

        private async UniTask StartTimerInternal(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            try
            {
                if (State == TimerState.Running)
                {
                    GameLoggerScriptable.LogWarning("Attempt to start an already running timer", null);
                    return;
                }

                State = TimerState.Running;
                RunTimerLoop(token, timerCompletionSource).Forget();
                await UniTask.Yield();
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error starting timer: {ex.Message}\n{ex.StackTrace}", null);
                StopTimerInternal(timerCompletionSource);
            }
        }

        private void StopTimerInternal(UniTaskCompletionSource<bool> timerCompletionSource)
        {
            if (State != TimerState.Running && State != TimerState.Paused)
            {
                GameLoggerScriptable.LogWarning("Attempt to stop a timer that is not running or paused", null);
                return;
            }

            timerCompletionSource?.TrySetResult(false);
            State = TimerState.Stopped;
            RemainingTime = Duration;
            GameLoggerScriptable.Log("Timer stopped and reset", null);
        }

        private void PauseTimerInternal(CancellationTokenSource tokenSource)
        {
            try
            {
                if (State != TimerState.Running)
                {
                    GameLoggerScriptable.LogWarning("Attempt to pause a non-running timer", null);
                    return;
                }

                tokenSource.Cancel();
                State = TimerState.Paused;
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error pausing timer: {ex.Message}\n{ex.StackTrace}", null);
            }
        }

        private void ResumeTimerInternal(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            try
            {
                if (State != TimerState.Paused)
                {
                    GameLoggerScriptable.LogWarning("Attempt to resume a non-paused timer", null);
                    return;
                }

                State = TimerState.Running;
                RunTimerLoop(token, timerCompletionSource).Forget();
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error resuming timer: {ex.Message}\n{ex.StackTrace}", null);
            }
        }

        private async UniTaskVoid RunTimerLoop(CancellationToken token, UniTaskCompletionSource<bool> timerCompletionSource)
        {
            try
            {
                int remainingSeconds = (int)Duration;

                while (remainingSeconds > 0 && !token.IsCancellationRequested)
                {
                    RemainingTime = remainingSeconds;

                    // Publish the timer update event
                    EventBus.Instance.Publish(new TimerUpdateEvent(RemainingTime));
                    GameLoggerScriptable.Log($"Timer update: {remainingSeconds}s remaining", null);

                    await UniTask.Delay(1000, cancellationToken: token);
                    remainingSeconds--;
                }

                if (remainingSeconds <= 0)
                {
                    GameLoggerScriptable.Log("Timer completed", null);
                    timerCompletionSource?.TrySetResult(true);
                    State = TimerState.Stopped;
                }
            }
            catch (OperationCanceledException)
            {
                GameLoggerScriptable.Log("Timer task was canceled", null);
                timerCompletionSource?.TrySetResult(false);
            }
            catch (Exception ex)
            {
                GameLoggerScriptable.LogError($"Error in timer loop: {ex.Message}\n{ex.StackTrace}", null);
                StopTimerInternal(timerCompletionSource);
            }



        }
      
    }
}
