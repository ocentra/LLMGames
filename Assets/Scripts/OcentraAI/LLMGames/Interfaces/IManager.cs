using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace OcentraAI.LLMGames
{
    public interface IManager
    {
        UniTask WaitForInitializationAsync();
    }
}