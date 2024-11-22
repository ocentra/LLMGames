namespace OcentraAI.LLMGames.Events
{
    public class OfferNewGameEvent : EventArgsBase
    {
        public float Delay { get; }

        public OfferNewGameEvent(float delay)
        {
            Delay = delay;

        }


    }
}