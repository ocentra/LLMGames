using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames
{
    public interface IConfigManager
    {
        UniTask FetchConfig(string playerId);
        bool TryGetConfigForProvider(ILLMProvider provider, out ILLMConfig config);
        void UpdateApiKey(ILLMProvider provider, string newApiKey);

        UniTask<(bool success, ILLMConfig config)> TryAddOrUpdateConfig(ILLMConfig newConfig);

        bool ValidateConfig(ILLMConfig config);
        UniTask SaveAllConfigsToCloud();
        UniTask<bool> TryGetAllConfigsFromCloud();
    }
}