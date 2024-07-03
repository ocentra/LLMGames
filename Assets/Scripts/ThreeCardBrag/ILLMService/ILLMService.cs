using System.Threading.Tasks;

namespace ThreeCardBrag.LLMService
{
    public interface ILLMService
    {
        Task<string> GetResponseAsync(string systemMessage, string userPrompt);
    }
}