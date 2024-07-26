using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OcentraAI.LLMGames
{
    public class PlayerPanel : MonoBehaviour
    {
        [Required, ShowInInspector] public GameObject BonusRule;
        [Required, ShowInInspector] public TextMeshProUGUI PlayerName;
        [Required, ShowInInspector] public TextMeshProUGUI HandRankSum;
        [Required, ShowInInspector] public TextMeshProUGUI HandValue;
        [Required, ShowInInspector] public Transform AppliedRules;
        [Required, ShowInInspector] public TextMeshProUGUI HandView;

        [Required, ShowInInspector] public Image[] Backgrounds;

        [Required, ShowInInspector] public Color OriginalColor;
        public Color WinnerColor = Color.cyan;

        void OnValidate()
        {
            Init();
        }

        public void Init()
        {
            Backgrounds = GetComponentsInChildren<Image>();

            if (Backgrounds.Length>0)
            {
                OriginalColor = Backgrounds[0].color;
            }

            if (AppliedRules == null)
            {
                AppliedRules = transform.FindChildRecursively<Transform>(nameof(AppliedRules));

            }

            if (PlayerName == null)
            {
                PlayerName = transform.FindChildRecursively<TextMeshProUGUI>(nameof(PlayerName));

            }

            if (HandRankSum == null)
            {
                HandRankSum = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HandRankSum));

            }

            if (HandValue == null)
            {
                HandValue = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HandValue));

            }



            if (HandView == null)
            {
                HandView = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HandView));

            }

            if (BonusRule == null)
            {
                BonusRule = Resources.Load<GameObject>($"Prefabs/{nameof(BonusRule)}");

            }


        }

        public void SetWinner(bool winner)
        {
            foreach (Image image in Backgrounds)
            {

                if (image != null)
                {
                    image.color = winner ? WinnerColor : OriginalColor;
                }
            }
        }

    }
}