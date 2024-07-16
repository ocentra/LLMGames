using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdatePlayerHandDisplay : EventArgs
    {
        public Player Player { get; }

        public UpdatePlayerHandDisplay(Player player)
        {
            Player = player;
        }
    }
}