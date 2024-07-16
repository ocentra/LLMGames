using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerActionContinueGame : EventArgs
    {
        public bool ShouldContinue { get; }
        public PlayerActionContinueGame(bool continueGame)
        {
            ShouldContinue = continueGame;
        }
    }
}