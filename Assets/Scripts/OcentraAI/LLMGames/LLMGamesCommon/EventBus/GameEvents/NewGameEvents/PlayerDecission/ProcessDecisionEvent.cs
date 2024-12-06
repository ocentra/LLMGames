
namespace OcentraAI.LLMGames.Events
{
    public class ProcessDecisionEvent : EventArgsBase
    {
        public PlayerDecisionEvent DecisionEvent { get; }
        public ulong PlayerId { get; }

        public ProcessDecisionEvent(PlayerDecisionEvent decisionEvent, ulong playerId)
        {
            DecisionEvent = decisionEvent;
            PlayerId = playerId;
        }
    }
}