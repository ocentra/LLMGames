using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.LLMServices
{
    [CreateAssetMenu(fileName = nameof(OpenAIService), menuName = "LLMGames/OpenAIService")]
    [GlobalConfig("Assets/Resources/")]
    public class OpenAIService : BaseLLMService<OpenAIService>
    {
        public override ILLMProvider Provider { get; protected set; } = LLMProvider.OpenAI;
        protected override string ProcessResponse(string jsonResponse)
        {
            OpenAIResponse response = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
            return response.Choices[0].Text;
        }
    }
}