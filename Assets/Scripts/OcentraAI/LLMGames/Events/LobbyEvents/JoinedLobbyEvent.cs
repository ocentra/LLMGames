using System;

namespace OcentraAI.LLMGames.Events
{
    public class JoinedLobbyEvent : EventArgsBase
    {
        public bool HasJoined { get; }


        public JoinedLobbyEvent(bool status = true)
        {
            HasJoined = status;

        }

    }
}