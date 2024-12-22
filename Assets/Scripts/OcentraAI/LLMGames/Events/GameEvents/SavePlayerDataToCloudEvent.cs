using OcentraAI.LLMGames.Authentication;

namespace OcentraAI.LLMGames.Events
{
    public class SavePlayerDataToCloudEvent : EventArgsBase
    {
        public IAuthPlayerData AuthPlayerData { get; }
        public SavePlayerDataToCloudEvent(IAuthPlayerData authPlayerData)
        {
            AuthPlayerData = authPlayerData;
           
        }
    }
}