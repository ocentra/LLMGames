using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.GameModes.Rules;
using OcentraAI.LLMGames.GamesNetworking;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    public class NetworkPlayerRecord: INetworkPlayerRecord
    {
        public NetworkPlayerRecord(IPlayerBase playerBase)
        {
            PlayerBase = playerBase;
            NetworkPlayer = PlayerBase as NetworkPlayer;
            
            if (NetworkPlayer != null)
            {
                PlayerId = NetworkPlayer.PlayerId.Value.ToString();
                Hand = NetworkPlayer.Hand;

                FormattedHand = Hand.GetFormattedHand();
                HandValue = NetworkPlayer.HandValue;
                HandRankSum = NetworkPlayer.HandRankSum;

                foreach (BonusDetail bonusDetail in NetworkPlayer.BonusDetails)
                {
                    if (!AppliedBonusDetails.Contains(bonusDetail))
                    {
                        AppliedBonusDetails.Add(bonusDetail);
                    }
                }
            }

        }

        [ShowInInspector][ReadOnly] public IPlayerBase PlayerBase { get; set; }
        [ShowInInspector][ReadOnly] public NetworkPlayer NetworkPlayer { get; set; }
        [ShowInInspector][ReadOnly] public string PlayerName { get; set; }
        [ShowInInspector][ReadOnly] public string PlayerId { get; set; }
        [ShowInInspector][ReadOnly] public Hand Hand { get; set; }
        [ShowInInspector][ReadOnly] public string FormattedHand { get; set; }
        [ShowInInspector][ReadOnly] public int HandValue { get; set; }
        [ShowInInspector][ReadOnly] public int HandRankSum { get; set; }

        [ShowInInspector]
        [ReadOnly]
        public List<BonusDetail> AppliedBonusDetails { get; set; } = new List<BonusDetail>();
    }
}