using System;

namespace OcentraAI.LLMGames.Events
{
    public class UnRegisterPlayerEvent : EventArgsBase
    {
        public IHumanPlayerData HumanPlayerData { get; }
        public UnRegisterPlayerEvent(IHumanPlayerData humanPlayerData)
        {
            HumanPlayerData = humanPlayerData;
           
        }
    }
}