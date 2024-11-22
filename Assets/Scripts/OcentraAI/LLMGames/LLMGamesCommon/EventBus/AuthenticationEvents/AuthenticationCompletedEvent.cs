using OcentraAI.LLMGames.Authentication;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationCompletedEvent : EventArgsBase
    {
        public AuthPlayerData AuthPlayerData { get;  }
        public AuthenticationCompletedEvent(AuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;
        }
    }
}