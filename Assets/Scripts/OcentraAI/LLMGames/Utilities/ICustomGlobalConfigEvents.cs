namespace OcentraAI.LLMGames.Utilities
{
    public interface ICustomGlobalConfigEvents
    {
        void OnConfigAutoCreated();
        void OnConfigInstanceFirstAccessed();
    }
}