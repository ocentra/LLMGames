using System;

namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationStatusEvent : EventArgsBase
    {
        public AuthResult Result { get; }

        public AuthenticationStatusEvent(AuthResult result)
        {
            Result = result;
        }
    }
}