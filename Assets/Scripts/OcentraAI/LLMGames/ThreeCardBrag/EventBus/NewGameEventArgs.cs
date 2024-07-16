using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class NewGameEventArgs : EventArgs
    {
        public int InitialCoins { get; }

        public NewGameEventArgs(int initialCoins)
        {
            InitialCoins = initialCoins;
        }
    }
}