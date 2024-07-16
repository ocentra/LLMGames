using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    [System.Serializable]
    public class TurnInfo
    {
        public Player CurrentPlayer;
        public float ElapsedTime;
        public TaskCompletionSource<bool> ActionCompletionSource { get; set; }
        public TaskCompletionSource<bool> TimerCompletionSource { get; set; }
        public float Duration { get; private set; }
        public float RemainingTime { get; private set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }

        public TurnInfo(Player currentPlayer, float duration)
        {
            CurrentPlayer = currentPlayer;
            Duration = duration;
            ElapsedTime = 0f;
            RemainingTime = Duration;
            ActionCompletionSource = new TaskCompletionSource<bool>();
            TimerCompletionSource = new TaskCompletionSource<bool>();
            CancellationTokenSource = new CancellationTokenSource();
        }

        public void StartTurn()
        {
            ActionCompletionSource = new TaskCompletionSource<bool>();
            TimerCompletionSource = new TaskCompletionSource<bool>();
            CancellationTokenSource = new CancellationTokenSource();

            StartTimer(CurrentPlayer);
        }

        public async void StartTimer(Player currentPlayer)
        {
            PlayerStartCountDown playerStartCountDown = new PlayerStartCountDown(this);
            ElapsedTime = 0f;
            RemainingTime = Duration;
            try
            {
                while (RemainingTime > 0)
                {
                    if (CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    await Task.Yield();
                    EventBus.Publish(playerStartCountDown);
                    RemainingTime = Mathf.Max(0, RemainingTime - Time.deltaTime);
                }
                TimerCompletionSource.TrySetResult(true);
            }
            catch (Exception ex)
            {
                TimerCompletionSource.TrySetException(ex);
            }
            finally
            {
                StopTurn();
            }
        }

        public void StopTurn()
        {
            CancellationTokenSource.Cancel();
            ElapsedTime = 0f;
            RemainingTime = Duration;
            EventBus.Publish(new PlayerStopCountDown(CurrentPlayer));
        }
    }
}