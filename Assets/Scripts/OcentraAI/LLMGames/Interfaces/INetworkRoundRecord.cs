
using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public interface INetworkRoundRecord
    {
        public int RoundNumber { get; set; }
        public List<INetworkPlayerRecord> PlayerRecords { get; set; }
        public int PotAmount { get; set; }
        public int MaxRounds { get; set; }
        public string Winner { get; set; }
        public ulong WinnerId { get; set; }
    }
}