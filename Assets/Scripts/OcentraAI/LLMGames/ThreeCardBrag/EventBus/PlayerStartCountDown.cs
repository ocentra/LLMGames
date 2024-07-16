using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class PlayerStartCountDown : EventArgs
    {
        public TurnInfo TurnInfo { get; }
        public PlayerStartCountDown(TurnInfo turnInfo)
        {

            TurnInfo = turnInfo;
        }
    }
}