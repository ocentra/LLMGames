using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.LLMServices
{
    public interface ILLMService
    {
        UniTask<string> GetResponseAsync(string systemMessage, string userPrompt);
    }
}