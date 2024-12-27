using UnityEngine;

namespace OcentraAI.LLMGames.Events
{
    public class OperationResult<T> : IOperationResult<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public int Attempts { get; }
        public string ErrorMessage { get; }
        public OperationResult(bool isSuccess, T value, int attempts = 0, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            Attempts = attempts;
            ErrorMessage = errorMessage;
        }

        public static OperationResult<T> Success(T value)
        {
            return new OperationResult<T>(true, value);
        }

        public static OperationResult<T> Failure(string errorMessage)
        {
            Debug.LogError(errorMessage);
            return new OperationResult<T>(false, default, errorMessage: errorMessage);
        }
    }

}