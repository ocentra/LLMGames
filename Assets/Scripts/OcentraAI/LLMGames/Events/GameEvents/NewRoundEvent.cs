namespace OcentraAI.LLMGames.Events
{
    public class NewRoundEvent<T> : EventArgsBase
    {
        public T GameManager { get; }
        public NewRoundEvent(T gameManager)
        {
            GameManager = gameManager;
        }


    }
}