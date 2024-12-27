namespace OcentraAI.LLMGames
{
    public interface IOperationResult<out T>
    {
        bool IsSuccess { get; }
        T Value { get; }
        string ErrorMessage { get; }
        int Attempts { get; }
    }



}