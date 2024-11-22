namespace OcentraAI.LLMGames.Events
{
    public class OfferContinuationEvent  : EventArgsBase
    {
        public float Delay { get; }
       

        public OfferContinuationEvent( float delay)
        {
            Delay = delay;
           
        }


    }
}