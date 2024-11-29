namespace OcentraAI.LLMGames.Events
{
    public interface IEventHandler
    {
        void SubscribeToEvents();
        void UnsubscribeFromEvents();

    }
}