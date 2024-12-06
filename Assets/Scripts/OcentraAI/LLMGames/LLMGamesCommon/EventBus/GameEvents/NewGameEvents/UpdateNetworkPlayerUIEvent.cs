namespace OcentraAI.LLMGames.Events
{
    public class UpdateNetworkPlayerUIEvent : EventArgsBase
    {
        public int Coins { get; set; }

        public UpdateNetworkPlayerUIEvent(int coins)
        {
            Coins = coins;
        }
    }
}