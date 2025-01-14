using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.LLMServices
{

    [CreateAssetMenu(fileName = nameof(LocalLLMService), menuName = "LLMGames/LocalLLMService")]
    [GlobalConfig("Assets/Resources/")]
    public class LocalLLMService : BaseLLMService<LocalLLMService>
    {
        public override ILLMProvider Provider { get; protected set; } = LLMProvider.LocalLLM;
        protected override string ProcessResponse(string jsonResponse)
        {
            LocalLLMResponse response = JsonConvert.DeserializeObject<LocalLLMResponse>(jsonResponse);
            return response.Result.GeneratedText;
        }
    }
}