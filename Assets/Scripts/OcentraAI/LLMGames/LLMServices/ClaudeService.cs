using Newtonsoft.Json;

namespace OcentraAI.LLMGames.LLMServices
{
    public class ClaudeService : BaseLLMService
    {
        public ClaudeService(LLMConfig config)
            : base(config)
        {
        }

        protected override string ProcessResponse(string jsonResponse)
        {
            var response = JsonConvert.DeserializeObject<ClaudeResponse>(jsonResponse);
            return response.Responses[0];
        }
    }
}