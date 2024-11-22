using Newtonsoft.Json;

namespace OcentraAI.LLMGames.LLMServices
{
    public class OpenAIService : BaseLLMService
    {
        public OpenAIService(LLMConfig config)
            : base(config)
        {
        }

        protected override string ProcessResponse(string jsonResponse)
        {
            var response = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
            return response.Choices[0].Text;
        }
    }
}