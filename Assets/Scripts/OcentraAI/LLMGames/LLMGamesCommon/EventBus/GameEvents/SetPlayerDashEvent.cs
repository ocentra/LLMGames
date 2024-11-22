using System;

namespace OcentraAI.LLMGames.Events
{
    public class SetPlayerDashEvent : EventArgsBase
    {
        public int NumberOfPlayers;

        public SetPlayerDashEvent()
        {
        }

        public SetPlayerDashEvent(int numberOfPlayers)
        {
            NumberOfPlayers = numberOfPlayers;
        }
    }
}