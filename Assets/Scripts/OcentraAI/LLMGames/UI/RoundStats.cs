using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes.Rules;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace OcentraAI.LLMGames
{
    public class RoundStats : MonoBehaviour
    {
        [Required][ShowInInspector] public GameObject PlayerPanel;
        [Required][ShowInInspector] public TextMeshProUGUI Pot;
        [Required][ShowInInspector] public TextMeshProUGUI RoundNumber;

        private void OnValidate()
        {
            Init();
        }

        void Awake()
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


        public void ShowStat(INetworkRoundRecord roundRecord)
        {
            if (Pot != null)
            {
                Pot.text = $"   {nameof(Pot)} : {roundRecord.PotAmount}";
            }

            if (RoundNumber != null)
            {
                RoundNumber.text = $"   Round : {roundRecord.RoundNumber}";
            }

            foreach (INetworkPlayerRecord networkPlayerRecord in roundRecord.PlayerRecords)
            {
                GameObject playerGameObject = Instantiate(PlayerPanel, transform);

                PlayerPanel playerPanel = playerGameObject.GetComponent<PlayerPanel>();


                if (playerPanel != null)
                {
                    playerPanel.Init();



                    foreach (IBonusDetail bonusDetail1 in networkPlayerRecord.AppliedBonusDetails)
                    {
                        BonusDetail bonusDetail = (BonusDetail)bonusDetail1;
                        if (playerPanel.BonusRule != null && playerPanel.AppliedRules != null)
                        {
                            GameObject rule = Instantiate(playerPanel.BonusRule, playerPanel.AppliedRules);
                            BonusRuleUI bonusRuleUI = rule.GetComponent<BonusRuleUI>();
                            if (bonusRuleUI != null)
                            {
                                bonusRuleUI.Init();
                                bonusRuleUI.SetBonus(bonusDetail);
                            }
                        }
                    }

                    playerPanel.SetWinner(roundRecord.WinnerId.ToString() == networkPlayerRecord.PlayerId);


                    if (playerPanel.HandValue != null)
                    {
                        playerPanel.HandValue.text = networkPlayerRecord.HandValue.ToString();
                    }

                    if (playerPanel.PlayerName != null)
                    {
                        playerPanel.PlayerName.text = networkPlayerRecord.PlayerName;
                    }

                    if (playerPanel.HandRankSum != null)
                    {
                        playerPanel.HandRankSum.text = networkPlayerRecord.HandRankSum.ToString();
                    }

                    if (playerPanel.HandView != null)
                    {
                        playerPanel.HandView.text = networkPlayerRecord.FormattedHand;
                    }
                }
            }
        }
    }
}