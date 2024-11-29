namespace OcentraAI.LLMGames.Events
{
    public interface IComputerPlayerData :IPlayerBase
    {
        int DifficultyLevel { get; set; }
        string AIModelName { get; set; }
    }
}