using OcentraAI.LLMGames.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class BonusRuleUI : MonoBehaviour
    {
        [Required, ShowInInspector] private TextMeshProUGUI BonusRuleName { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI BonusRuleValue { get; set; }

        void OnValidate()
        {
            Init();
        }

        void Start()
        {
            Init();
        }
        private void Init()
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