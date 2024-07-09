using OcentraAI.LLMGames.ThreeCardBrag.Players;

namespace OcentraAI.LLMGames.ThreeCardBrag.Scores
{
    [System.Serializable]
    public class RoundScore
    {
        public Player Winner;
        public int Pot;
    }
}