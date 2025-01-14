using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

namespace OcentraAI.LLMGames.Events
{
    public interface IUnityServicesManager : IScriptableSingletonManagerBase
    {
        IAnalyticsService AnalyticsService { get; }
        ICloudSaveService CloudSaveService { get; }
        IConfigManager ConfigManager { get; }
        IAuthenticationService AuthenticationService { get; }
        UniTask SavePlayerDataToCloud(string key, IAuthPlayerData authPlayerData);
        UniTask SavePlayerDataToCloud(IAuthPlayerData authPlayerData);
        UniTask<(bool success, IAuthPlayerData playerData)> TryGetPlayerDataFromCloud(string key);
        UniTask<(bool success, string playerName)> TryGetPlayerName(string key);
        UniTask<(bool success, string email)> TryGetPlayerEmail(string key);
        UniTask SignOut(IAuthPlayerData authPlayerData);

       
    }
}