using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Authentication;
using System;
using System.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public interface IAuthenticationManager
    {
        bool UseAnonymousSignInFromScene { get; }
        bool IsGuest { get; }
        bool IsSigningIn { get; }
        bool IsLoggedIn { get; }
        IAuthPlayerData AuthPlayerData { get; }

        UniTask<IAuthResult> PerformAuthenticationAsync(Func<string, string, Task> authOperation, string username, string password, int maxRetries = 2);
        UniTask<(bool success, IAuthPlayerData playerData)> GetOrCreatePlayerDataAndUpdate(bool isGuest, string playerId);
        UniTask UpdatePlayerNameAsync(string username);
        UniTask SignOut(IAuthPlayerData authPlayerData);
        UniTask SignInAnonymouslyAsync();
        UniTask<IAuthResult> StartSignInUsingUnityAsync();
        UniTask<IAuthResult> SignInWithUnityAsync();
        UniTask<bool> LinkWithUnityAsync();
    }
}