using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateGameState : EventArgs
    {
        public GameManager GameManager { get; }
        public UpdateGameState(GameManager gameManager)
        {
            GameManager = gameManager;
        }


    }
}