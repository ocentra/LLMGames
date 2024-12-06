using Unity.Netcode;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerDecisionPickAndSwapEvent : PlayerDecisionEvent
    {
        public string CardInHand;
        public string DraggedCard;

        public PlayerDecisionPickAndSwapEvent(PlayerDecision decision, string cardInHand, string draggedCard) : base(decision)
        {
            CardInHand = cardInHand;
            DraggedCard = draggedCard;
        }
        

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref CardInHand);
            serializer.SerializeValue(ref DraggedCard);
        }
    }
}