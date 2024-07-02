using Newtonsoft.Json;

namespace ThreeCardBrag.LLMService
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
    public class OpenAIResponse
    {
        public Choice[] Choices { get; set; }
    }

    public class Choice
    {
        public string Text { get; set; }
    }
}

