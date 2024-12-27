using OcentraAI.LLMGames.Authentication;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class StartMainGameEvent : EventArgsBase
    {
        public IAuthPlayerData AuthPlayerData { get; }
        public StartMainGameEvent(IAuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;
        }
    }
}