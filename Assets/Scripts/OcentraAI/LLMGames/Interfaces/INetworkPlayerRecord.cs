using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public interface INetworkPlayerRecord
    {
        public string PlayerName { get; set; }
        public string PlayerId { get; set; }
        public int HandValue { get; set; }
        public int HandRankSum { get; set; }
        string FormattedHand { get; set; }
        public List<IBonusDetail> AppliedBonusDetails { get; set; }
    }
}