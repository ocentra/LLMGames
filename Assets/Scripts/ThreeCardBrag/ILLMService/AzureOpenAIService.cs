using Newtonsoft.Json;

namespace ThreeCardBrag.LLMService
{
    public class AzureOpenAIService : BaseLLMService
    {
        public AzureOpenAIService(LLMConfig config)
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