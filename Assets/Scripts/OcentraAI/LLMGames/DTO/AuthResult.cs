using OcentraAI.LLMGames.Authentication;

namespace OcentraAI.LLMGames.Events
{
    public class AuthResult
    {
        public enum Status
        {
            Success,
            Failure,
            Pending,
            Authenticated
        }

        public Status ResultStatus { get; }
        public string Message { get; }
        public AuthPlayerData AuthPlayerData { get; }
        private AuthResult(Status status, AuthPlayerData authPlayerData)
        {
            ResultStatus = status;
            AuthPlayerData = authPlayerData;
            Message = string.Empty;
        }

        private AuthResult(Status status, string message)
        {
            ResultStatus = status;
            AuthPlayerData = null;
            Message = message;

        }

        public bool IsSuccess => ResultStatus == Status.Success;
        public bool IsPending => ResultStatus == Status.Pending;
        public bool IsAuthenticated => ResultStatus == Status.Authenticated;

        public static AuthResult Authenticated(AuthPlayerData authPlayerData) => new AuthResult(Status.Authenticated, authPlayerData);
        public static AuthResult Success(string message = null) => new AuthResult(Status.Success, message);
        public static AuthResult Failure(string message) => new AuthResult(Status.Failure, message);
        public static AuthResult Pending(string message = "Authentication is in progress...") => new AuthResult(Status.Pending, message);
    }
}