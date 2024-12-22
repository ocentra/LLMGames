namespace OcentraAI.LLMGames.Events
{
    public class NewRoundStartedEvent : EventArgsBase
    {
        public bool IsNewGame { get; }

        public NewRoundStartedEvent(bool isNewGame)
        {
            IsNewGame = isNewGame;
        }
    }
}