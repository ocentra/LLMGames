using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames
{
    public interface IApplicationQuitter
    {
        UniTask<bool> ApplicationWantsToQuit();
    }
}