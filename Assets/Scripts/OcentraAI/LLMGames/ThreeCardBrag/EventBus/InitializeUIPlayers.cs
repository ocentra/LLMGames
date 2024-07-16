using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;
using System.Threading.Tasks;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{

    public class InitializeUIPlayers : EventArgs
    {
        public TaskCompletionSource<bool> CompletionSource { get; }
        public GameManager GameManager { get; }
        public InitializeUIPlayers(TaskCompletionSource<bool> completionSource, GameManager gameManager)
        {
            CompletionSource = completionSource;
            GameManager = gameManager;
        }
    }
}