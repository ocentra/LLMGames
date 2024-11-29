using Unity.Netcode;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerDecisionRaiseBetEvent : PlayerDecisionEvent
    {
        public float Amount;

        public PlayerDecisionRaiseBetEvent(PlayerDecision decision, float amount)
            : base(decision)
        {
            Amount = amount;
        }

        public PlayerDecisionRaiseBetEvent() { }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref Amount);
        }
    }
}