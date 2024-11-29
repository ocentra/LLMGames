using Unity.Netcode;
using Unity.Collections;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerDecisionEvent : EventArgsBase, INetworkSerializable
    {
        public PlayerDecision Decision;
       

        public PlayerDecisionEvent(PlayerDecision decision)
        {
            Decision = decision;
           
        }

        public PlayerDecisionEvent() { }

        public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int decisionId = Decision?.DecisionId ?? 0;
            serializer.SerializeValue(ref decisionId);

            if (serializer.IsReader)
            {
                Decision = PlayerDecision.FromId(decisionId);
            }
        }
    }
}