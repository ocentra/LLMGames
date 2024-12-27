using OcentraAI.LLMGames.Authentication;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationCompletedEvent : EventArgsBase
    {
        public IAuthPlayerData AuthPlayerData { get;  }
        public AuthenticationCompletedEvent(IAuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;
        }
    }
}