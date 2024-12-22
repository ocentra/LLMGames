namespace OcentraAI.LLMGames.Events
{
    public class DecisionTakenEvent : EventArgsBase
    {
        public IPlayerBase PlayerBase { get; }
        public bool EndTurn { get; }
        public PlayerDecision Decision { get; }
        public DecisionTakenEvent(PlayerDecision decision, IPlayerBase playerBase, bool endTurn)
        {
            PlayerBase = playerBase;
            EndTurn = endTurn;
            Decision = decision;
        }

    }
}