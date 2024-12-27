using System;

namespace OcentraAI.LLMGames.Events
{
    [Serializable]
    public class UsageInfo
    {
        public EventInfo Publishers;
        public EventInfo Subscribers;

        public UsageInfo()
        {
            Publishers = new EventInfo();
            Subscribers = new EventInfo();
        }
    }
}