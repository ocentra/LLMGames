using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.LLMServices
{
    [CreateAssetMenu(fileName = nameof(AzureOpenAIService), menuName = "LLMGames/AzureOpenAIService")]
    [GlobalConfig("Assets/Resources/")]
    public class AzureOpenAIService : BaseLLMService<AzureOpenAIService>
    {
        public override ILLMProvider Provider { get; protected set; } = LLMProvider.AzureOpenAI;
        protected override string ProcessResponse(string jsonResponse)
        {
            OpenAIResponse response = JsonConvert.DeserializeObject<OpenAIResponse>(jsonResponse);
            return response.Choices[0].Text;
        }
    }
}