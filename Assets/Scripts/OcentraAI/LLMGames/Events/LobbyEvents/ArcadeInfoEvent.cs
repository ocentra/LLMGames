namespace OcentraAI.LLMGames.Events
{
    public class ArcadeInfoEvent : EventArgsBase
    {
        public string Info { get; }
        public ArcadeInfoEvent(string info)
        {
            Info = info;
        }

    }
}