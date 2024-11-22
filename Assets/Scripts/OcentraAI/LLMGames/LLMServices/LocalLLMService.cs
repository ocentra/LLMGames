using Newtonsoft.Json;

namespace OcentraAI.LLMGames.LLMServices
{
    public class LocalLLMService : BaseLLMService
    {
        public LocalLLMService(LLMConfig config)
            : base(config)
        {
        }

        protected override string ProcessResponse(string jsonResponse)
        {
            var response = JsonConvert.DeserializeObject<LocalLLMResponse>(jsonResponse);
            return response.Result.GeneratedText;
        }
    }
}