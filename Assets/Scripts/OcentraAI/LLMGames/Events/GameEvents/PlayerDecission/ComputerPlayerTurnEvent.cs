namespace OcentraAI.LLMGames.Events
{
    public class ComputerPlayerTurnEvent : EventArgsBase
    {
        public IPlayerBase CurrentPlayer { get; }

        public int CurrentBet { get; }

        public ComputerPlayerTurnEvent(IPlayerBase currentPlayer, int currentBet)
        {
            CurrentPlayer = currentPlayer;
            CurrentBet = currentBet;
        }
    }
}