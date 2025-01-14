using OcentraAI.LLMGames.Authentication;

namespace OcentraAI.LLMGames.Events
{
    public interface IAuthResult
    {
        IAuthStatus ResultAuthStatus { get; }
        string Message { get; }
        IAuthPlayerData AuthPlayerData { get; }
        bool IsSuccess { get; }
        bool IsPending { get; }
        bool IsAuthenticated { get; }
    }
}