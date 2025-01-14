using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames
{
    public interface ILLMService
    {
        ILLMProvider Provider { get; }
        UniTask<string> GetResponseAsync(string systemMessage, string userPrompt);
        UniTask InitializeAsync(ILLMConfig config);
    }
}