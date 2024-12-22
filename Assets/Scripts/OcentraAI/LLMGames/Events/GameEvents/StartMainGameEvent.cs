using OcentraAI.LLMGames.Authentication;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class StartMainGameEvent : EventArgsBase
    {
        public AuthPlayerData AuthPlayerData { get; }
        public StartMainGameEvent(AuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;
        }
    }
}