using Unity.Netcode;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerDecisionUIEvent : PlayerDecisionEvent
    {
        public PlayerDecisionUIEvent(PlayerDecision decision)
            : base(decision)
        {
        }

        public PlayerDecisionUIEvent() { }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
        }
    }
}