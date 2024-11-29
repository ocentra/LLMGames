using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public interface IPlayerEvent : IEventHandler
    {
        UniTask OnPlayerDecision(PlayerDecisionEvent decisionEvent);

    }
}