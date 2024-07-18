using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateGameState : EventArgs
    {
        public bool IsNewRound { get; } = false;


        public GameManager GameManager { get; }
        public UpdateGameState(GameManager gameManager, bool isNewRound =false)
        {
            GameManager = gameManager;
        }


    }
}