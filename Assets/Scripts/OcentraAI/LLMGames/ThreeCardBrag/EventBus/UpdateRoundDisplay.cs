using OcentraAI.LLMGames.ThreeCardBrag.Scores;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateRoundDisplay : EventArgs
    {
        public ScoreKeeper ScoreKeeper { get; }
        public UpdateRoundDisplay(ScoreKeeper scoreKeeper)
        {
            ScoreKeeper = scoreKeeper;
        }
    }
}