using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdateTurnState : EventArgs
    {
        public Player CurrentPlayer { get; }
        public bool IsHumanTurn { get; }

        public bool IsComputerTurn { get; }

        public UpdateTurnState(Player currentPlayer)
        {
            CurrentPlayer = currentPlayer;
            IsHumanTurn = CurrentPlayer is HumanPlayer;
            IsComputerTurn = CurrentPlayer is ComputerPlayer;

        }


    }
}