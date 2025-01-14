using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

namespace OcentraAI.LLMGames.LLMServices
{
    [CreateAssetMenu(fileName = nameof(ClaudeService), menuName = "LLMGames/ClaudeService")]
    [GlobalConfig("Assets/Resources/")]
    public class ClaudeService : BaseLLMService<ClaudeService>
    {
        public override ILLMProvider Provider { get; protected set; } = LLMProvider.Claude;
        protected override string ProcessResponse(string jsonResponse)
        {
            ClaudeResponse response = JsonConvert.DeserializeObject<ClaudeResponse>(jsonResponse);
            return response.Responses[0];
        }
    }
}