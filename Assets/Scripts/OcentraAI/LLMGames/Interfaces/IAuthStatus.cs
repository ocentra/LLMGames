namespace OcentraAI.LLMGames.Events
{
    public interface IAuthStatus
    {
        int StatusCode { get; }
        string Name { get; }
    }
}