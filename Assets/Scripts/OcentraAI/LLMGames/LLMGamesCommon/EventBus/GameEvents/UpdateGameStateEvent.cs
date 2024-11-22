
using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateGameStateEvent<T> : EventArgsBase
    {
        public T GameManager { get; }
        public UpdateGameStateEvent(T gameManager)
        {
            GameManager = gameManager;
        }


    }
}