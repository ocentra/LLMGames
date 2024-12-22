
using System;

namespace OcentraAI.LLMGames.Events
{
    public class UpdateRoundDisplayEvent<T> : EventArgsBase
    {
        public T ScoreManager { get; }
        public UpdateRoundDisplayEvent(T scoreManager)
        {
            ScoreManager = scoreManager;
        }


    }
}