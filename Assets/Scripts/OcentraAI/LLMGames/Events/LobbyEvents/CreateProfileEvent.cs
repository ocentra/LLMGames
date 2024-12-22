using OcentraAI.LLMGames.Authentication;
using System;

namespace OcentraAI.LLMGames.Events
{
    public class CreateProfileEvent : EventArgsBase
    {
        public IAuthPlayerData AuthPlayerData { get; }
        public CreateProfileEvent(IAuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;

        }

    }
}