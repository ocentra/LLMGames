namespace OcentraAI.LLMGames.Events
{
    public class ShowSubTabEvent : EventArgsBase
    {
        public bool Show { get; }
        public ShowSubTabEvent(bool show)
        {
            Show = show;
        }
        
    }

    public class InfoSubTabStateChangedEvent : EventArgsBase
    {
        public bool InfoSubEnabled { get; }
        public InfoSubTabStateChangedEvent(bool infoSubEnabled)
        {
            InfoSubEnabled = infoSubEnabled;
        }

    }
}