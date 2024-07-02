using Newtonsoft.Json;

namespace ThreeCardBrag.LLMService
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

    public class ClaudeResponse
    {
        public string[] Responses { get; set; }
    }
    
    
}