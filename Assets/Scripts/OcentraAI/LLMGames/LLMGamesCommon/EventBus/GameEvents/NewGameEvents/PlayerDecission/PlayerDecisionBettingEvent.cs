using Unity.Netcode;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerDecisionBettingEvent : PlayerDecisionEvent
    {
        public PlayerDecisionBettingEvent(PlayerDecision decision)
            : base(decision)
        {
        }

        public PlayerDecisionBettingEvent() { }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
        }
    }
}