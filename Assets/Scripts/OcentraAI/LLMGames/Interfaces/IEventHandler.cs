namespace OcentraAI.LLMGames.Events
{
    public interface IEventHandler
    {
        void SubscribeToEvents();
        void UnsubscribeFromEvents();
        IEventRegistrar EventRegistrar { get; set; }
    }
}