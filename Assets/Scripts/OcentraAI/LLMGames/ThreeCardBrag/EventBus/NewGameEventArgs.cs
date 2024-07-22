using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class NewGameEventArgs : EventArgs
    {
        public int InitialCoins { get; }
        public string Message { get; }
        public GameManager GameManager { get; }
        public NewGameEventArgs(GameManager gameManager, string message)
        {
            GameManager = gameManager;
            Message = message;
            InitialCoins = GameManager.ScoreManager.InitialCoins;
        }
    }
}