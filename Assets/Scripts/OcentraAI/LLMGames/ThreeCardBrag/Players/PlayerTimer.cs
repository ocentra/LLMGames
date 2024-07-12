using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames.ThreeCardBrag.Players
{
    public class PlayerTimer : MonoBehaviour
    {
        [Required, ShowInInspector]
        private TextMeshProUGUI TurnCountdownText { get; set; }

        [Required, ShowInInspector]
        private Image CircleImage { get; set; }

        [Required, ShowInInspector]
        private Image BackgroundImage { get; set; }

        [Required, ShowInInspector]
        private Image Image { get; set; }

        private Player Player { get; set; }
        private float Duration => GameManager.Instance.TurnDuration;
        private float RemainingTime { get; set; }
        private TaskCompletionSource<bool> TimerCompletionSource { get; set; }
        private CancellationTokenSource cancellationTokenSource;

        private void OnValidate()
        {
            Init();
        }

        void Start()
        {
            Init();
        }

        private void Init()
        {
            Image = GetComponent<Image>();
            TurnCountdownText = transform.FindChildRecursively<TextMeshProUGUI>(nameof(TurnCountdownText));
            CircleImage = transform.FindChildRecursively<Image>(nameof(CircleImage));
            BackgroundImage = transform.FindChildRecursively<Image>(nameof(BackgroundImage));
        }

        public void SetPlayer(Player player)
        {
            Player = player;
        }

        public async Task StartTimer()
        {
            Show(true, nameof(StartTimer));
            RemainingTime = Duration;
            TimerCompletionSource = new TaskCompletionSource<bool>();
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (RemainingTime > 0)
                {
                    UpdateDisplay();
                    await Task.Yield(); 
                    RemainingTime = Mathf.Max(0, RemainingTime - Time.deltaTime);
                }

                UpdateDisplay();
                TimerCompletionSource.TrySetResult(true);
            }
            catch (TaskCanceledException)
            {
                // Handle cancellation
            }
        }

        public void StopTimer()
        {
            Show(false, nameof(StopTimer));
            cancellationTokenSource.Cancel();
            TimerCompletionSource?.TrySetResult(false);
            ResetTimer();
        }

        public Task<bool> WaitForCompletion()
        {
            return TimerCompletionSource?.Task ?? Task.FromResult(true);
        }

        private void UpdateDisplay()
        {
            if (TurnCountdownText != null) TurnCountdownText.text = $"{RemainingTime:F1}";
            if (CircleImage != null) CircleImage.fillAmount = RemainingTime / Duration;
        }

        private void ResetTimer()
        {
            RemainingTime = Duration;
            UpdateDisplay();
        }

        public void Show(bool show, string fromMethod)
        {
            if (TurnCountdownText != null) TurnCountdownText.enabled = show;
            if (CircleImage != null) CircleImage.enabled = show;
            if (BackgroundImage != null) BackgroundImage.enabled = show;
            if (Image != null) Image.enabled = show;
        }
    }
}
