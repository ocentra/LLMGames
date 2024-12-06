using Unity.Netcode;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerDecisionWildcardEvent : PlayerDecisionEvent
    {
        public PlayerDecisionWildcardEvent(PlayerDecision decision)
            : base(decision)
        {
        }

        public PlayerDecisionWildcardEvent() { }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
        }
    }
}