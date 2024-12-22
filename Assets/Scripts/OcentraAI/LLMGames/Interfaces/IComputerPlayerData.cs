using Cysharp.Threading.Tasks;

namespace OcentraAI.LLMGames.Events
{
    public interface IComputerPlayerData :IPlayerBase
    {
        int DifficultyLevel { get; set; }
        string AIModelName { get; set; }
        UniTask<bool> SimulateComputerPlayerTurn(ulong currentPlayerID, int currentBet);
    }
}