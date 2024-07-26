using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

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


        void OnValidate()
        {
            Init();
        }
        public void Init()
        {
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
    }
}