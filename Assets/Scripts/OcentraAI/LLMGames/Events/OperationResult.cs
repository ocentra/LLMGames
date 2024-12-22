namespace OcentraAI.LLMGames.Events
{
    public class OperationResult<T>
    {
        public OperationResult(bool isSuccess, T value, int attempts=0, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            Attempts = attempts;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }
        public T Value { get; }
        public int Attempts { get; }
        public string ErrorMessage { get; }
    }
}