using OcentraAI.LLMGames.ThreeCardBrag.Manager;
using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerStartCountDown : EventArgs
    {
        public TurnManager TurnManager { get; }
        public PlayerStartCountDown(TurnManager turnManager)
        {

            TurnManager = turnManager;
        }
    }
}