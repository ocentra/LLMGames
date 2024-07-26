using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.LLMServices;
using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class RoundStats : MonoBehaviour
    {
        [Required, ShowInInspector] public GameObject PlayerPanel;
        [Required, ShowInInspector] public TextMeshProUGUI RoundNumber;
        [Required, ShowInInspector] public TextMeshProUGUI Pot;
        void OnValidate()
        {
            Init();
        }

        public void Init()
        {

            if (PlayerPanel == null)
            {
                PlayerPanel = Resources.Load<GameObject>($"Prefabs/{nameof(PlayerPanel)}");

            }

            if (RoundNumber == null)
            {
                RoundNumber = transform.FindChildRecursively<TextMeshProUGUI>(nameof(RoundNumber));
            }

            if (Pot == null)
            {
                Pot = transform.FindChildRecursively<TextMeshProUGUI>(nameof(Pot));

            }
        }


        public void ShowStat(RoundRecord roundRecord)
        {
            if (Pot != null)
            {
                Pot.text = $"   {nameof(Pot)} : {roundRecord.PotAmount}";

            }

            if (RoundNumber != null)
            {
                RoundNumber.text = $"   Round : {roundRecord.RoundNumber}";
            }

            foreach (Player player in roundRecord.Players)
            {
                GameObject playerGameObject = Instantiate(PlayerPanel, transform);

                PlayerPanel playerPanel = playerGameObject.GetComponent<PlayerPanel>();
               

                if (playerPanel != null)
                {
                    playerPanel.Init();

                    foreach (BaseBonusRule bonusRule in player.AppliedRules)
                    {
                        if (playerPanel.BonusRule != null && playerPanel.AppliedRules != null)
                        {
                            GameObject rule = Instantiate(playerPanel.BonusRule, playerPanel.AppliedRules);
                            BonusRuleUI bonusRuleUI = rule.GetComponent<BonusRuleUI>();
                            if (bonusRuleUI != null)
                            {
                                bonusRuleUI.Init();
                                bonusRuleUI.SetBonus(bonusRule.RuleName, bonusRule.BonusValue.ToString());

                            }
                        }
                    }

                    playerPanel.SetWinner(roundRecord.Winner == player);



                    if (playerPanel.HandValue != null)
                    {
                        playerPanel.HandValue.text = player.HandValue.ToString();
                    }

                    if (playerPanel.PlayerName != null)
                    {
                        playerPanel.PlayerName.text = player.PlayerName;
                    }

                    if (playerPanel.HandRankSum != null)
                    {
                        playerPanel.HandRankSum.text = player.HandRankSum.ToString();
                    }

                    if (playerPanel.HandView != null)
                    {
                        playerPanel.HandView.text = player.GetFormattedHand();
                    }
                }


            }
        }


    }
}
