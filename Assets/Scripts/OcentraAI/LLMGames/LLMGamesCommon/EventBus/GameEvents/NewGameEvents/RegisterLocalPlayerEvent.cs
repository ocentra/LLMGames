namespace OcentraAI.LLMGames.Events
{
    public class RegisterLocalPlayerEvent : EventArgsBase
    {
        public IPlayerData LocalPlayer { get; }

        public RegisterLocalPlayerEvent(IPlayerData localPlayer)
        {
            LocalPlayer = localPlayer;
        }
    }
}