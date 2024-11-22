using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.Players;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Manager
{
    [Serializable]
    public class RoundRecord
    {
        [ShowInInspector] [ReadOnly] public int RoundNumber { get; set; }
        [ShowInInspector] [ReadOnly] public List<PlayerRecord> PlayerRecords { get; set; } = new List<PlayerRecord>();
        [ShowInInspector] [ReadOnly] public string WinnerId { get; set; }
        [ShowInInspector] [ReadOnly] public int PotAmount { get; set; }
    }

    [Serializable]
    public class PlayerRecord
    {
        public PlayerRecord(LLMPlayer llmPlayer)
        {
            PlayerName = llmPlayer.AuthPlayerData.PlayerName;
            PlayerId = llmPlayer.AuthPlayerData.PlayerID;
            Hand = llmPlayer.Hand;
            HandValue = llmPlayer.HandValue;
            HandRankSum = llmPlayer.HandRankSum;

            foreach (BonusDetail bonusDetail in llmPlayer.BonusDetails)
            {
                if (!AppliedBonusDetails.Contains(bonusDetail))
                {
                    AppliedBonusDetails.Add(bonusDetail);
                }
            }
        }

        [ShowInInspector] [ReadOnly] public string PlayerName { get; set; }
        [ShowInInspector] [ReadOnly] public string PlayerId { get; set; }
        [ShowInInspector] [ReadOnly] public Hand Hand { get; set; }
        [ShowInInspector] [ReadOnly] public int HandValue { get; set; }
        [ShowInInspector] [ReadOnly] public int HandRankSum { get; set; }

        [ShowInInspector]
        [ReadOnly]
        public List<BonusDetail> AppliedBonusDetails { get; set; } = new List<BonusDetail>();
    }
}