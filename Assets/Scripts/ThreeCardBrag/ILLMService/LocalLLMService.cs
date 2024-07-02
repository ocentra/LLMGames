using Newtonsoft.Json;

namespace ThreeCardBrag.LLMService
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
            return response.result.generated_text;
        }



    }

    public class LocalLLMResponse
    {
        public Result result { get; set; }
    }

    public class Result
    {
        public string generated_text { get; set; }
    }
}
