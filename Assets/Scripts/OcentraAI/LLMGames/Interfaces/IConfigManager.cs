using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames
{
    public interface IConfigManager
    {
        UniTask FetchConfig();
        bool TryGetConfigForProvider(string provider, out ILLMConfig config);
        void UpdateApiKey(string provider, string newApiKey);

        UniTask<(bool success, ILLMConfig config)> TryAddOrUpdateConfig(ILLMConfig newConfig);

        bool ValidateConfig(ILLMConfig config);
        UniTask SaveAllConfigsToCloud();
        UniTask<bool> TryGetAllConfigsFromCloud();
    }
}