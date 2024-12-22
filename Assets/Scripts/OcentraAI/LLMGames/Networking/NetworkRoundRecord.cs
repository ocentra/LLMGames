using OcentraAI.LLMGames.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Networking.Manager
{
    [Serializable]
    public class NetworkRoundRecord: INetworkRoundRecord
    {
        [ShowInInspector, ReadOnly] public int RoundNumber { get; set; }
        [ShowInInspector, ReadOnly] public List<INetworkPlayerRecord> PlayerRecords { get; set; } = new List<INetworkPlayerRecord>();
        [ShowInInspector, ReadOnly] public int PotAmount { get; set; }
        [ShowInInspector, ReadOnly] public int MaxRounds { get; set; }
        [ShowInInspector, ReadOnly] public string Winner { get; set; }
        [ShowInInspector, ReadOnly] public ulong WinnerId { get; set; }
       
    }
}