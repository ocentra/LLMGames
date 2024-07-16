using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerStopCountDown : EventArgs
    {
        public Player CurrentPlayer { get; }

        public PlayerStopCountDown(Player currentPlayer)
        {
            CurrentPlayer = currentPlayer;
        }
    }
}