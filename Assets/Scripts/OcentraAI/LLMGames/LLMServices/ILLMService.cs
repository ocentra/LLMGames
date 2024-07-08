using System.Threading.Tasks;

namespace OcentraAI.LLMGames.LLMServices
{
    public interface ILLMService
    {
        Task<string> GetResponseAsync(string systemMessage, string userPrompt);
    }
}