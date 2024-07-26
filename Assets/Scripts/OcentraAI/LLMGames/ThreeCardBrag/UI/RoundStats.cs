using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.ThreeCardBrag.Events;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using OcentraAI.LLMGames.ThreeCardBrag.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class RoundStats : MonoBehaviour
    {
        [Required, ShowInInspector] private GameObject BonusRule { get; set; }

        [Required, ShowInInspector] private TextMeshProUGUI PlayerName { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI HandRankSum { get; set; }
        [Required, ShowInInspector] private TextMeshProUGUI HandValue { get; set; }
        [Required, ShowInInspector] private Transform AppliedRules { get; set; }


        [Required, ShowInInspector] private TextMeshProUGUI HandView { get; set; }

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
            AppliedRules = transform.FindChildRecursively<Transform>(nameof(AppliedRules));

            PlayerName = transform.FindChildRecursively<TextMeshProUGUI>(nameof(PlayerName));
            HandRankSum = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HandRankSum));
            HandValue = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HandValue));

            HandView = transform.FindChildRecursively<TextMeshProUGUI>(nameof(HandView));

            BonusRule = Resources.Load<GameObject>($"Prefabs/{nameof(BonusRule)}");
        }

        public void ShowStat(OfferContinuation e)
        {
            ShowStat(e.GameManager.PlayerManager);
        }

        public void ShowStat(OfferNewGame e)
        {
            ShowStat(e.GameManager.PlayerManager);
        }

        public void ShowStat(PlayerManager playerManager)
        {
            List<Player> allPlayers = playerManager.GetAllPlayers().OrderByDescending(p => p.HandValue).ToList();

            foreach (Player player in allPlayers)
            {
                foreach (BaseBonusRule bonusRule in player.AppliedRules)
                {
                    GameObject rule = Instantiate(BonusRule, AppliedRules);
                    BonusRuleUI bonusRuleUI = rule.GetComponent<BonusRuleUI>();
                    bonusRuleUI.SetBonus(bonusRule.RuleName, bonusRule.BonusValue.ToString());
                }

                HandValue.text = player.HandValue.ToString();

                PlayerName.text = player.PlayerName;

                HandRankSum.text = player.HandRankSum.ToString();

                HandView.text = player.GetFormattedHand();



            }
        }



    }
}
