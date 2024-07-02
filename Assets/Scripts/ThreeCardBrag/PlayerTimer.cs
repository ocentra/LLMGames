using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ThreeCardBrag.Extensions;

namespace ThreeCardBrag
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

        private Coroutine timerCoroutine;

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

        public void StartTimer(float duration)
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
            timerCoroutine = StartCoroutine(TimerCoroutine(duration));
        }

        private IEnumerator TimerCoroutine(float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float remainingTime = duration - elapsedTime;
                if (TurnCountdownText != null) TurnCountdownText.text = $"{remainingTime}";
                if (CircleImage != null) CircleImage.fillAmount = remainingTime / duration;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (TurnCountdownText != null) TurnCountdownText.text = "0";
            if (CircleImage != null) CircleImage.fillAmount = 0f;

            timerCoroutine = null;
        }

        public void Show(bool show)
        {
            if (TurnCountdownText != null) TurnCountdownText.gameObject.SetActive(show);
            if (CircleImage != null) CircleImage.gameObject.SetActive(show);
            if (BackgroundImage != null) BackgroundImage.gameObject.SetActive(show);
            if (Image != null) Image.enabled = show;
        }
    }
}