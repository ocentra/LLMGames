namespace OcentraAI.LLMGames.Events
{
    public class NewGameEvent<T> : EventArgsBase
    {
        public int InitialCoins { get; }
        public string Message { get; }
        public T GameManager { get; }
        public NewGameEvent(T gameManager, string message, int initialCoins)
        {
            GameManager = gameManager;
            Message = message;
            InitialCoins = initialCoins;
        }


    }
}