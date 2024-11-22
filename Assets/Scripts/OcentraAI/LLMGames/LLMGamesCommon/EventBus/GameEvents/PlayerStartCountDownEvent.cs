
using System;

namespace OcentraAI.LLMGames.Events
{
    public class PlayerStartCountDownEvent<TM> : EventArgsBase
    {
        public TM TurnManager { get; }
        public PlayerStartCountDownEvent(TM turnManager)
        {
            TurnManager = turnManager;
        }


    }
}