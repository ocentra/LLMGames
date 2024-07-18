using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class NewGameEventArgs : EventArgs
    {
        public int InitialCoins { get; }
        public string Message { get; }

        public NewGameEventArgs(int initialCoins, string message)
        {
            InitialCoins = initialCoins;
            Message = message;
        }
    }
}