using OcentraAI.LLMGames.ThreeCardBrag.Players;
using System;

namespace OcentraAI.LLMGames.ThreeCardBrag.Events
{
    public class UpdatePlayerHandDisplay : EventArgs
    {
        public Player Player { get; }
        public bool IsRoundEnd { get; set; }

        public UpdatePlayerHandDisplay(Player player,bool isRoundEnd =false)
        {
            Player = player;
            IsRoundEnd = isRoundEnd;
        }
    }
}