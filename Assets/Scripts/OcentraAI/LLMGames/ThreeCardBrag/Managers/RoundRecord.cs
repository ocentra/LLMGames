using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Scriptable;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.ThreeCardBrag.Manager
{
    [System.Serializable]
    public class RoundRecord
    {
        [ShowInInspector, ReadOnly] public int RoundNumber { get; set; }
        [ShowInInspector, ReadOnly] public List<PlayerRecord> PlayerRecords { get; set; } = new List<PlayerRecord>();
        [ShowInInspector, ReadOnly] public string WinnerId { get; set; }
        [ShowInInspector, ReadOnly] public int PotAmount { get; set; }
    }

    [System.Serializable]
    public class PlayerRecord
    {
        [ShowInInspector, ReadOnly] public string PlayerName { get; set; }
        [ShowInInspector, ReadOnly] public string PlayerId { get; set; }
        [ShowInInspector, ReadOnly] public List<Card> Hand { get; set; }
        [ShowInInspector, ReadOnly] public int HandValue { get; set; }
        [ShowInInspector, ReadOnly] public int HandRankSum { get; set; }

        [ShowInInspector, ReadOnly]
        public List<BonusDetail> AppliedBonusDetails { get; set; } = new List<BonusDetail>();

        public PlayerRecord(Player player)
        {
            PlayerName = player.PlayerName;
            PlayerId = player.Id;
            Hand = new List<Card>(player.Hand);
            HandValue = player.HandValue;
            HandRankSum = player.HandRankSum;

            foreach (BonusDetail bonusDetail in player.BonusDetails)
            {
                if (!AppliedBonusDetails.Contains(bonusDetail))
                {
                    AppliedBonusDetails.Add(bonusDetail);
                }
            }




        }


    }



}