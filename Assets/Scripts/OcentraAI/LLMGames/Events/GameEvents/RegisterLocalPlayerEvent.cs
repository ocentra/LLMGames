namespace OcentraAI.LLMGames.Events
{
    public class RegisterLocalPlayerEvent : EventArgsBase
    {
        public IHumanPlayerData LocalHumanPlayer { get; }

        public RegisterLocalPlayerEvent(IHumanPlayerData localHumanPlayer)
        {
            LocalHumanPlayer = localHumanPlayer;
        }
    }
}