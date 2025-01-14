using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.GameModes;
using System.Collections.Generic;

namespace OcentraAI.LLMGames
{
    public interface IManagerBase
    {
        UniTask WaitForInitializationAsync();
        UniTask InitializeAsync();
        UniTaskCompletionSource InitializationSource { get; set; }
    }

    public interface IButton3DSimple
    {

    }

    public interface IChildElement
    {
        public int GameModeTypeId { get; }
    }

}