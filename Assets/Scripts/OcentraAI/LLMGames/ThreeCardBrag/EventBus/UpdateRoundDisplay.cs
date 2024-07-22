using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateRoundDisplay : EventArgs
    {
        public ScoreManager ScoreManager { get; }
        public UpdateRoundDisplay(ScoreManager scoreManager)
        {
            ScoreManager = scoreManager;
        }
    }
}