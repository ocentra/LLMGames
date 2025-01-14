using System;

namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationStatusEvent : EventArgsBase
    {
        public IAuthResult Result { get; }

        public AuthenticationStatusEvent(IAuthResult result)
        {
            Result = result;
        }
    }
}