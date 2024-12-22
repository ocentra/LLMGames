namespace OcentraAI.LLMGames.Events
{
    public interface ILeaderboardEntry
    {
        int Wins { get; }
        int TotalWinnings { get; }
        public void Update(int wins, int totalWinnings);
    }
}