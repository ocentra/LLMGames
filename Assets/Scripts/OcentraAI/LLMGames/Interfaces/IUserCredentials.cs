namespace OcentraAI.LLMGames.Events
{
    public interface IUserCredentials
    {
        string Username { get; }
        string Password { get; }
        bool IsValid { get; }
    }
}