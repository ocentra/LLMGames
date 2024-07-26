using OcentraAI.LLMGames.Extensions;
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

        public void SetBonus(string ruleName, string ruleValue)
        {
            BonusRuleName.text = ruleName;
            BonusRuleValue.text = ruleValue;
        }
    }
}