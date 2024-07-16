using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
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

        private Player  Player { get; set; }
        private float Duration { get; set; }
        private float RemainingTime { get; set; }



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


        public void StartTimer(TurnInfo turnInfo)
        {
            if (Player != turnInfo.CurrentPlayer)
            {
                StopTimer();
                return;
            }
            Duration = turnInfo.Duration;
            Show(true);
            RemainingTime = turnInfo.RemainingTime;
           
        }


        public void StopTimer()
        {
            Show(false);
           
        }

 

        private void UpdateDisplay()
        {
            if (TurnCountdownText != null) TurnCountdownText.text = $"{RemainingTime:F1}";
            if (CircleImage != null) CircleImage.fillAmount = RemainingTime / Duration;
        }



        public void Show(bool show, string fromMethod = "")
        {
            UpdateDisplay();
            if (TurnCountdownText != null) TurnCountdownText.enabled = show;
            if (CircleImage != null) CircleImage.enabled = show;
            if (BackgroundImage != null) BackgroundImage.enabled = show;
            if (Image != null) Image.enabled = show;
        }


    }
}
