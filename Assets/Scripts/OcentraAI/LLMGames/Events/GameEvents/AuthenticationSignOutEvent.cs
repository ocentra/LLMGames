using OcentraAI.LLMGames.Authentication;

namespace OcentraAI.LLMGames.Events
{
    public class AuthenticationSignOutEvent : EventArgsBase
    {
        public IAuthPlayerData AuthPlayerData { get; }
        public AuthenticationSignOutEvent(IAuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;
        }
        
    }
}