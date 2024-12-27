using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames
{
    public interface IMonoBehaviourBase
    {
        UniTask WaitForInitializationAsync();
        UniTask InitializeAsync();
        UniTaskCompletionSource InitializationSource { get; set; }
    }
}