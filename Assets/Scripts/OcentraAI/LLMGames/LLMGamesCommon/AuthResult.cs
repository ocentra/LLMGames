using OcentraAI.LLMGames.Authentication;

namespace OcentraAI.LLMGames.Events
{
    public class AuthResult:IAuthResult
    {
        public IAuthStatus ResultAuthStatus { get; }
        public string Message { get; }
        public IAuthPlayerData AuthPlayerData { get; }
        private AuthResult(IAuthStatus authStatus, IAuthPlayerData authPlayerData)
        {
            ResultAuthStatus = authStatus;
            AuthPlayerData = authPlayerData;
            Message = string.Empty;
        }

        private AuthResult(IAuthStatus authStatus, string message)
        {
            ResultAuthStatus = authStatus;
            AuthPlayerData = null;
            Message = message;

        }

        public bool IsSuccess => Equals(ResultAuthStatus, AuthStatus.Success);
        public bool IsPending => Equals(ResultAuthStatus, AuthStatus.Pending);
        public bool IsAuthenticated => Equals(ResultAuthStatus, AuthStatus.Authenticated);

        public static AuthResult Authenticated(IAuthPlayerData authPlayerData) => new AuthResult(AuthStatus.Authenticated, authPlayerData);
        public static AuthResult Success(string message = null) => new AuthResult(AuthStatus.Success, message);
        public static AuthResult Failure(string message) => new AuthResult(AuthStatus.Failure, message);
        public static AuthResult Pending(string message = "Authentication is in progress...") => new AuthResult(AuthStatus.Pending, message);
    }
}