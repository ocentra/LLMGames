using OcentraAI.LLMGames.Authentication;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class CreateProfileEvent : EventArgsBase
    {
        public AuthPlayerData AuthPlayerData { get; }
        public CreateProfileEvent(AuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;

        }

    }
}