using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OcentraAI.LLMGames.Utilities;

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
        private float Duration { get; set; }
        private float RemainingTime { get; set; }

        private bool isTimerRunning = false;

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
            GameLogger.Log("[UI] PlayerTimer components initialized");
        }

        public void SetPlayer(Player player)
        {
            Player = player;
            GameLogger.Log($"[UI] PlayerTimer set for player: {player.GetType().Name}");
        }

        public void StartTimer(TurnManager turnManager)
        {
            if (Player != turnManager.CurrentPlayer)
            {
                GameLogger.Log($"[UI] PlayerTimer not started. Current player ({turnManager.CurrentPlayer.PlayerName}) doesn't match this timer's player ({Player.PlayerName})");
                return;
            }
            Duration = turnManager.TurnDuration;
            RemainingTime = turnManager.RemainingTime;
            isTimerRunning = true;
            GameLogger.Log($"[UI] PlayerTimer started. Duration: {Duration}, RemainingTime: {RemainingTime}");
            Show(true);
            UpdateDisplay();
        }

        public void StopTimer(Player currentPlayer)
        {
            if (!isTimerRunning || Player != currentPlayer)
            {
                GameLogger.Log($"[UI] PlayerTimer cannot be stopped. Current player ({currentPlayer.PlayerName}) doesn't match this timer's player ({Player.PlayerName})");
                return;
            }
            isTimerRunning = false;
            GameLogger.Log($"[UI] PlayerTimer stopped for {currentPlayer.PlayerName}");
            Show(false);
        }

        private void UpdateDisplay()
        {
            if (TurnCountdownText != null)
            {
                TurnCountdownText.text = Mathf.CeilToInt(RemainingTime).ToString();
                GameLogger.Log($"[UI] PlayerTimer text updated: {TurnCountdownText.text}");
            }
            if (CircleImage != null)
            {
                CircleImage.fillAmount = RemainingTime / Duration;
                GameLogger.Log($"[UI] PlayerTimer circle fill updated: {CircleImage.fillAmount:F2}");
            }
        }

        public void Show(bool show, string fromMethod = "")
        {
            if (TurnCountdownText != null) TurnCountdownText.enabled = show;
            if (CircleImage != null) CircleImage.enabled = show;
            if (BackgroundImage != null) BackgroundImage.enabled = show;
            if (Image != null) Image.enabled = show;
            GameLogger.Log($"[UI] PlayerTimer visibility set to {show}. Called from: {(string.IsNullOrEmpty(fromMethod) ? "unknown method" : fromMethod)}");
        }
    }
}
