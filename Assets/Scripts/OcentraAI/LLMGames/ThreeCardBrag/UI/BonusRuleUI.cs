using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class BonusRuleUI : MonoBehaviour
    {
        [Required, ShowInInspector] public TextMeshProUGUI BonusRuleName;
        [Required, ShowInInspector] public TextMeshProUGUI BonusRuleValue;

        void OnValidate()
        {
            Init();
        }


        public void Init()
        {
            BonusRuleName = transform.FindChildRecursively<TextMeshProUGUI>(nameof(BonusRuleName));
            BonusRuleValue = transform.FindChildRecursively<TextMeshProUGUI>(nameof(BonusRuleValue));

        }

        public void SetBonus(BonusDetail bonusDetail)
        {
            BonusRuleName.text = bonusDetail.RuleName;
            BonusRuleValue.text = bonusDetail.BonusCalculationDescriptions;
        }
    }
}