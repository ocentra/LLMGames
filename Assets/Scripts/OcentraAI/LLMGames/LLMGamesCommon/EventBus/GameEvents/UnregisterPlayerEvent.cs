using System;

namespace OcentraAI.LLMGames.Events
{
    public class UnRegisterPlayerEvent : EventArgsBase
    {
        public IPlayerData PlayerData { get; }
        public UnRegisterPlayerEvent(IPlayerData playerData)
        {
            PlayerData = playerData;
           
        }
    }
}