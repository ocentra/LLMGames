using System.Collections.Generic;

namespace OcentraAI.LLMGames.Events
{
    public class OfferNewGameEvent : EventArgsBase
    {
        public float Delay { get; }
        public List<INetworkRoundRecord> RoundRecord { get; }
        public (IPlayerBase OverallWinner, int WinCount) OverallWinner { get; }
        public OfferNewGameEvent(float delay, List<INetworkRoundRecord> roundRecord, (IPlayerBase OverallWinner, int WinCount) overallWinner)
        {
            Delay = delay;
            RoundRecord = roundRecord;
            OverallWinner = overallWinner;
        }


    }
}