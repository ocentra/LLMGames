namespace OcentraAI.LLMGames.Events
{
    public class OfferContinuationEvent  : EventArgsBase
    {
        public float Delay { get; }

        public INetworkRoundRecord RoundRecord { get; }
        public OfferContinuationEvent( float delay, INetworkRoundRecord roundRecord)
        {
            Delay = delay;
            RoundRecord = roundRecord;
        }


    }
}