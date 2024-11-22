using OcentraAI.LLMGames.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Scores
{
    [Serializable]
    public class RoundScore
    {
        public int Pot;
        public LLMPlayer Winner;
    }
}