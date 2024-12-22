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
    public class NetworkPlayerRecord : INetworkPlayerRecord
    {
        [ShowInInspector, ReadOnly] public string PlayerName { get; set; }
        [ShowInInspector, ReadOnly] public string PlayerId { get; set; }
        [ShowInInspector, ReadOnly] public Hand Hand { get; set; }
        [ShowInInspector, ReadOnly] public string FormattedHand { get; set; }
        [ShowInInspector, ReadOnly] public int HandValue { get; set; }
        [ShowInInspector, ReadOnly] public int HandRankSum { get; set; }
        [ShowInInspector, ReadOnly] public List<IBonusDetail> AppliedBonusDetails { get; set; } = new List<IBonusDetail>();


        public NetworkPlayerRecord()  
        {
            AppliedBonusDetails = new List<IBonusDetail>();
        }
        public NetworkPlayerRecord(IPlayerBase playerBase)
        {

            NetworkPlayer networkPlayer = playerBase as NetworkPlayer;

            if (networkPlayer != null)
            {
                networkPlayer.CalculateHandValue();

                PlayerName = networkPlayer.PlayerName.Value.Value;
                PlayerId = networkPlayer.PlayerId.Value.ToString();
                Hand = networkPlayer.Hand;
                FormattedHand = Hand.GetFormattedHand();
                HandValue = networkPlayer.HandValue;
                HandRankSum = networkPlayer.HandRankSum;

                if (networkPlayer.BonusDetails != null)
                {
                    foreach (BonusDetail bonusDetail in networkPlayer.BonusDetails)
                    {
                        if (!AppliedBonusDetails.Contains(bonusDetail))
                        {
                            AppliedBonusDetails.Add(bonusDetail);
                        }
                    }
                }
            }

        }
        

    }
}